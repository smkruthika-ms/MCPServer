using MCPServer.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MCPServer.Services;

/// <summary>
/// Implementation of plugin registry that fetches from Dataverse OData API
/// </summary>
public class DataversePluginRegistryService : IPluginRegistryService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DataversePluginRegistryService> _logger;
    private readonly DataverseApiService _dataverseService;
    private readonly PluginFilterOptions _filterOptions;
    private const string PluginsCacheKey = "mcp_plugins_cache";
    private const int CacheDurationMinutes = 60;

    public DataversePluginRegistryService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<DataversePluginRegistryService> logger,
        DataverseApiService dataverseService,
        IOptions<PluginFilterOptions> filterOptions)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _dataverseService = dataverseService;
        _filterOptions = filterOptions.Value;
    }

    /// <summary>
    /// Fetches all active plugins from Dataverse
    /// </summary>
    public async Task<IEnumerable<PluginInfo>> GetAllPluginsAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        

        try
        {
            // Fetch from plugin API
            var plugins = await FetchPluginsFromDataverseAsync(token, cancellationToken);
            
            // Filter out ignored planner keys
            var ignoredKeys = _filterOptions.IgnoredPlannerKeys ?? new List<string>();
            var filteredPlugins = plugins
                .Where(p => ignoredKeys.Contains(p.PlannerKey, StringComparer.OrdinalIgnoreCase))
                .ToList();
            
            if (ignoredKeys.Count > 0)
            {
                var ignoredCount = plugins.Count() - filteredPlugins.Count;
                _logger.LogInformation("Filtered out {IgnoredCount} plugins based on IgnoredPlannerKeys configuration", ignoredCount);
            }
            
            var activePlugins = filteredPlugins//.Where(p => p.IsActive)
            .ToList();

            _logger.LogInformation("Fetched {PluginCount} active plugins from Dataverse (after filtering)", activePlugins.Count);

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));
            _cache.Set(PluginsCacheKey, (IEnumerable<PluginInfo>)activePlugins, cacheOptions);

            return activePlugins;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plugins from Dataverse");
            return [];
        }
    }

    /// <summary>
    /// Refreshes the cached plugin list
    /// </summary>
    public async Task RefreshPluginsAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        _cache.Remove(PluginsCacheKey);
        _ = await GetAllPluginsAsync(token, cancellationToken);
        _logger.LogInformation("Plugin cache refreshed");
    }

    /// <summary>
    /// Gets a specific plugin by name
    /// </summary>
    public async Task<PluginInfo?> GetPluginByNameAsync(string pluginName, string? token = null, CancellationToken cancellationToken = default)
    {
        var plugins = await GetAllPluginsAsync(token, cancellationToken);
        return plugins.FirstOrDefault(p =>
            p.PluginName?.Equals(pluginName, StringComparison.OrdinalIgnoreCase) == true ||
            p.FunctionName?.Equals(pluginName, StringComparison.OrdinalIgnoreCase) == true ||
            p.PlannerKey?.Equals(pluginName, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Invokes a plugin with the user prompt
    /// </summary>
    public async Task<object?> InvokePluginAsync(
        string pluginName,
        string inputs,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        var plugin = await GetPluginByNameAsync(pluginName, token, cancellationToken);
        if (plugin == null)
        {
            _logger.LogWarning("Plugin not found: {PluginName}", pluginName);
            throw new InvalidOperationException($"Plugin '{pluginName}' not found");
        }
/*
        if (!plugin.IsActive)
        {
            _logger.LogWarning("Plugin is not active: {PluginName}", pluginName);
            throw new InvalidOperationException($"Plugin '{pluginName}' is not active");
        }
*/
        try
        {
            var endpoint = "https://msxuat-fd-gxbmeya2c3gwbyhq.b02.azurefd.net/api/executionHost"+ $"/{plugin.PlannerKey}?api-version=1";
            _logger.LogInformation("Invoking plugin {PluginName} at {Endpoint}", pluginName, endpoint);

            // Build the request with inputs.text and inputs.text_3
            var request = new PluginInvocationRequest
            {
                FunctionName = plugin.FunctionName,
                Inputs = JsonSerializer.Deserialize<PluginInputs>(inputs)
            };
            
            _logger.LogInformation("Plugin invocation request - FunctionName: {FunctionName}, Inputs length: {InputsLength}", 
                plugin.FunctionName, request.ToString());

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            // Create a new request with Authorization header
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Content = jsonContent;
            
            // Add the Bearer token (passed securely via parameter, not stored)
            if (!string.IsNullOrEmpty(token))
            {
                var tokenValue = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token.Substring(7).Trim()
                    : token.Trim();
                _logger.LogInformation("Plugin invocation token length: {TokenLength}", tokenValue.Length);
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenValue);
                _logger.LogInformation("Added Authorization header to plugin invocation request");
            }
            else
            {
                _logger.LogWarning("No token provided for plugin invocation - request may fail with 401");
            }

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Plugin invocation failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode, errorContent);
                throw new InvalidOperationException(
                    $"Plugin invocation failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var pluginResponse = JsonSerializer.Deserialize<PluginInvocationResponse>(responseContent);

            _logger.LogInformation("Plugin {PluginName} invoked successfully", pluginName);
            return pluginResponse?.Output ?? responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking plugin {PluginName}", pluginName);
            throw;
        }
    }

    /// <summary>
    /// Fetches plugins from the Plugin API endpoint
    /// </summary>
    private async Task<IEnumerable<PluginInfo>> FetchPluginsFromDataverseAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a new HttpClient instance for this request to avoid header conflicts
            using var client = new HttpClient();
            
            // Use provided token directly (no fallback to avoid storing tokens)
            var effectiveToken = token;
            
            if (!string.IsNullOrEmpty(effectiveToken))
            {
                // Remove "Bearer " prefix (case-insensitive) and trim all whitespace
                var cleanToken = effectiveToken;
                if (cleanToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    cleanToken = cleanToken.Substring(7); // Remove "Bearer " (7 characters)
                }
                cleanToken = cleanToken.Trim();
                
                // Only remove quotes if the token is wrapped in them (from JSON serialization)
                if (cleanToken.StartsWith("\"") && cleanToken.EndsWith("\"") && cleanToken.Length > 2)
                {
                    cleanToken = cleanToken.Substring(1, cleanToken.Length - 2);
                    _logger.LogInformation("Removed JSON quotes from token");
                }
                
                
                if (cleanToken.Length > 0)
                {
                    _logger.LogInformation("Setting Authorization header with Bearer token");
                    
                    // Set Authorization header on the new client instance (not the injected _httpClient)
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                    
                    _logger.LogInformation("Authorization header set successfully");
                }
                else
                {
                    _logger.LogWarning("Token provided but empty after cleaning");
                }
            }
            else
            {
                _logger.LogWarning("No token provided for plugin API request");
            }
            

            // Call the plugin API endpoint
            const string pluginApiUrl = "https://salescopilotskeusuat.azurewebsites.net/api/plugins";
            _logger.LogInformation("Fetching plugins from: {PluginApiUrl}", pluginApiUrl);
            
            var response = await client.GetAsync(pluginApiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to fetch plugins from API: {StatusCode}. Using mock data. Error: {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                // Return mock data when API fails
                return [];// GetMockPlugins();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received plugin API response: {ResponseLength} characters", content.Length);
            
            var pluginResponse = JsonSerializer.Deserialize<PluginListResponse>(content);

            return pluginResponse?.Plugins ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching plugins from API, using mock data");
            // Return mock data on HTTP errors
            return [];//GetMockPlugins();
        }
    }

    /// <summary>
    /// Returns mock plugin data for testing when Dataverse API is unavailable
    /// </summary>
    private static IEnumerable<PluginInfo> GetMockPlugins()
    {
        return new[]
        {
            new PluginInfo
            {
                PluginId = "e2b171d9-b892-f011-b4cc-6045bdd69937",
                PluginName = "AccountV2",
                FriendlyName = "Account V2",
                Description = "This plugin provides account profiles (name, TPID, opportunities, priorities, estimated revenue) and general info when no other action applies.",
                BaseHttpEndpoint = "https://salescopilotpluginsnonprod.microsoft.com/uat/v2/skillstudio/",
                MethodEndpoint = "accountplugin",
                FunctionName = "GetAccountV2",
                InputParameter = "Extracted AccountId(GUID), Account Name (Company name), or TPID (Top parent ID), or all",
                OutputParameter = "Summarized information about profile/highlights of an account.",
                StateCode = 0,
                StatusCode = 1
            }
        };
    }
}
