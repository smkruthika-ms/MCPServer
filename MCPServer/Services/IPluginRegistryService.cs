using MCPServer.Models;

namespace MCPServer.Services;

/// <summary>
/// Service for managing dynamic plugin registration from Dataverse
/// </summary>
public interface IPluginRegistryService
{
    /// <summary>
    /// Fetches all active plugins from Dataverse
    /// </summary>
    Task<IEnumerable<PluginInfo>> GetAllPluginsAsync(string? token = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the cached plugin list
    /// </summary>
    Task RefreshPluginsAsync(string? token = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a plugin with the user prompt
    /// </summary>
    Task<object?> InvokePluginAsync(
        string pluginName,
        string userPrompt,
        string? token = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific plugin by name
    /// </summary>
    Task<PluginInfo?> GetPluginByNameAsync(string pluginName, string? token = null, CancellationToken cancellationToken = default);
}
