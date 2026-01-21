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
    /// Registers the plugin registry service and adds dynamic tools from Dataverse to the MCP server.
    /// Token is extracted from httpContext during ConfigureSessionOptions and stored in TokenContext
    /// </summary>
    public static IMcpServerBuilder WithDynamicPlugins(this IMcpServerBuilder builder, IServiceCollection services)
    {
        // Register memory cache
        services.AddMemoryCache();

        // Register Dataverse API service (required by DataversePluginRegistryService)
        // Use Singleton so the HttpClient is reused
        services.AddSingleton<DataverseApiService>();

        // Register the plugin registry service as SINGLETON so cache is shared across all requests
        // This is critical - the cache must persist across requests for tools/list to work
        services.AddSingleton<IPluginRegistryService, DataversePluginRegistryService>();

        // Add HTTP client for plugin invocation  
        services.AddHttpClient<DataversePluginRegistryService>();
        
        // Register the dynamic provider as TRANSIENT (fresh instance each time it's requested)
        services.AddTransient<DynamicMcpToolProvider>();

        return builder;
    }
}




