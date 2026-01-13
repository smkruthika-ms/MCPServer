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
    /// Returns the enumerator of dynamically created tools
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator()
    {
        // Load tools synchronously (called during server startup)
        var tools = LoadToolsAsync().GetAwaiter().GetResult();
        return tools.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Loads all plugins as MCP tools from Dataverse
    /// </summary>
    private async Task<List<McpServerTool>> LoadToolsAsync()
    {
        if (_cachedTools != null)
        {
            return _cachedTools;
        }

        try
        {
            var plugins = await _pluginRegistry.GetAllPluginsAsync();
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
        _ = await LoadToolsAsync();
        _logger.LogInformation("Dynamic tools refreshed");
    }
}
