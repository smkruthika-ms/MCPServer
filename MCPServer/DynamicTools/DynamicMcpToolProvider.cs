using MCPServer.Models;
using MCPServer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Collections;
using System.Text.Json;

namespace MCPServer.DynamicTools;

/// <summary>
/// Provides MCPServerTool instances dynamically from Dataverse plugins.
/// Implements IEnumerable<McpServerTool> to be registered with IMcpServerBuilder.WithTools()
/// </summary>
public class DynamicMcpToolProvider : IEnumerable<McpServerTool>
{
    private readonly IPluginRegistryService _pluginRegistry;
    private readonly ILogger<DynamicMcpToolProvider> _logger;
    private List<McpServerTool>? _cachedTools;

    public DynamicMcpToolProvider(
        IPluginRegistryService pluginRegistry,
        ILogger<DynamicMcpToolProvider> logger)
    {
        _pluginRegistry = pluginRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Returns the enumerator of dynamically created tools (parameterless - required by IEnumerable)
    /// Uses CurrentToken from singleton PluginRegistry
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator()
    {
        // Get token from the singleton registry (set during ConfigureSessionOptions)
        var token = _pluginRegistry.CurrentToken;
        Console.WriteLine($"[TOOLS] GetEnumerator called - Token from registry: {(!string.IsNullOrEmpty(token) ? "YES" : "NO")}");
        
        // Load tools synchronously
        var tools = LoadToolsAsync(token).GetAwaiter().GetResult();
        return tools.GetEnumerator();
    }
    
    /// <summary>
    /// Returns the enumerator of dynamically created tools with explicit McpServer (for token extraction)
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator(McpServer mcpServer)
    {
        // Extract token from McpServer's ServerInfo.Name (where we stored it)
        var token = mcpServer.ServerOptions?.ServerInfo?.Name;
        Console.WriteLine($"[TOOLS] GetEnumerator(mcpServer) called - Token: {(!string.IsNullOrEmpty(token) ? "YES" : "NO")}");
        
        // Also store it in the registry for future use
        if (!string.IsNullOrEmpty(token))
        {
            _pluginRegistry.CurrentToken = token;
        }
        
        var tools = LoadToolsAsync(token).GetAwaiter().GetResult();
        return tools.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Loads all plugins as MCP tools from Dataverse
    /// </summary>
    private async Task<List<McpServerTool>> LoadToolsAsync(string token)
    {
        if (_cachedTools != null)
        {
            return _cachedTools;
        }

        try
        {
            var plugins = await _pluginRegistry.GetAllPluginsAsync(token);
            var tools = new List<McpServerTool>();

            foreach (var plugin in plugins)
            {
                var tool = CreateToolFromPlugin(plugin);
                if (tool != null)
                {
                    tools.Add(tool);
                    _logger.LogInformation("Created dynamic tool for plugin: {PluginName}", plugin.PluginName);
                }
            }

            _logger.LogInformation("Loaded {ToolCount} dynamic tools from plugins", tools.Count);
            _cachedTools = tools;
            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dynamic tools from plugins");
            return [];
        }
    }

    /// <summary>
    /// Creates an McpServerTool for a given plugin
    /// </summary>
    private McpServerTool? CreateToolFromPlugin(PluginInfo plugin)
    {
        if (string.IsNullOrWhiteSpace(plugin.FunctionName))
        {
            _logger.LogWarning("Plugin {PluginName} has no function name", plugin.PluginName);
            return null;
        }

        try
        {
            var toolName = plugin.FunctionName ?? plugin.PluginName ?? "unknown_tool";
            var toolDescription = plugin.Description ?? plugin.FriendlyName ?? "No description available";

            // Create a wrapper function that invokes the plugin
            Func<Dictionary<string, object?>, CancellationToken, Task<object?>> pluginInvoker =
                async (args, ct) => await _pluginRegistry.InvokePluginAsync(plugin.PluginName!, args, ct);

            // Create tool options with metadata
            var options = new McpServerToolCreateOptions
            {
                Name = toolName,
                Description = toolDescription,
                // Optional: Set ReadOnly based on plugin characteristics
                ReadOnly = false,
            };

            // Create the tool
            var tool = McpServerTool.Create(pluginInvoker.Method, options);
            return tool;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool for plugin {PluginName}", plugin.PluginName);
            return null;
        }
    }

    /// <summary>
    /// Refreshes the cached tools (useful if plugins change in Dataverse)
    /// </summary>
    public async Task RefreshToolsAsync()
    {
        _cachedTools = null;
        await _pluginRegistry.RefreshPluginsAsync();
        _ = await LoadToolsAsync(_pluginRegistry.CurrentToken);
        _logger.LogInformation("Dynamic tools refreshed");
    }
}
