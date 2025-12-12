using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MCPServer.Gateway;

/// <summary>
/// Centralized logging service for all MCP gateway operations
/// </summary>
public class GatewayLoggingService
{
    private readonly ILogger<GatewayLoggingService> _logger;
    private readonly Dictionary<string, Stopwatch> _requestTimers = new();

    public GatewayLoggingService(ILogger<GatewayLoggingService> logger)
    {
        _logger = logger;
    }

    public string StartToolCall(string toolName, string targetServer, object arguments)
    {
        var requestId = Guid.NewGuid().ToString();
        _requestTimers[requestId] = Stopwatch.StartNew();

        _logger.LogInformation(
            "[Gateway] RequestId={RequestId} | Tool={ToolName} | TargetServer={TargetServer} | Arguments={Arguments}",
            requestId, toolName, targetServer, System.Text.Json.JsonSerializer.Serialize(arguments));

        return requestId;
    }

    public void LogToolSuccess(string requestId, string toolName, string targetServer, object response, int statusCode)
    {
        if (_requestTimers.TryGetValue(requestId, out var timer))
        {
            timer.Stop();
            _logger.LogInformation(
                "[Gateway] RequestId={RequestId} | Tool={ToolName} | TargetServer={TargetServer} | StatusCode={StatusCode} | Duration={Duration}ms | Response={Response}",
                requestId, toolName, targetServer, statusCode, timer.ElapsedMilliseconds, System.Text.Json.JsonSerializer.Serialize(response));
            
            _requestTimers.Remove(requestId);
        }
    }

    public void LogToolError(string requestId, string toolName, string targetServer, Exception ex, int? statusCode = null)
    {
        if (_requestTimers.TryGetValue(requestId, out var timer))
        {
            timer.Stop();
            _logger.LogError(ex,
                "[Gateway] RequestId={RequestId} | Tool={ToolName} | TargetServer={TargetServer} | StatusCode={StatusCode} | Duration={Duration}ms | Error={ErrorMessage}",
                requestId, toolName, targetServer, statusCode ?? 500, timer.ElapsedMilliseconds, ex.Message);
            
            _requestTimers.Remove(requestId);
        }
    }

    public void LogServerDiscovery(string serverName, int toolCount, bool success, string? errorMessage = null)
    {
        if (success)
        {
            _logger.LogInformation(
                "[Gateway] Server={ServerName} | ToolDiscovery=Success | ToolCount={ToolCount}",
                serverName, toolCount);
        }
        else
        {
            _logger.LogWarning(
                "[Gateway] Server={ServerName} | ToolDiscovery=Failed | Error={ErrorMessage}",
                serverName, errorMessage);
        }
    }
}
