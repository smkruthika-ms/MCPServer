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
    /// Returns the enumerator of dynamically created tools - called per request, NO CACHING
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator()
    {
        Console.WriteLine("[TOOLS] GetEnumerator called - ALWAYS LOADING FRESH TOOLS FROM API");
        
        // Debug: Check if tokenContext is null
        if (_tokenContext == null)
        {
            Console.WriteLine("[TOOLS] WARNING: _tokenContext is NULL!");
            _logger.LogError("[TOOLS] _tokenContext is NULL - cannot load plugins without token context");
        }
        else
        {
            Console.WriteLine("[TOOLS] _tokenContext is present");
        }
        
        var token = _tokenContext?.Token;
        Console.WriteLine($"[TOOLS] Token value: {(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : $"Present ({token?.Length} chars)")}");
        _logger.LogInformation("[TOOLS] GetEnumerator called - Token: {HasToken}", !string.IsNullOrEmpty(token));
        
        // Always load fresh tools from API (no caching)
        var tools = LoadToolsAsync().GetAwaiter().GetResult();
        Console.WriteLine($"[TOOLS] GetEnumerator returning {tools.Count} tools from API");
        
        return tools.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Loads all plugins as MCP tools from Dataverse - FRESH PER REQUEST
    /// </summary>
    private async Task<List<McpServerTool>> LoadToolsAsync()
    {
        var token = _tokenContext?.Token;
        Console.WriteLine($"[REQUEST] LoadToolsAsync: _tokenContext is {(_tokenContext == null ? "NULL" : "PRESENT")}");
        Console.WriteLine($"[REQUEST] LoadToolsAsync: Loading FRESH tools with token: {(!string.IsNullOrEmpty(token) ? "YES ("+token.Length+" chars)" : "NO/NULL")}");
        _logger.LogInformation("[REQUEST] LoadToolsAsync: Loading FRESH tools with token: {HasToken}", !string.IsNullOrEmpty(token));

        try
        {
            Console.WriteLine($"[REQUEST] Calling GetAllPluginsAsync with token...");
            var plugins = await _pluginRegistry.GetAllPluginsAsync(token);
            var pluginList = plugins.ToList();
            var tools = new List<McpServerTool>();
            
            Console.WriteLine($"[REQUEST] ✓ Fetched {pluginList.Count()} plugins from Dataverse");
            _logger.LogInformation("[REQUEST] ✓ Fetched {PluginCount} plugins from Dataverse", pluginList.Count());

            if (pluginList.Count() == 0)
            {
                Console.WriteLine($"[REQUEST] WARNING: GetAllPluginsAsync returned 0 plugins!");
            }

            foreach (var plugin in pluginList)
            {
                var tool = CreateToolFromPlugin(plugin);
                if (tool != null)
                {
                    tools.Add(tool);
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

            // Create tool options with metadata
            var options = new McpServerToolCreateOptions
            {
                Name = toolName,
                Description = toolDescription,
                // Optional: Set ReadOnly based on plugin characteristics
                ReadOnly = false,
            };

            // Create a wrapper function that invokes the plugin
            Func<Dictionary<string, object?>, CancellationToken, Task<object?>> pluginInvoker =
                async (args, ct) => await _pluginRegistry.InvokePluginAsync(plugin.PluginName!, args, ct);

            // Create the tool with the delegate directly
            var tool = McpServerTool.Create(pluginInvoker, options);
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
