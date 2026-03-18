using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using Microsoft.Identity.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCPServer.Services;

public class SalesAgentPluginApiService
{
    private readonly ILogger<SalesAgentPluginApiService> _logger;

    public SalesAgentPluginApiService(ILogger<SalesAgentPluginApiService> logger)
    {
        _logger = logger;
    }

    public async Task<object> ChatWithAgentObjectAsync(
     string plannerKey,
     string message, string token)
    {
        
        // Adaptive Card JSON template
        var adaptiveCardJson = "{\"type\":\"AdaptiveCard\",\"msteams\":{\"width\":\"full\"},\"version\":\"1.5\",\"body\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Prompts curated for Kruthika S M\",\"wrap\":true,\"weight\":\"Bolder\",\"size\":\"Large\"}],\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"Container\",\"height\":\"stretch\",\"verticalScroll\":true,\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"Contact - FY26 Red Carpet\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"Contact - FY26 Red Carpet\",\"displayName\":\"Show engaged contacts for Walmart Inc.\",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"Show engaged contacts for Walmart Inc.\\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\uD83D\\uDC51\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Contact - FY26 Red Carpet\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"Show engaged contacts for Walmart Inc.\",\"title\":\"Show engaged contacts for Walmart Inc.\",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"Pipeline - FY26 Red Carpet\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"Pipeline - FY26 Red Carpet\",\"displayName\":\"Do I have any expiring recommendations?\",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"Do I have any expiring recommendations?\\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\uD83D\\uDC51\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Pipeline - FY26 Red Carpet\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"Do I have any expiring recommendations?\",\"title\":\"Do I have any expiring recommendations?\",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"Pipeline - FY26 Red Carpet\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"Pipeline - FY26 Red Carpet\",\"displayName\":\"Give me the total pipeline in stage 1 for all of my accounts\",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"Give me the total pipeline in stage 1 for all of my accounts\\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\uD83D\\uDC51\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Pipeline - FY26 Red Carpet\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"Give me the total pipeline in stage 1 for all of my accounts\",\"title\":\"Give me the total pipeline in stage 1 for all of my accounts\",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"Open opportunities\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"Open opportunities\",\"displayName\":\"Show me my open opportunities\",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"Show me my open opportunities\\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\u2728\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Open opportunities\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"Show me my open opportunities\",\"title\":\"Show me my open opportunities\",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"ACR consumption\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"ACR consumption\",\"displayName\":\"Get latest ACR consumption for Walmart Inc.\",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"Get latest ACR consumption for Walmart Inc.\\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\u2728\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"ACR consumption\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"Get latest ACR consumption for Walmart Inc.\",\"title\":\"Get latest ACR consumption for Walmart Inc.\",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"207px\",\"items\":[{\"type\":\"Container\",\"style\":\"emphasis\",\"selectAction\":{\"type\":\"Action.Submit\",\"title\":\"Pipeline licensing\",\"data\":{\"promptId\":\"\",\"payload\":\"\",\"eventName\":\"Pipeline licensing\",\"displayName\":\"How much of the pipeline is associated with CSP licensing programs? \",\"source\":\"API\",\"msteams\":{\"type\":\"imBack\",\"value\":\"How much of the pipeline is associated with CSP licensing programs? \\u200B\"}}},\"width\":\"207px\",\"minWidth\":\"207px\",\"height\":\"stretch\",\"minHeight\":\"90px\",\"items\":[{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"\\u2728\",\"size\":\"Medium\",\"weight\":\"Bolder\",\"horizontalAlignment\":\"Center\",\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Pipeline licensing\",\"spacing\":\"None\",\"wrap\":true,\"size\":\"Small\",\"weight\":\"Bolder\",\"width\":\"stretch\",\"maxLines\":1}],\"spacing\":\"Small\",\"verticalContentAlignment\":\"Top\"}]},{\"type\":\"TextBlock\",\"text\":\"How much of the pipeline is associated with CSP licensing programs? \",\"title\":\"How much of the pipeline is associated with CSP licensing programs? \",\"wrap\":true,\"size\":\"Default\",\"isSubtle\":true,\"spacing\":\"Small\",\"maxLines\":3}],\"verticalContentAlignment\":\"Top\"}],\"verticalContentAlignment\":\"Top\"}]}]}],\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\"}";
        var h = "{ \"type\": \"AdaptiveCard\", \"msteams\": { \"width\": \"full\" }, \"version\": \"1.5\", \"body\": [ { \"type\": \"ColumnSet\", \"columns\": [ { \"type\": \"Column\", \"width\": \"stretch\", \"items\": [ { \"type\": \"TextBlock\", \"text\": \"Prompts curated for Kruthika S M\", \"wrap\": true, \"weight\": \"Bolder\", \"size\": \"Large\" } ], \"verticalContentAlignment\": \"Top\" } ] } ], \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\" }";

        var wrappedResult = new
        {
            messages = new[] { h },
            templates = new Dictionary<string, string> { { "0", h } },
            outputs = new
            {
                response = "Prompts curated for Kruthika S M, \uD83D\uDC51, Account - FY26 Red Carpet, Get latest news for account Walmart Inc., \uD83D\uDC51, Pipeline - FY26 Red Carpet, How much is the total qualified pipeline for tpid 784852  for current and next quarter? , \uD83D\uDC51, Pipeline - FY26 Red Carpet, Summarize opportunities for account Walmart Inc., \u2728, Content, Show me content recommendations for Walmart Inc., \u2728, Last opportunity, My last modified opportunity, \u2728, Account team, Who is the account team for  Walmart Inc.?",
                template_selector = "$.messages[0]"
            },
            cards = new[]
            {
                new
                {
                    template_selector = "$.templates.0",
                    title = "Prompts curated for Kruthika S M",
                    subtitle = "",
                    url = "",
                    thumbnailUrl = ""
                }
            }
        };
        
        _logger.LogInformation("Wrapped Received response from Sales Agent Plugin API., " + JsonSerializer.Serialize(wrappedResult));

        return wrappedResult;
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
            Source = "M365Agent"
        };
        var jsonBody = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://salescopilotskeusuat.azurewebsites.net/api/Playground");
        request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        
        if (!string.IsNullOrEmpty(AuthToken))
        {
            _logger.LogWarning("AuthToken is empty, attempting to acquire token.");
            _logger.LogInformation("App {Id} and Scope {Scope} will be used for token acquisition.", 
                Environment.GetEnvironmentVariable("AppId"), Environment.GetEnvironmentVariable("AppScope"));

            string[] scopes = new[] { "api://5d437e13-722e-4a8d-8f4f-3158cf570e94/.default" };


            var credential = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = "8dc5f85c-0433-4316-8d8a-350879a6f59c"
                    });
            AccessToken accessToken = await credential.GetTokenAsync(new TokenRequestContext(scopes));
            AuthToken = accessToken.Token;
            _logger.LogInformation("Token successfully acquired 1. {count}", AuthToken);

            var confidentialClient = ConfidentialClientApplicationBuilder.Create("972bf644-79c8-4dbc-9921-5040af077272")
           .WithClientAssertion(() =>
           {
               var managedIdentityApp = ManagedIdentityApplicationBuilder
                   .Create(ManagedIdentityId.WithUserAssignedClientId("8dc5f85c-0433-4316-8d8a-350879a6f59c"))
                   .Build();
               var managedIdentityAssertion = managedIdentityApp
                   .AcquireTokenForManagedIdentity("api://AzureADTokenExchange/.default")
                   .ExecuteAsync().GetAwaiter().GetResult();
               return managedIdentityAssertion.AccessToken;
           })
           .WithAuthority(new Uri("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/token"))
           .Build();


            var userAssertion = new UserAssertion(token);

            try
            {
                var result1 =  await confidentialClient
                .AcquireTokenOnBehalfOf(new[] { "api://5d437e13-722e-4a8d-8f4f-3158cf570e94/access_as_user" }, new UserAssertion(token))
                .ExecuteAsync();

                _logger.LogInformation("OBO token acquired successfully. Token Expiry: {ExpiryDate}", result1.ExpiresOn);
               // return result.AccessToken;
               AuthToken  = result1.AccessToken;

            }
            catch (MsalServiceException ex)
            {
                _logger.LogError(ex, "Failed to acquire OBO token.");
                return null;
            }

            //AuthToken = await GetTokenAsync(new[] { Environment.GetEnvironmentVariable("AppScope") }, token);
            if (string.IsNullOrEmpty(AuthToken))
            {
                _logger.LogError("Failed to acquire AuthToken.");
                throw new InvalidOperationException("Sales Agent Plugin token could not be acquired.");
            }
            else
            {
                _logger.LogInformation("Token successfully acquired 2. {count}", AuthToken);
                
            }
        }
        
        //AuthToken = token;// Environment.GetEnvironmentVariable("SALES_AGENT_PLUGIN_TOKEN");
        request.Headers.Add("Authorization", $"Bearer {AuthToken}");

        _logger.LogInformation("Sending request to Sales Agent Plugin API.");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Received response from Sales Agent Plugin API., " + result);
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
