using MCPServer.Gateway;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace WebSearchMCPServer.Tools;

/// <summary>
/// Gateway proxy tool that dynamically calls downstream MCP server tools
/// </summary>
[McpServerToolType]
public sealed class GatewayProxyTool
{
    private readonly McpGatewayService _gatewayService;
    private readonly ILogger<GatewayProxyTool> _logger;

    public GatewayProxyTool(McpGatewayService gatewayService, ILogger<GatewayProxyTool> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [McpServerTool, Description("Call a tool from any connected downstream MCP server")]
    public async Task<string> CallDownstreamTool(
        McpServer mcpServer,
        [Description("Name of the tool to call")] string toolName,
        [Description("JSON string of arguments to pass to the tool")] string argumentsJson)
    {
        try
        {
            // Parse arguments
            var arguments = JsonSerializer.Deserialize<object>(argumentsJson);

            // Extract token from server options if available
            string? token = null;
            if (mcpServer.ServerOptions?.ServerInfo?.Name != null)
            {
                token = mcpServer.ServerOptions.ServerInfo.Name.Split('|')[0];
            }

            // Call the downstream tool
            var result = await _gatewayService.CallToolAsync(toolName, arguments!, token);

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Gateway] Error calling downstream tool {ToolName}", toolName);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
