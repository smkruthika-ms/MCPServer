using ModelContextProtocol;
using ModelContextProtocol.Server;
using MCPServer.Gateway;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public class DiscoverDownstreamToolsTool
{
    private readonly McpGatewayService _gatewayService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DiscoverDownstreamToolsTool> _logger;

    public DiscoverDownstreamToolsTool(
        McpGatewayService gatewayService,
        IHttpClientFactory httpClientFactory,
        ILogger<DiscoverDownstreamToolsTool> logger)
    {
        _gatewayService = gatewayService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [McpServerTool, Description("Discovers what tools are available on a downstream MCP server by calling tools/list")]
    public async Task<string> DiscoverTools(
        [Description("The base URL of the downstream MCP server (e.g., https://mcpservernet.azurewebsites.net)")]
        string serverUrl)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Add MCP headers
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            // Build JSON-RPC request for tools/list
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/list",
                @params = new { }
            };

            var requestJson = System.Text.Json.JsonSerializer.Serialize(mcpRequest);
            _logger.LogInformation("[Discovery] Requesting tools/list from {Url}", serverUrl);
            _logger.LogInformation("[Discovery] Request: {Request}", requestJson);

            var content = new StringContent(
                requestJson,
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(serverUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("[Discovery] Response status: {Status}", response.StatusCode);
            _logger.LogInformation("[Discovery] Response body: {Body}", responseBody);

            // Parse SSE format if needed
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

            return $"Server: {serverUrl}\nStatus: {response.StatusCode}\nResponse:\n{jsonResponse}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Discovery] Error discovering tools from {Url}", serverUrl);
            return $"Error: {ex.Message}";
        }
    }
}
