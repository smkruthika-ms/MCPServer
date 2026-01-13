using MCPServer.Models;
using MCPServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace MCPServer.DynamicTools;

/// <summary>
/// Wraps a plugin invocation as a method that can be converted to an McpServerTool
/// </summary>
internal class DynamicPluginInvoker
{
    private readonly ExecutionHostService _executionHostService;
    private readonly PluginInfo _plugin;
    private readonly ILogger? _logger;

    public DynamicPluginInvoker(ExecutionHostService executionHostService, PluginInfo plugin, ILogger? logger = null)
    {
        _executionHostService = executionHostService;
        _plugin = plugin;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the plugin via ExecutionHostService with the required MCP tool parameters
    /// Returns the response body as a string (not HttpResponseData object to avoid serialization issues)
    /// </summary>
    public async Task<string> InvokeAsync(
        McpServer thisServer,
        [Description("The user prompt or query")] string userPrompt,
        [Description("Additional context for the plugin")] string context,
        CancellationToken cancellationToken = default)
    {
        // Use PlannerKey field strictly
        var plannerKey = _plugin.PlannerKey;
        
        // Extract token from server
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        
        _logger?.LogInformation("=== Dynamic Plugin Invocation ===");
        _logger?.LogInformation("Plugin Name: {PluginName}", _plugin.PluginName);
        _logger?.LogInformation("Plugin ID: {PluginId}", _plugin.PluginId);
        _logger?.LogInformation("Planner Key: {PlannerKey}", plannerKey);
        _logger?.LogInformation("User Prompt: {UserPrompt}", userPrompt);
        _logger?.LogInformation("Context: {Context}", context);
        _logger?.LogInformation("Token: {Token}", !string.IsNullOrEmpty(token) ? "Bearer ***" : "No token");
        _logger?.LogInformation("Base Endpoint: {BaseEndpoint}", _plugin.BaseHttpEndpoint);
        _logger?.LogInformation("Method Endpoint: {MethodEndpoint}", _plugin.MethodEndpoint);
        
        if (string.IsNullOrEmpty(plannerKey))
        {
            _logger?.LogError("PlannerKey is null or empty for plugin {PluginName}", _plugin.PluginName);
            return $"{{\"error\": \"PlannerKey is null or empty for plugin {_plugin.PluginName}\"}}";
        }
        
        // Invoke via ExecutionHostService using the new convenience method
        var result = await _executionHostService.InvokeDynamicPluginAsync(
            thisServer,
            plannerKey, 
            userPrompt, 
            context,
            cancellationToken);
        
        _logger?.LogInformation("Plugin invocation completed with status: {StatusCode}", result.StatusCode);
        
        // Return just the response body as a string (not the HttpResponseData object)
        // This avoids JSON serialization issues with Stream property
        return result.BodyAsString ?? $"{{\"error\": \"No response body. Status: {result.StatusCode}\"}}";
    }
}
