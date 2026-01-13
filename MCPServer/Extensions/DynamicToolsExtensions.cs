using MCPServer.DynamicTools;
using MCPServer.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Reflection;

namespace MCPServer.Extensions;

/// <summary>
/// Extension methods for setting up dynamic MCP tools from Dataverse plugins
/// </summary>
public static class DynamicToolsExtensions
{
    /// <summary>
    /// Registers the plugin registry service and adds dynamic tools from Dataverse to the MCP server
    /// </summary>
    public static IMcpServerBuilder WithDynamicPlugins(this IMcpServerBuilder builder, IServiceCollection services)
    {
        // Register memory cache
        services.AddMemoryCache();

        // Register Dataverse API service (required by DataversePluginRegistryService)
        services.AddScoped<DataverseApiService>();

        // Register the plugin registry service as scoped
        services.AddScoped<IPluginRegistryService, DataversePluginRegistryService>();

        // Add HTTP client for plugin invocation  
        services.AddHttpClient<DataversePluginRegistryService>();

        // Build a temporary provider to load plugins immediately
        var tempProvider = services.BuildServiceProvider();
        
        try
        {
            // Create a scope to load plugins
            using (var scope = tempProvider.CreateScope())
            {
                var registry = scope.ServiceProvider.GetRequiredService<IPluginRegistryService>();
                var executionHostService = scope.ServiceProvider.GetRequiredService<ExecutionHostService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<DynamicMcpToolProvider>>();
                
                logger.LogInformation("Starting dynamic plugin loading...");
                
                // Get token from builder options if available
                string? token = null;
                if (builder is IMcpServerBuilder mcpBuilder)
                {
                    // Try to get token from ServerOptions (will be set by the time plugins are loaded)
                    logger.LogInformation("Getting token for plugin API request...");
                }
                
                // Load plugins synchronously (blocking on async)
                var plugins = registry.GetAllPluginsAsync(token, CancellationToken.None).GetAwaiter().GetResult();
                
                logger.LogInformation("Found {PluginCount} plugins", plugins.Count());
                
                // Create tools from plugins
                var tools = new List<McpServerTool>();
                foreach (var plugin in plugins)
                {
                    try
                    {
                        // Create an invoker for this plugin
                        var invoker = new DynamicPluginInvoker(executionHostService, plugin, logger);
                        var invokerType = invoker.GetType();
                        var invokeMethod = invokerType.GetMethod("InvokeAsync", 
                            BindingFlags.Public | BindingFlags.Instance);
                        
                        if (invokeMethod == null)
                        {
                            logger.LogError("Could not find InvokeAsync method on DynamicPluginInvoker");
                            continue;
                        }
                        
                        var tool = McpServerTool.Create(
                            invokeMethod,
                            invoker,
                            new McpServerToolCreateOptions
                            {
                                Name = plugin.PluginName,
                                Description = plugin.Description ?? $"Plugin: {plugin.FriendlyName}",
                                Services = scope.ServiceProvider
                            });
                        
                        tools.Add(tool);
                        logger.LogInformation("Registered dynamic tool: {ToolName}", plugin.PluginName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to create tool for plugin {PluginName}", plugin.PluginName);
                    }
                }
                
                // Register all loaded tools with the builder
                if (tools.Any())
                {
                    builder.WithTools(tools);
                    logger.LogInformation("Loaded {ToolCount} dynamic tools from plugins", tools.Count);
                }
            }
        }
        catch (Exception ex)
        {
            var logger = tempProvider.GetRequiredService<ILogger<DynamicMcpToolProvider>>();
            logger.LogError(ex, "Error loading dynamic tools");
        }

        return builder;
    }
}




