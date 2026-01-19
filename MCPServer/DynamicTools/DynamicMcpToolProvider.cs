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
    private readonly AuthTokenContext? _tokenContext;
    // DO NOT CACHE between requests - load fresh tools for each request

    public DynamicMcpToolProvider(
        IPluginRegistryService pluginRegistry,
        ILogger<DynamicMcpToolProvider> logger,
        AuthTokenContext? tokenContext = null)
    {
        _pluginRegistry = pluginRegistry;
        _logger = logger;
        _tokenContext = tokenContext;
        var tokenValue = tokenContext?.Token;
        Console.WriteLine($"[NEW REQUEST] DynamicMcpToolProvider created - Token: {(!string.IsNullOrEmpty(tokenValue) ? "PRESENT" : "MISSING")}");
        _logger.LogInformation("[NEW REQUEST] DynamicMcpToolProvider created - Token: {TokenStatus}", !string.IsNullOrEmpty(tokenValue) ? "PRESENT" : "MISSING");
    }

    /// <summary>
    /// Returns the enumerator of dynamically created tools - called per request
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator()
    {
        Console.WriteLine("[TOOLS] GetEnumerator called - LOADING TOOLS NOW");
        var token = _tokenContext?.Token;
        Console.WriteLine($"[TOOLS] Token available: {(!string.IsNullOrEmpty(token) ? "YES ✓" : "NO ✗")}");
        _logger.LogInformation("[TOOLS] GetEnumerator called - Token: {HasToken}", !string.IsNullOrEmpty(token));
        
        // Load FRESH tools every time (not cached)
        var tools = LoadToolsAsync().GetAwaiter().GetResult();
        Console.WriteLine($"[TOOLS] GetEnumerator returning {tools.Count} tools");
        
        return tools.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Loads all plugins as MCP tools from Dataverse - FRESH PER REQUEST
    /// </summary>
    private async Task<List<McpServerTool>> LoadToolsAsync()
    {
        var token = _tokenContext?.Token;
        Console.WriteLine($"[REQUEST] LoadToolsAsync: Loading FRESH tools with token: {(!string.IsNullOrEmpty(token) ? "YES" : "NO")}");
        _logger.LogInformation("[REQUEST] LoadToolsAsync: Loading FRESH tools with token: {HasToken}", !string.IsNullOrEmpty(token));

        try
        {
            Console.WriteLine($"[REQUEST] Calling GetAllPluginsAsync with token...");
            var plugins = await _pluginRegistry.GetAllPluginsAsync(token);
            var tools = new List<McpServerTool>();
            
            Console.WriteLine($"[REQUEST] ✓ Fetched {plugins.Count()} plugins from Dataverse");
            _logger.LogInformation("[REQUEST] ✓ Fetched {PluginCount} plugins from Dataverse", plugins.Count());

            foreach (var plugin in plugins)
            {
                var tool = CreateToolFromPlugin(plugin);
                if (tool != null)
                {
                    tools.Add(tool);
                    Console.WriteLine($"[REQUEST]   ✓ Created tool: {plugin.PluginName}");
                }
            }

            Console.WriteLine($"[REQUEST] ✓ Successfully created {tools.Count} tools for this request");
            _logger.LogInformation("[REQUEST] ✓ Successfully created {ToolCount} tools for this request", tools.Count);
            return tools;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REQUEST] ✗ ERROR in LoadToolsAsync: {ex.Message}");
            Console.WriteLine($"[REQUEST] StackTrace: {ex.StackTrace}");
            _logger.LogError(ex, "[REQUEST] Error loading dynamic tools from plugins");
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
            Console.WriteLine($"WARNING: Plugin {plugin.PluginName} has no function name");
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
            Console.WriteLine($"Successfully created tool for plugin: {plugin.PluginName}");
            return tool;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR creating tool for plugin {plugin.PluginName}: {ex.Message}");
            _logger.LogError(ex, "Error creating tool for plugin {PluginName}", plugin.PluginName);
            return null;
        }
    }

    /// <summary>
    /// Refreshes the dynamic tools (loads fresh tools for current request)
    /// </summary>
    public async Task RefreshToolsAsync()
    {
        var token = _tokenContext?.Token;
        Console.WriteLine($"[REQUEST] RefreshToolsAsync called - Token: {(!string.IsNullOrEmpty(token) ? "YES ✓" : "NO ✗")}");
        _logger.LogInformation("[REQUEST] RefreshToolsAsync called - Token: {HasToken}", !string.IsNullOrEmpty(token));
        
        // Refresh plugins with current request token
        await _pluginRegistry.RefreshPluginsAsync(token);
        _ = await LoadToolsAsync();
        
        Console.WriteLine("[REQUEST] Tools refreshed");
        _logger.LogInformation("[REQUEST] Tools refreshed");
    }
}
