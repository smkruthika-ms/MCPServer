using MCPServer.Gateway;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebSearchMCPServer.Tools;

/// <summary>
/// Router tool that dispatches to downstream MCP tools based on toolId
/// Implements Approach B: Copilot sees all tools via enum + oneOf, but only one tool function exists here
/// </summary>
[McpServerToolType]
public sealed class GatewayProxyTool
{
    private readonly McpGatewayService _gatewayService;
    private readonly ILogger<GatewayProxyTool> _logger;

    // Tool ID to (server name, tool name) mapping
    private static readonly Dictionary<string, (string ServerName, string ToolName)> ToolMapping = new()
    {
        { "winwireSucessStories", ("WinWire", "winwireSucessStories") },
        { "customer_engagement_hub_copilot", ("CEHub", "customer_engagement_hub_copilot") },
        { "process_rvc_request_for_opportunity", ("RVC", "process_rvc_request_for_opportunity") },
        { "get_leading_prompts", ("RVC", "get_leading_prompts") },
        { "get_account_competitor", ("RVC", "get_account_competitor") },
        { "summarize_account_profile", ("RVC", "summarize_account_profile") },
        { "get_account_team", ("RVC", "get_account_team") },
        { "get_account_news", ("RVC", "get_account_news") }
    };

    public GatewayProxyTool(McpGatewayService gatewayService, ILogger<GatewayProxyTool> logger)
    {
        _gatewayService = gatewayService;
        _logger = logger;
    }

    [McpServerTool, Description("Router: Call any downstream MCP tool by toolId with structured parameters")]
    public async Task<string> CallDownstreamTool(
        McpServer mcpServer,
        [Description("Fully qualified downstream tool ID")] string toolId,
        [Description("Arguments for the selected tool (structure depends on toolId)")] object? @params)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "[Router START] RequestId={RequestId} | ToolId={ToolId} | ParamsType={ParamsType}",
            requestId, toolId, @params?.GetType().Name ?? "null");

        try
        {
            // Validate and map toolId
            if (!ToolMapping.TryGetValue(toolId, out var mapping))
            {
                var error = $"Unknown toolId: {toolId}. Valid IDs: {string.Join(", ", ToolMapping.Keys)}";
                _logger.LogError("[Router] ❌ {Error}", error);
                return JsonSerializer.Serialize(new { error = error });
            }

            var (serverName, downstreamToolName) = mapping;
            _logger.LogInformation(
                "[Router] Routing | ToolId={ToolId} → Server={ServerName} | Tool={ToolName}",
                toolId, serverName, downstreamToolName);

            // Transform params if needed (e.g., winwireSucessStories expects body as JSON string)
            var transformedArgs = TransformParamsIfNeeded(toolId, @params);

            _logger.LogDebug(
                "[Router] Transformed args | ToolId={ToolId} | Original={Original} | Transformed={Transformed}",
                toolId, 
                JsonSerializer.Serialize(@params),
                JsonSerializer.Serialize(transformedArgs));

            // Extract token from server options
            string? token = null;
            if (mcpServer.ServerOptions?.ServerInfo?.Name != null)
            {
                token = mcpServer.ServerOptions.ServerInfo.Name.Split('|')[0];
            }

            // Call the downstream tool
            var result = await _gatewayService.CallToolAsync(downstreamToolName, transformedArgs!, token, serverName);
            stopwatch.Stop();

            _logger.LogInformation(
                "[Router END] RequestId={RequestId} | ToolId={ToolId} | Status=Success | Duration={DurationMs}ms",
                requestId, toolId, stopwatch.ElapsedMilliseconds);

            // Return the full result structure (don't extract text)
            var resultJson = JsonSerializer.Serialize(result);
            return resultJson;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[Router END] RequestId={RequestId} | ToolId={ToolId} | Status=Error | Duration={DurationMs}ms",
                requestId, toolId, stopwatch.ElapsedMilliseconds);

            return JsonSerializer.Serialize(new { error = ex.Message, requestId });
        }
    }

    /// <summary>
    /// Transform params based on toolId requirements
    /// Some downstream tools expect specific formats
    /// </summary>
    private object TransformParamsIfNeeded(string toolId, object? @params)
    {
        if (@params == null)
        {
            return new { };
        }

        // For winwireSucessStories: expects body as JSON string like {"UserPrompt": "..."}
        if (toolId == "winwireSucessStories")
        {
            if (@params is JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty("userPrompt", out var userPrompt))
                {
                    var bodyObj = new { UserPrompt = userPrompt.GetString() };
                    return new { body = JsonSerializer.Serialize(bodyObj) };
                }
            }
            else if (@params is Dictionary<string, object?> dict && dict.TryGetValue("userPrompt", out var prompt))
            {
                var bodyObj = new { UserPrompt = prompt };
                return new { body = JsonSerializer.Serialize(bodyObj) };
            }
        }

        // For customerEngagementHubParams: if body is already a string, keep it as-is
        if (toolId == "customer_engagement_hub_copilot")
        {
            if (@params is JsonElement jsonElem && jsonElem.TryGetProperty("body", out var body))
            {
                return new { body = body.GetString() };
            }
        }

        // For RVC tools: pass params as-is (already structured correctly)
        if (toolId.StartsWith("process_rvc") || toolId.StartsWith("get_"))
        {
            return @params;
        }

        // Default: return as-is
        return @params;
    }
}
