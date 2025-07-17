using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Logging;

namespace MCPServer.Services;

public class SalesAgentPluginApiService
{
    private readonly ILogger<SalesAgentPluginApiService> _logger;

    public SalesAgentPluginApiService(ILogger<SalesAgentPluginApiService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ChatWithAgentAsync(
        string plannerKey,
        string message, string token)
    {
        _logger.LogInformation("Starting ChatWithAgentAsync with PlannerKey: {PlannerKey} {TokenLength}", plannerKey, token?.Length ?? 0);

        string AuthToken = token;
        var requestBody = new
        {
            InvokeAllFunctions = true,
            InvokeAllSkills = false,
            filterURLs = false,
            tracePlan = false,
            RequestId = Guid.NewGuid().ToString(),
            Value = message,
            Inputs = new[] {
                new { Key = "useOboTokenGeneration", Value = "true" },
                new { Key = "Debug", Value = "yes" },
                new { Key = "SendAdapativeCardFormat", Value = "yes" }
            },
            ExpeditionId = Guid.NewGuid().ToString(),
            UserPrompt = message,
            PlannerKey = plannerKey,
            Source = "MCPServer"
        };
        var jsonBody = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://salescopilotskeusuat.azurewebsites.net/api/Playground");
        request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        /*
        if (string.IsNullOrEmpty(AuthToken))
        {
            _logger.LogWarning("AuthToken is empty, attempting to acquire token.");
            _logger.LogInformation("App {Id} and Scope {Scope} will be used for token acquisition.", 
                Environment.GetEnvironmentVariable("AppId"), Environment.GetEnvironmentVariable("AppScope"));
            
            AuthToken = await GetTokenAsync(new [] { Environment.GetEnvironmentVariable("AppScope") }, token);
            if (string.IsNullOrEmpty(AuthToken))
            {
                _logger.LogError("Failed to acquire AuthToken.");
                throw new InvalidOperationException("Sales Agent Plugin token could not be acquired.");
            } else
            {
                _logger.LogInformation("Token successfully acquired. {count}", AuthToken.Length);
                
            }
        }
        */
        AuthToken = token;// Environment.GetEnvironmentVariable("SALES_AGENT_PLUGIN_TOKEN");
        request.Headers.Add("Authorization", $"Bearer {AuthToken}");

        _logger.LogInformation("Sending request to Sales Agent Plugin API.");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Received response from Sales Agent Plugin API.");
        return result;
    }

    // Modify GetTokenAsync to use On-Behalf-Of flow
    public async Task<string?> GetTokenAsync(IReadOnlyCollection<string> scopes, string userToken)
    {
        _logger.LogInformation("Starting GetTokenAsync with scopes: {Scopes}", string.Join(",", scopes));

        bool retry = false;
        var retryDueToException = 0;
        var retryDuetoInvalidOutput = 0;
        var tokenRetryCount = 3;
        do
        {
            AuthenticationResult? result = null;
            TimeSpan? delay = null;
            try
            {
                var _confidentialClient = ConfidentialClientApplicationBuilder.Create(Environment.GetEnvironmentVariable("AppId"))
                    .WithClientAssertion(new ManagedIdentityClientAssertion(Environment.GetEnvironmentVariable("ManagedIdentity")).GetSignedAssertionAsync)
                    .WithAuthority(new Uri("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47"))
                    .WithAzureRegion(ConfidentialClientApplication.AttemptRegionDiscovery)
                    .WithLegacyCacheCompatibility()
                    .Build();

                var userAssertion = new UserAssertion(userToken);
                result = await _confidentialClient.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                retryDueToException = 0;

                if (result != null)
                {
                    _logger.LogInformation("Token acquired successfully. Token Expiry: {ExpiryDate}", result.ExpiresOn);
                }
            }
            catch (MsalServiceException serviceException)
            {
                _logger.LogError(serviceException, "MsalServiceException occurred while acquiring token.");
                if (serviceException.Headers?.RetryAfter != null)
                {
                    RetryConditionHeaderValue retryAfter = serviceException.Headers.RetryAfter;
                    if (retryAfter?.Delta.HasValue == true)
                    {
                        delay = retryAfter.Delta;
                    }
                    else if (retryAfter?.Date.HasValue == true)
                    {
                        delay = retryAfter.Date.Value.Offset;
                    }
                }
                else
                {
                    retryDueToException++;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred while acquiring token.");
                retryDueToException++;
            }

            if (delay.HasValue)
            {
                _logger.LogWarning("Retrying token acquisition after delay: {Delay}", delay.Value);
                Thread.Sleep((int)delay.Value.TotalMilliseconds);
                retry = true;
                continue;
            }

            if (retryDueToException > 0 && retryDueToException < tokenRetryCount && result == null)
            {
                _logger.LogWarning("Retrying token acquisition due to exception.");
                retry = true;
                continue;
            }

            if (((result == null) || (result != null && string.IsNullOrEmpty(result?.AccessToken))) && retryDuetoInvalidOutput < tokenRetryCount)
            {
                _logger.LogWarning("Retrying token acquisition due to invalid output.");
                retryDuetoInvalidOutput++;
                retry = true;
                continue;
            }

            return result?.AccessToken;
        } while (retry);

        _logger.LogError("Failed to acquire token after retries.");
        return null;
    }
}
