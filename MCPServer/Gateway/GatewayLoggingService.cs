using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MCPServer.Gateway;

/// <summary>
/// Centralized logging service for all MCP gateway operations with performance tracking
/// </summary>
public class GatewayLoggingService
{
    private readonly ILogger<GatewayLoggingService> _logger;
    private readonly Dictionary<string, Stopwatch> _requestTimers = new();
    
    // Performance tracking
    private long _totalRequests = 0;
    private long _successfulRequests = 0;
    private long _failedRequests = 0;
    private long _totalDurationMs = 0;
    private long _totalGatewayOverheadMs = 0; // Time spent in gateway processing
    private readonly object _perfLock = new object();

    public GatewayLoggingService(ILogger<GatewayLoggingService> logger)
    {
        _logger = logger;
    }

    public string StartToolCall(string toolName, string targetServer, object arguments)
    {
        var requestId = Guid.NewGuid().ToString();
        _requestTimers[requestId] = Stopwatch.StartNew();

        lock (_perfLock)
        {
            _totalRequests++;
        }

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
            
            lock (_perfLock)
            {
                _successfulRequests++;
                _totalDurationMs += timer.ElapsedMilliseconds;
            }

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
            
            lock (_perfLock)
            {
                _failedRequests++;
                _totalDurationMs += timer.ElapsedMilliseconds;
            }

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

    public void LogGatewayOverhead(long overheadMs)
    {
        lock (_perfLock)
        {
            _totalGatewayOverheadMs += overheadMs;
        }
    }

    public void LogPerformanceMetrics()
    {
        lock (_perfLock)
        {
            var avgDuration = _totalRequests > 0 ? _totalDurationMs / _totalRequests : 0;
            var avgOverhead = _totalRequests > 0 ? _totalGatewayOverheadMs / _totalRequests : 0;
            var successRate = _totalRequests > 0 ? (_successfulRequests * 100.0 / _totalRequests) : 0;

            _logger.LogInformation(
                "📊 [Gateway Performance Metrics] TotalRequests={TotalRequests} | Successful={Successful} | Failed={Failed} | SuccessRate={SuccessRate:F2}% | TotalDuration={TotalDuration}ms | AvgDuration={AvgDuration}ms | TotalOverhead={TotalOverhead}ms | AvgOverhead={AvgOverhead}ms",
                _totalRequests, _successfulRequests, _failedRequests, successRate, _totalDurationMs, avgDuration, _totalGatewayOverheadMs, avgOverhead);
        }
    }

    public (long TotalRequests, long Successful, long Failed, long TotalDurationMs, long TotalOverheadMs) GetMetrics()
    {
        lock (_perfLock)
        {
            return (_totalRequests, _successfulRequests, _failedRequests, _totalDurationMs, _totalGatewayOverheadMs);
        }
    }
}

