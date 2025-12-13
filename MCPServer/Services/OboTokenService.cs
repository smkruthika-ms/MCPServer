using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Collections.Concurrent;

namespace MCPServer.Services;

/// <summary>
/// Service that handles On-Behalf-Of (OBO) token acquisition for downstream MCP servers
/// </summary>
public class OboTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OboTokenService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // Cache for confidential client applications (one per downstream server)
    private readonly ConcurrentDictionary<string, IConfidentialClientApplication> _confidentialClients = new();
    
    // In-memory token cache (consider Redis for production scale)
    private readonly ConcurrentDictionary<string, TokenCacheEntry> _tokenCache = new();

    public OboTokenService(
        IConfiguration configuration,
        ILogger<OboTokenService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Acquire an access token for a downstream service using OBO flow
    /// </summary>
    /// <param name="serverName">Name of the downstream server (matches config)</param>
    /// <param name="incomingToken">The token received from Teams/Copilot</param>
    /// <returns>Access token for the downstream service</returns>
    public async Task<string> GetAccessTokenAsync(string serverName, string incomingToken)
    {
        _logger.LogInformation("[OBO] ========== START OBO TOKEN ACQUISITION for {ServerName} ==========", serverName);
        
        var oboConfig = GetOboConfiguration(serverName);
        if (oboConfig == null || !oboConfig.Enabled)
        {
            _logger.LogError("[OBO] ❌ OBO not configured or not enabled for server: {ServerName}", serverName);
            throw new InvalidOperationException($"OBO is not configured or enabled for server '{serverName}'");
        }
        
        _logger.LogInformation(
            "[OBO] Configuration loaded | Enabled={Enabled} | UseManagedIdentity={UseMI} | ManagedIdentityClientId={MIClientId} | ClientId={ClientId} | TenantId={TenantId} | Scopes={Scopes}",
            oboConfig.Enabled,
            oboConfig.UseManagedIdentity,
            oboConfig.ManagedIdentityClientId ?? "(none)",
            oboConfig.ClientId,
            oboConfig.TenantId,
            string.Join(", ", oboConfig.Scopes));

        // Check cache first
        var cacheKey = GenerateCacheKey(serverName, incomingToken);
        _logger.LogDebug("[OBO] Checking cache with key: {CacheKey}", cacheKey);
        
        if (_tokenCache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
        {
            _logger.LogInformation(
                "[OBO] ✅ Using cached token for {ServerName} | ExpiresOn={ExpiresOn} | MinutesRemaining={Minutes}",
                serverName,
                cachedEntry.ExpiresOn,
                (cachedEntry.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes);
            return cachedEntry.AccessToken;
        }
        else if (_tokenCache.ContainsKey(cacheKey))
        {
            _logger.LogInformation(
                "[OBO] ⏰ Cached token expired for {ServerName}, acquiring new token",
                serverName);
        }
        else
        {
            _logger.LogInformation("[OBO] 🆕 No cached token found for {ServerName}, acquiring new token", serverName);
        }

        try
        {
            // Get or create confidential client app
            _logger.LogInformation("[OBO] Getting or creating confidential client app for {ServerName}", serverName);
            var app = GetOrCreateConfidentialClientApp(serverName, oboConfig);

            // Perform OBO token exchange
            _logger.LogInformation(
                "[OBO] 🔄 Calling AcquireTokenOnBehalfOf | Scopes={Scopes}",
                string.Join(", ", oboConfig.Scopes));
                
            var result = await app
                .AcquireTokenOnBehalfOf(oboConfig.Scopes, new UserAssertion(incomingToken))
                .ExecuteAsync();
                
            _logger.LogInformation("[OBO] ✅ AcquireTokenOnBehalfOf succeeded");

            _logger.LogInformation(
                "[OBO] Successfully acquired token for {ServerName} | Scopes={Scopes} | ExpiresOn={ExpiresOn}",
                serverName,
                string.Join(", ", oboConfig.Scopes),
                result.ExpiresOn);

            // ⚠️ SECURITY WARNING: Full token logging enabled for debugging - REMOVE IN PRODUCTION
            _logger.LogWarning(
                "[OBO] 🔓 OBO TOKEN for {ServerName} (FULL): {Token}",
                serverName,
                result.AccessToken);

            // Cache the token
            _tokenCache[cacheKey] = new TokenCacheEntry
            {
                AccessToken = result.AccessToken,
                ExpiresOn = result.ExpiresOn
            };

            return result.AccessToken;
        }
        catch (MsalUiRequiredException ex)
        {
            _logger.LogError(ex,
                "[OBO] User interaction required for {ServerName} - this should not happen in OBO flow",
                serverName);
            throw new InvalidOperationException(
                $"User consent required for '{serverName}'. Ensure admin consent is granted.", ex);
        }
        catch (MsalServiceException ex)
        {
            _logger.LogError(ex,
                "[OBO] Failed to acquire token for {ServerName} | Error={Error} | ErrorCode={ErrorCode}",
                serverName, ex.Message, ex.ErrorCode);
            throw new InvalidOperationException(
                $"Failed to acquire OBO token for '{serverName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract the incoming bearer token from the current HTTP request
    /// </summary>
    public string? GetIncomingToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogError("[OBO] ❌ HttpContext is NULL - cannot extract incoming token");
            return null;
        }

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        
        _logger.LogWarning(
            "[OBO] 🔍 AUTHORIZATION HEADER (RAW): '{AuthHeader}' | Length={Length}",
            authHeader ?? "(null)",
            authHeader?.Length ?? 0);
        
        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogError("[OBO] ❌ No Authorization header found in request");
            
            // Log all headers for debugging
            _logger.LogWarning("[OBO] Available headers: {Headers}",
                string.Join(", ", httpContext.Request.Headers.Keys));
            
            return null;
        }
        
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "[OBO] ❌ Authorization header does not start with 'Bearer ' | Header='{Header}'",
                authHeader);
            return null;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        _logger.LogWarning(
            "[OBO] ✅ Token extracted | Length={Length} | First50Chars={Preview}",
            token.Length,
            token.Length > 50 ? token.Substring(0, 50) : token);
        
        // ⚠️ SECURITY WARNING: Full token logging enabled for debugging - REMOVE IN PRODUCTION
        _logger.LogWarning(
            "[OBO] 🔓 INCOMING TOKEN (FULL): {Token}",
            token);
        
        return token;
    }

    /// <summary>
    /// Get or create a confidential client application for a downstream server
    /// </summary>
    private IConfidentialClientApplication GetOrCreateConfidentialClientApp(string serverName, OboConfiguration config)
    {
        return _confidentialClients.GetOrAdd(serverName, _ =>
        {
            var builder = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{config.TenantId}"));

            // Use Managed Identity if enabled, otherwise use Client Secret
            if (config.UseManagedIdentity)
            {
                _logger.LogInformation(
                    "[OBO] 🔐 Using Managed Identity authentication | ManagedIdentityClientId={MIClientId}",
                    config.ManagedIdentityClientId ?? "System-assigned");
                    
                // Use Managed Identity (User-assigned or System-assigned)
                TokenCredential credential;
                if (string.IsNullOrEmpty(config.ManagedIdentityClientId))
                {
                    // System-assigned MI
                    credential = new DefaultAzureCredential();
                    _logger.LogInformation("[OBO] Created credential | Type=DefaultAzureCredential");
                }
                else
                {
                    // User-assigned MI
                    credential = new ManagedIdentityCredential(config.ManagedIdentityClientId);
                    _logger.LogInformation("[OBO] Created credential | Type=ManagedIdentityCredential");
                }

                // Get access token for Microsoft Graph to use as client credential
                var tokenRequestContext = new TokenRequestContext(
                    new[] { "https://graph.microsoft.com/.default" });
                
                // Note: For OBO with Managed Identity, we use certificate or federated credential
                // This is a workaround - ideally use certificate-based auth
                builder.WithClientAssertion(async (AssertionRequestOptions options) =>
                {
                    _logger.LogDebug("[OBO] Acquiring client assertion token from Managed Identity");
                    var token = await credential.GetTokenAsync(tokenRequestContext, default);
                    _logger.LogDebug("[OBO] ✅ Client assertion token acquired | ExpiresOn={ExpiresOn}", token.ExpiresOn);
                    return token.Token;
                });

                _logger.LogInformation(
                    "[OBO] Created confidential client with Managed Identity for {ServerName} | ClientId={ClientId} | ManagedIdentityClientId={ManagedIdentityClientId}",
                    serverName, config.ClientId, config.ManagedIdentityClientId ?? "System-assigned");
            }
            else if (!string.IsNullOrEmpty(config.ClientSecret))
            {
                _logger.LogInformation(
                    "[OBO] 🔑 Using Client Secret authentication | ClientId={ClientId}",
                    config.ClientId);
                    
                // Traditional client secret authentication
                builder.WithClientSecret(config.ClientSecret);

                _logger.LogInformation(
                    "[OBO] Created confidential client with Client Secret for {ServerName} | ClientId={ClientId}",
                    serverName, config.ClientId);
            }
            else
            {
                _logger.LogError(
                    "[OBO] ❌ No authentication method configured | ServerName={ServerName} | UseManagedIdentity={UseMI} | HasClientSecret={HasSecret}",
                    serverName,
                    config.UseManagedIdentity,
                    !string.IsNullOrEmpty(config.ClientSecret));
                    
                throw new InvalidOperationException(
                    $"OBO configuration for '{serverName}' must have either UseManagedIdentity=true or ClientSecret configured");
            }

            var app = builder.Build();

            // Enable in-memory token cache (for production, use distributed cache)
            app.AddInMemoryTokenCache();

            _logger.LogInformation(
                "[OBO] Confidential client created for {ServerName} | TenantId={TenantId}",
                serverName, config.TenantId);

            return app;
        });
    }

    /// <summary>
    /// Get OBO configuration for a specific downstream server
    /// </summary>
    private OboConfiguration? GetOboConfiguration(string serverName)
    {
        var servers = _configuration
            .GetSection("McpGateway:DownstreamServers")
            .Get<List<DownstreamServerConfig>>();

        return servers?.FirstOrDefault(s => s.Name == serverName)?.OboConfig;
    }

    /// <summary>
    /// Generate cache key for token storage
    /// </summary>
    private string GenerateCacheKey(string serverName, string incomingToken)
    {
        // Use hash of incoming token to avoid storing full token in cache key
        var tokenHash = incomingToken.GetHashCode().ToString();
        return $"{serverName}:{tokenHash}";
    }

    /// <summary>
    /// Background task to clean up expired tokens from cache
    /// </summary>
    public void CleanupExpiredTokens()
    {
        var expiredKeys = _tokenCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _tokenCache.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("[OBO] Cleaned up {Count} expired tokens from cache", expiredKeys.Count);
        }
    }
}

/// <summary>
/// Configuration for OBO authentication to a downstream server
/// </summary>
public class OboConfiguration
{
    /// <summary>
    /// Whether OBO is enabled for this server
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Client ID of your gateway application registration
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret for your gateway application (optional if using Managed Identity)
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Whether to use Managed Identity instead of Client Secret
    /// </summary>
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// Client ID of the User-Assigned Managed Identity (optional, uses system-assigned if not specified)
    /// Example: "99453c85-3919-4a0e-b5e1-691d6986b018"
    /// </summary>
    public string? ManagedIdentityClientId { get; set; }

    /// <summary>
    /// Tenant ID (directory ID)
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Scopes to request for the downstream service
    /// Example: ["api://downstream-app-id/.default"] or ["api://downstream-app-id/access_as_user"]
    /// </summary>
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Extended downstream server configuration with OBO support
/// </summary>
public class DownstreamServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string McpEndpoint { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string[] ToolNames { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// OBO configuration for this server (optional)
    /// </summary>
    public OboConfiguration? OboConfig { get; set; }
}

/// <summary>
/// Token cache entry with expiration
/// </summary>
internal class TokenCacheEntry
{
    public required string AccessToken { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }

    /// <summary>
    /// Check if token is expired (with 5 minute buffer)
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow.AddMinutes(5) >= ExpiresOn;
}
