using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCPServer.Services;

namespace MCPServer.Gateway;

/// <summary>
/// Gateway service that routes tool calls to downstream MCP servers using JSON-RPC protocol
/// </summary>
public class McpGatewayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly GatewayLoggingService _loggingService;
    private readonly ILogger<McpGatewayService> _logger;
    private readonly OboTokenService _oboTokenService;
    
    private readonly List<DownstreamMcpServer> _downstreamServers = new();
    private readonly Dictionary<string, DownstreamMcpServer> _toolRegistry = new();

    public McpGatewayService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        GatewayLoggingService loggingService,
        ILogger<McpGatewayService> logger,
        OboTokenService oboTokenService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _loggingService = loggingService;
        _logger = logger;
        _oboTokenService = oboTokenService;
    }

    /// <summary>
    /// Initialize gateway and build tool registry from configuration
    /// </summary>
    public Task InitializeAsync()
    {
        // Load downstream server configurations
        var serversConfig = _configuration.GetSection("McpGateway:DownstreamServers");
        _downstreamServers.Clear();
        _downstreamServers.AddRange(serversConfig.Get<List<DownstreamMcpServer>>() ?? new());

        // Build tool registry from configured mappings
        _toolRegistry.Clear();
        foreach (var server in _downstreamServers.Where(s => s.IsEnabled))
        {
            foreach (var toolName in server.ToolNames)
            {
                if (_toolRegistry.ContainsKey(toolName))
                {
                    _logger.LogWarning(
                        "[Gateway] Tool name conflict: {ToolName} exists in both {Server1} and {Server2}",
                        toolName, _toolRegistry[toolName].Name, server.Name);
                }
                else
                {
                    _toolRegistry[toolName] = server;
                    _logger.LogInformation("[Gateway] Registered tool {ToolName} → {ServerName}", toolName, server.Name);
                }
            }
        }

        _logger.LogInformation("[Gateway] Initialized with {ServerCount} servers and {ToolCount} tools", 
            _downstreamServers.Count, _toolRegistry.Count);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Call a tool by routing to the appropriate downstream server
    /// </summary>
    public async Task<object> CallToolAsync(string toolName, object arguments, string? authToken = null)
    {
        // Trim whitespace from tool name
        toolName = toolName?.Trim() ?? string.Empty;
        
        // Find which server owns this tool
        if (!_toolRegistry.TryGetValue(toolName, out var targetServer))
        {
            throw new InvalidOperationException($"Tool '{toolName}' not found in gateway registry");
        }

        // Start logging
        var requestId = _loggingService.StartToolCall(toolName, targetServer.Name, arguments);

        try
        {
            var result = await InvokeToolOnServerAsync(targetServer, toolName, arguments, authToken);
            _loggingService.LogToolSuccess(requestId, toolName, targetServer.Name, result, 200);
            return result;
        }
        catch (HttpRequestException ex)
        {
            var statusCode = (int?)ex.StatusCode ?? 500;
            _loggingService.LogToolError(requestId, toolName, targetServer.Name, ex, statusCode);
            throw;
        }
        catch (Exception ex)
        {
            _loggingService.LogToolError(requestId, toolName, targetServer.Name, ex);
            throw;
        }
    }

    /// <summary>
    /// Invoke a tool on a specific downstream server using MCP JSON-RPC protocol
    /// </summary>
    private async Task<object> InvokeToolOnServerAsync(
        DownstreamMcpServer server, 
        string toolName, 
        object arguments,
        string? authToken = null)
    {
        var requestId = Guid.NewGuid().ToString();
        var mcpUrl = server.GetMcpUrl();
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var networkStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 🚀 START LOG - Request initiated
        _logger.LogInformation(
            "[Gateway Request START] RequestId={RequestId} | Tool={ToolName} | Server={ServerName} | Url={Url} | Method=tools/call | Arguments={Arguments}",
            requestId, toolName, server.Name, mcpUrl, JsonSerializer.Serialize(arguments));

        networkStopwatch.Stop(); // Stop for overhead calculation

        var httpClient = _httpClientFactory.CreateClient();
        
        // Add MCP Streamable HTTP headers - accept both JSON and SSE
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
        
        // 🔐 OBO TOKEN ACQUISITION - Check if OBO is enabled for this server
        var serverConfig = _configuration
            .GetSection("McpGateway:DownstreamServers")
            .Get<List<DownstreamServerConfig>>()?
            .FirstOrDefault(s => s.Name == server.Name);

        if (serverConfig?.OboConfig?.Enabled == true)
        {
            _logger.LogInformation(
                "[Gateway] OBO authentication enabled for {ServerName} | UseManagedIdentity={UseMI}",
                server.Name,
                serverConfig.OboConfig.UseManagedIdentity);
                
            try
            {
                // Get incoming token from the original request
                var incomingToken = _oboTokenService.GetIncomingToken();
                if (string.IsNullOrEmpty(incomingToken))
                {
                    _logger.LogError(
                        "[Gateway] ❌ No incoming token found for {ServerName}. OBO is enabled but cannot proceed.",
                        server.Name);
                    _logger.LogWarning(
                        "[OBO] No incoming token found for {ServerName}. OBO is enabled but cannot proceed without token.",
                        server.Name);
                }
                else
                {
                    _logger.LogInformation(
                        "[Gateway] ✅ Incoming token extracted | Length={Length}",
                        incomingToken.Length);
                    
                    // Acquire OBO token for downstream server
                    var oboToken = await _oboTokenService.GetAccessTokenAsync(server.Name, incomingToken);
                    
                    // Validate the OBO token before sending
                    if (string.IsNullOrWhiteSpace(oboToken))
                    {
                        _logger.LogError(
                            "[Gateway] ❌ OBO token is null or empty for {ServerName}. Cannot proceed with request.",
                            server.Name);
                        throw new InvalidOperationException(
                            $"Failed to acquire valid OBO token for {server.Name}. Token is null or empty.");
                    }
                    
                    // Basic JWT format validation (should have 3 parts separated by dots)
                    var tokenParts = oboToken.Split('.');
                    if (tokenParts.Length != 3)
                    {
                        _logger.LogError(
                            "[Gateway] ❌ OBO token is not a valid JWT format for {ServerName} | Parts={Parts} | Token={Token}",
                            server.Name,
                            tokenParts.Length,
                            oboToken);
                        throw new InvalidOperationException(
                            $"OBO token for {server.Name} is not in valid JWT format. Expected 3 parts, got {tokenParts.Length}.");
                    }
                    
                    _logger.LogInformation(
                        "[Gateway] ✅ OBO token validated | Parts={Parts} | Length={Length}",
                        tokenParts.Length,
                        oboToken.Length);
                    
                    // Add Authorization header with OBO token
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", oboToken);
                    
                    // ⚠️ SECURITY WARNING: Full token logging enabled for debugging - REMOVE IN PRODUCTION
                    _logger.LogWarning(
                        "[OBO] 🔓 SENDING OBO TOKEN to {ServerName} (FULL): {Token}",
                        server.Name,
                        oboToken);
                    
                    _logger.LogInformation(
                        "[OBO] Added OBO token to request for {ServerName}",
                        server.Name);
                    
                    _logger.LogInformation(
                        "[Gateway] ✅ Authorization header set with OBO token for {ServerName}",
                        server.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[OBO] ❌ Failed to acquire OBO token for {ServerName} | ExceptionType={ExceptionType} | Message={Message}",
                    server.Name,
                    ex.GetType().Name,
                    ex.Message);
                
                // STOP HERE - Don't send request with invalid/missing OBO token
                _logger.LogError(
                    "[Gateway] ❌ Aborting request to {ServerName} - OBO token acquisition failed",
                    server.Name);
                throw; // Re-throw to prevent sending request without valid auth
            }
        }
        else if (!string.IsNullOrEmpty(authToken))
        {
            // Fallback: Use provided authToken if no OBO
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            
            _logger.LogInformation(
                "[Gateway] Using provided auth token for {ServerName} (OBO not enabled)",
                server.Name);
        }
        else
        {
            _logger.LogInformation(
                "[Gateway] No authentication configured for {ServerName}",
                server.Name);
        }
        
        // Add custom headers
        foreach (var header in server.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        networkStopwatch.Start(); // Restart for network timing

        _logger.LogInformation(
            "[Gateway] Creating MCP request | Tool={ToolName} | Method=tools/call",
            toolName);
        var mcpRequest = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments = arguments
            }
        };

        var requestJson = JsonSerializer.Serialize(mcpRequest);

        var content = new StringContent(
            requestJson,
            System.Text.Encoding.UTF8,
            "application/json");

        // Start network timer
        networkStopwatch.Restart();
        var response = await httpClient.PostAsync(mcpUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        networkStopwatch.Stop();
        
        var networkDuration = networkStopwatch.ElapsedMilliseconds;
        
        // Parse SSE format if response starts with "event:"
        string jsonResponse = responseBody;
        if (responseBody.StartsWith("event:"))
        {
            var lines = responseBody.Split('\n');
            var dataLine = lines.FirstOrDefault(l => l.StartsWith("data:"));
            if (dataLine != null)
            {
                jsonResponse = dataLine.Substring(5).Trim();
            }
        }

        var mcpResponse = JsonSerializer.Deserialize<McpJsonRpcResponse>(jsonResponse);
        
        totalStopwatch.Stop();
        var totalDuration = totalStopwatch.ElapsedMilliseconds;
        var gatewayOverhead = totalDuration - networkDuration;
        
        // Log overhead for performance tracking
        _loggingService.LogGatewayOverhead(gatewayOverhead);
        
        // ✅ END LOG - Response received
        _logger.LogInformation(
            "[Gateway Request END] RequestId={RequestId} | Tool={ToolName} | Server={ServerName} | Url={Url} | StatusCode={StatusCode} | TotalDuration={TotalDuration}ms | NetworkDuration={NetworkDuration}ms | GatewayOverhead={GatewayOverhead}ms | HasError={HasError} | ResponseLength={ResponseLength}",
            requestId, toolName, server.Name, mcpUrl, (int)response.StatusCode, totalDuration, networkDuration, gatewayOverhead, mcpResponse?.Error != null, responseBody.Length);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Response: {responseBody}");
        }
        
        if (mcpResponse?.Error != null)
        {
            return new
            {
                error = true,
                errorCode = mcpResponse.Error.Code,
                errorMessage = mcpResponse.Error.Message,
                server = server.Name,
                tool = toolName
            };
        }

        return mcpResponse?.Result ?? new McpResult { Content = new List<McpContent>() };
    }

    // Helper class for MCP JSON-RPC response
    private class McpJsonRpcResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string? Jsonrpc { get; set; }
        
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("result")]
        public McpResult? Result { get; set; }
        
        [JsonPropertyName("error")]
        public McpError? Error { get; set; }
    }

    private class McpResult
    {
        [JsonPropertyName("content")]
        public List<McpContent>? Content { get; set; }
    }

    private class McpContent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class McpError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
}
