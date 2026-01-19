using MCPServer.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    private const string PluginsCacheKey = "mcp_plugins_cache";
    private const int CacheDurationMinutes = 60;

    public DataversePluginRegistryService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<DataversePluginRegistryService> logger,
        DataverseApiService dataverseService)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _dataverseService = dataverseService;
    }

    /// <summary>
    /// Fetches all active plugins from Dataverse
    /// </summary>
    public async Task<IEnumerable<PluginInfo>> GetAllPluginsAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[REGISTRY] GetAllPluginsAsync called with token: {(!string.IsNullOrEmpty(token) ? "YES ✓" : "NO ✗")}");
        
        Console.WriteLine($"[REGISTRY] Cache miss - fetching from API");
        try
        {
            // Fetch from plugin API
            var plugins = await FetchPluginsFromDataverseAsync(token, cancellationToken);
            var activePlugins = plugins//.Where(p => p.IsActive)
            .ToList();

            Console.WriteLine($"[REGISTRY] Fetched {activePlugins.Count} plugins from Dataverse");
            _logger.LogInformation("Fetched {PluginCount} active plugins from Dataverse", activePlugins.Count);

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));
            _cache.Set(PluginsCacheKey, (IEnumerable<PluginInfo>)activePlugins, cacheOptions);

            return activePlugins;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REGISTRY] Error fetching plugins: {ex.Message}");
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
            p.FunctionName?.Equals(pluginName, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Invokes a plugin with the given parameters
    /// </summary>
    public async Task<object?> InvokePluginAsync(
        string pluginName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var plugin = await GetPluginByNameAsync(pluginName, null, cancellationToken);
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
            var endpoint = plugin.GetPluginEndpoint();
            _logger.LogInformation("Invoking plugin {PluginName} at {Endpoint}", pluginName, endpoint);

            var request = new PluginInvocationRequest
            {
                FunctionName = plugin.FunctionName,
                Input = parameters
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(endpoint, jsonContent, cancellationToken);

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

    private async Task<IEnumerable<PluginInfo>> FetchPluginsFromDataverseAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[API CALL] FetchPluginsFromDataverseAsync called");
            Console.WriteLine($"[API CALL] Token provided: {(!string.IsNullOrEmpty(token) ? "YES ✓" : "NO ✗")}");
            
            // Create a new HttpClient instance for this request to avoid header conflicts
            using var client = new HttpClient();
            
            // Add authorization header if token is provided
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[API CALL] ========== TOKEN PROCESSING START ==========");
                Console.WriteLine($"[API CALL] ORIGINAL TOKEN (length={token.Length}):");
                Console.WriteLine(token);
                
                var cleanToken = token;
                if (cleanToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    cleanToken = cleanToken.Substring(7); // Remove "Bearer " (7 characters)
                    Console.WriteLine($"[API CALL] After removing 'Bearer ' (length={cleanToken.Length}):");
                    Console.WriteLine(cleanToken);
                }
                cleanToken = cleanToken.Trim();
                Console.WriteLine($"[API CALL] After Trim (length={cleanToken.Length}):");
                Console.WriteLine(cleanToken);
                
                // DO NOT REMOVE FIRST AND LAST CHARACTER - that was breaking the token!
                Console.WriteLine($"[API CALL] Using token as-is (no char removal)");
                Console.WriteLine($"[API CALL] Final token to send (length={cleanToken.Length}):");
                Console.WriteLine(cleanToken);
                Console.WriteLine($"[API CALL] Token cleaned and Authorization header added");
                _logger.LogInformation("Adding Authorization header to plugin API request");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                Console.WriteLine($"[API CALL] ========== TOKEN PROCESSING END ==========");
            }
            else
            {
                Console.WriteLine($"[API CALL] NO token provided - request will go without Authorization header");
                _logger.LogInformation("No token provided for plugin API request");
            }

            // Call the plugin API endpoint
            const string pluginApiUrl = "https://salescopilotskeusuat.azurewebsites.net/api/plugins";
            Console.WriteLine($"[API CALL] Making HTTP GET request to: {pluginApiUrl}");
            _logger.LogInformation("Fetching plugins from: {PluginApiUrl}", pluginApiUrl);
            
            var response = await client.GetAsync(pluginApiUrl, cancellationToken);
            Console.WriteLine($"[API CALL] Response received: Status={response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"[API CALL] ERROR: API returned {response.StatusCode}");
                Console.WriteLine($"[API CALL] Error response: {errorContent}");
                _logger.LogWarning("Failed to fetch plugins from API: {StatusCode}. Using mock data. Error: {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                // Return mock data when API fails
                return [];// GetMockPlugins();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[API CALL] Success! Received {content.Length} characters from API");
            Console.WriteLine($"[API CALL] ========== FULL API RESPONSE START ==========");
            Console.WriteLine(content);
            Console.WriteLine($"[API CALL] ========== FULL API RESPONSE END ==========");
            _logger.LogInformation("Received plugin API response: {ResponseLength} characters", content.Length);
            
            Console.WriteLine($"[API CALL] Attempting to deserialize as PluginListResponse...");
            var pluginResponse = JsonSerializer.Deserialize<PluginListResponse>(content);
            Console.WriteLine($"[API CALL] Deserialization result: pluginResponse = {(pluginResponse == null ? "NULL" : "NOT NULL")}");
            
            if (pluginResponse != null)
            {
                var plugins = pluginResponse.GetPlugins();
                var pluginCount = plugins.Count;
                Console.WriteLine($"[API CALL] Deserialized {pluginCount} plugins from response");
                
                if (pluginCount > 0)
                {
                    Console.WriteLine($"[API CALL] Response object details:");
                    Console.WriteLine($"  - Plugins: {pluginCount} items");
                    foreach (var plugin in plugins)
                    {
                        Console.WriteLine($"    * {plugin.PluginName} ({plugin.FunctionName})");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[API CALL] pluginResponse is NULL - deserialization failed!");
                Console.WriteLine($"[API CALL] Trying alternate deserialization to debug...");
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
                    Console.WriteLine($"[API CALL] JSON parsed successfully. Root element type: {jsonDoc.RootElement.ValueKind}");
                    Console.WriteLine($"[API CALL] Root element keys: {string.Join(", ", jsonDoc.RootElement.EnumerateObject().Select(p => p.Name))}");
                }
                catch (Exception parseEx)
                {
                    Console.WriteLine($"[API CALL] Failed to parse JSON: {parseEx.Message}");
                }
            }

            return pluginResponse?.GetPlugins() ?? [];
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[API CALL] HTTP Exception: {ex.Message}");
            _logger.LogError(ex, "HTTP error fetching plugins from API, using mock data");
            // Return mock data on HTTP errors
            return [];//GetMockPlugins();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API CALL] Unexpected exception: {ex.Message}");
            throw;
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
