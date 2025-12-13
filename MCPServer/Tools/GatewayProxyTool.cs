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
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 🚀 START LOG - Gateway proxy tool invoked
        _logger.LogInformation(
            "[Gateway Proxy START] RequestId={RequestId} | Tool={ToolName} | ArgumentsJson={ArgumentsJson}",
            requestId, toolName, argumentsJson);

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
            stopwatch.Stop();

            // Extract text from MCP response structure to avoid double-wrapping
            string responseText;
            
            if (result is JsonElement jsonElement)
            {
                // Handle JsonElement (common case)
                if (jsonElement.TryGetProperty("content", out var contentProp) && 
                    contentProp.ValueKind == JsonValueKind.Array &&
                    contentProp.GetArrayLength() > 0)
                {
                    var firstContent = contentProp[0];
                    if (firstContent.TryGetProperty("text", out var textProp))
                    {
                        responseText = textProp.GetString() ?? JsonSerializer.Serialize(result);
                        _logger.LogDebug(
                            "[Gateway Proxy] Extracted text from content array | Length={Length}",
                            responseText.Length);
                    }
                    else
                    {
                        // No text property, serialize the whole content item
                        responseText = JsonSerializer.Serialize(firstContent);
                        _logger.LogDebug("[Gateway Proxy] Using serialized first content item");
                    }
                }
                else
                {
                    // No content array, return as-is
                    responseText = JsonSerializer.Serialize(result);
                    _logger.LogDebug("[Gateway Proxy] No content array found, using full result");
                }
            }
            else
            {
                // Try dynamic property access for other object types
                try
                {
                    var resultJson = JsonSerializer.Serialize(result);
                    var doc = JsonDocument.Parse(resultJson);
                    
                    if (doc.RootElement.TryGetProperty("content", out var contentProp) &&
                        contentProp.ValueKind == JsonValueKind.Array &&
                        contentProp.GetArrayLength() > 0)
                    {
                        var firstContent = contentProp[0];
                        if (firstContent.TryGetProperty("text", out var textProp))
                        {
                            responseText = textProp.GetString() ?? resultJson;
                            _logger.LogDebug(
                                "[Gateway Proxy] Extracted text from parsed JSON | Length={Length}",
                                responseText.Length);
                        }
                        else
                        {
                            responseText = JsonSerializer.Serialize(firstContent);
                        }
                    }
                    else
                    {
                        responseText = resultJson;
                    }
                }
                catch
                {
                    // Fallback to full serialization
                    responseText = JsonSerializer.Serialize(result);
                    _logger.LogDebug("[Gateway Proxy] Using fallback full serialization");
                }
            }

            // ✅ END LOG - Success
            _logger.LogInformation(
                "[Gateway Proxy END] RequestId={RequestId} | Tool={ToolName} | Status=Success | Duration={DurationMs}ms | ResultLength={ResultLength}",
                requestId, toolName, stopwatch.ElapsedMilliseconds, responseText.Length);

            return responseText;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // ❌ END LOG - Error
            _logger.LogError(
                "[Gateway Proxy END] RequestId={RequestId} | Tool={ToolName} | Status=Error | Duration={DurationMs}ms | Error={ErrorMessage}",
                requestId, toolName, stopwatch.ElapsedMilliseconds, ex.Message);

            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
