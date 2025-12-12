using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    
    private readonly List<DownstreamMcpServer> _downstreamServers = new();
    private readonly Dictionary<string, DownstreamMcpServer> _toolRegistry = new();

    public McpGatewayService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        GatewayLoggingService loggingService,
        ILogger<McpGatewayService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _loggingService = loggingService;
        _logger = logger;
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
        var httpClient = _httpClientFactory.CreateClient();
        
        // Add MCP Streamable HTTP headers - accept both JSON and SSE
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
        
        // Add custom headers
        foreach (var header in server.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Add auth token if provided
        /*
        if (!string.IsNullOrEmpty(authToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        }
*/
        var mcpUrl = server.GetMcpUrl();
        
        _logger.LogDebug("[Gateway] Calling {ToolName} on {ServerName} at {Url}", 
            toolName, server.Name, mcpUrl);

        // Build MCP JSON-RPC request for tools/call
        var mcpRequest = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments = arguments
            }
        };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(mcpRequest);
        _logger.LogInformation("[Gateway] Request payload: {Payload}", requestJson);

        var content = new StringContent(
            requestJson,
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(mcpUrl, content);
        
        // Log response details before checking status
        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("[Gateway] Response status: {StatusCode}, Body: {Body}", 
            response.StatusCode, responseBody);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Response: {responseBody}");
        }

        // Parse SSE format if response starts with "event:"
        string jsonResponse = responseBody;
        if (responseBody.StartsWith("event:"))
        {
            // Extract JSON from SSE format: "event: message\ndata: {json}\n\n"
            var lines = responseBody.Split('\n');
            var dataLine = lines.FirstOrDefault(l => l.StartsWith("data:"));
            if (dataLine != null)
            {
                jsonResponse = dataLine.Substring(5).Trim(); // Remove "data:" prefix
                _logger.LogInformation("[Gateway] Extracted JSON from SSE: {Json}", jsonResponse);
            }
        }

        var mcpResponse = System.Text.Json.JsonSerializer.Deserialize<McpJsonRpcResponse>(jsonResponse);
        
        if (mcpResponse?.Error != null)
        {
            _logger.LogWarning("[Gateway] MCP Error from {Server}: {Message} (Code: {Code})", 
                server.Name, mcpResponse.Error.Message, mcpResponse.Error.Code);
            
            // Return the error details as a structured object instead of throwing
            return new
            {
                error = true,
                errorCode = mcpResponse.Error.Code,
                errorMessage = mcpResponse.Error.Message,
                server = server.Name,
                tool = toolName
            };
        }

        // Return the full result object (which contains content array)
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
