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
/// NOTE: This class is no longer used - tools are now loaded via McpServerHandlers in Program.cs
/// </summary>
public class DynamicMcpToolProvider : IEnumerable<McpServerTool>
{
    private readonly IPluginRegistryService _pluginRegistry;
    private readonly ILogger<DynamicMcpToolProvider> _logger;
    private List<McpServerTool>? _cachedTools;
    private string? _sessionToken;

    public DynamicMcpToolProvider(
        IPluginRegistryService pluginRegistry,
        ILogger<DynamicMcpToolProvider> logger)
    {
        _pluginRegistry = pluginRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Sets the session token for plugin API calls
    /// </summary>
    public void SetSessionToken(string? token)
    {
        _sessionToken = token;
    }

    /// <summary>
    /// Returns the enumerator of dynamically created tools (parameterless - required by IEnumerable)
    /// </summary>
    public IEnumerator<McpServerTool> GetEnumerator()
    {
        Console.WriteLine($"[TOOLS] GetEnumerator called - Token: {(!string.IsNullOrEmpty(_sessionToken) ? "YES" : "NO")}");
        
        // Load tools synchronously
        var tools = LoadToolsAsync(_sessionToken).GetAwaiter().GetResult();
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
        
        // Store it for future use
        if (!string.IsNullOrEmpty(token))
        {
            _sessionToken = token;
        }
        
        var tools = LoadToolsAsync(_sessionToken).GetAwaiter().GetResult();
        return tools.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Loads all plugins as MCP tools from Dataverse
    /// </summary>
    private async Task<List<McpServerTool>> LoadToolsAsync(string? token)
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
                var tool = CreateToolFromPlugin(plugin, token);
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
    private McpServerTool? CreateToolFromPlugin(PluginInfo plugin, string? token)
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

            // Create a wrapper function that invokes the plugin with the token
            // Extract text from args for the user prompt
            Func<Dictionary<string, object?>, CancellationToken, Task<object?>> pluginInvoker =
                async (args, ct) => 
                {
                    var userPrompt = args?.ContainsKey("text") == true ? args["text"]?.ToString() ?? "" : "";
                    return await _pluginRegistry.InvokePluginAsync(plugin.PluginName!, userPrompt, token, ct);
                };

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
        await _pluginRegistry.RefreshPluginsAsync(_sessionToken);
        _ = await LoadToolsAsync(_sessionToken);
        _logger.LogInformation("Dynamic tools refreshed");
    }
}
