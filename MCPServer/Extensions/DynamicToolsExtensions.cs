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
        services.AddScoped<DataverseApiService>();

        // Register the plugin registry service as scoped
        services.AddScoped<IPluginRegistryService, DataversePluginRegistryService>();

        // Add HTTP client for plugin invocation  
        services.AddHttpClient<DataversePluginRegistryService>();
        
        // Register the dynamic provider as SCOPED (per-request)
        services.AddScoped<DynamicMcpToolProvider>();

        return builder;
    }
}




