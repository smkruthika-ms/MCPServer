using MCPServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Logging;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public sealed class SalesChatPluginTool
{
    private readonly SalesAgentPluginApiService _salesAgentPluginApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SalesChatPluginTool> _logger;

    public SalesChatPluginTool(SalesAgentPluginApiService salesAgentPluginApiService, IHttpContextAccessor httpContextAccessor, ILogger<SalesChatPluginTool> logger)
    {
        _salesAgentPluginApiService = salesAgentPluginApiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public async Task<string> ChatWithAgentAsync(
        string plannerKey,
        string message)
    {
        _logger.LogInformation("Starting ChatWithAgentAsync with PlannerKey: {PlannerKey}", plannerKey);

        string AuthToken = "";
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
        if (string.IsNullOrEmpty(AuthToken))
        {
            _logger.LogWarning("AuthToken is empty, attempting to acquire token.");
            _logger.LogInformation("App {Id} and Scope {Scope} will be used for token acquisition.", 
                Environment.GetEnvironmentVariable("AppId"), Environment.GetEnvironmentVariable("AppScope"));
            
            AuthToken = await GetTokenAsync(new [] { Environment.GetEnvironmentVariable("AppScope") });
            if (string.IsNullOrEmpty(AuthToken))
            {
                _logger.LogError("Failed to acquire AuthToken.");
                throw new InvalidOperationException("Sales Agent Plugin token could not be acquired.");
            } else
            {
                _logger.LogInformation("Token successfully acquired. {count}", AuthToken.Length);
                
            }
        }
        request.Headers.Add("Authorization", $"Bearer {AuthToken}");

        _logger.LogInformation("Sending request to Sales Agent Plugin API.");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Received response from Sales Agent Plugin API.");
        return result;
    }

    public async Task<string?> GetTokenAsync(IReadOnlyCollection<string> scopes)
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

                result = await _confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
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

    private async Task<string> CallAgent(string plannerKey, string message)
    {
        try
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var token = "";
            if (request != null)
            {
                _logger.LogInformation("McpServer Request Headers: {Headers}", string.Join(",", request.Headers.Select(h => $"{h.Key}")));
                var envHeader = request.Headers["x-ms-environment-id"].FirstOrDefault();
                if (envHeader != "892d452a-23d7-eb34-aece-cfc2c7dde70a")
                {
                    _logger.LogWarning("Request blocked: Invalid or missing x-ms-environment-id header. Value received: {EnvHeader}", envHeader);
                    // throw new UnauthorizedAccessException("Invalid or missing x-ms-environment-id header.");
                }
                var authHeader = request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }
            else
            {
                _logger.LogInformation("No HttpContext or Request available in tool.");
            }

            var response = await ChatWithAgentAsync(plannerKey, message);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error : {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Summarize information about profile/highlights of an account")]
    public async Task<string> SummarizeAccountProfile(string input)
    {
        return await CallAgent("Account Summary", input);
    }

    [McpServerTool, Description("Account News Highlights for the Account")]
    public async Task<string> GetAccountNews(string input)
    {
        return await CallAgent("DraupAccountNews", input);
    }

    [McpServerTool, Description("Summarize the raw data about list of top competitors of an account in a way that is presentable to a user")]
    public async Task<string> GetAccountCompetitor(string input)
    {
        return await CallAgent("Account Competitor", input);
    }

    [McpServerTool, Description("Fetches and displays the list of account team members working on an account")]
    public async Task<string> GetAccountTeam(string input)
    {
        return await CallAgent("GetAccountTeamM365", input);
    }
    /*
    [McpServerTool, Description("Data related to Agreement, Enrollment, deal book and license details for a specific agreement Id/agreement Number or Account/TPID. It provides detailed insights on various elements of agreement such as - applied discounts, price details, quantity, related SKUs details, license estates, amendment details, product family details.")]
    public async Task<string> GetAgreementDetails(string message)
    {
        return await CallAgent("Commercial Executive Plugin", message);
    }

    [McpServerTool, Description("ACR stands for Azure Consumption Revenue, also referred to as Azure Resource Consumption or Azure Consumption. This can answer about anything related to ACU, invoiced usage, Advisor Score, Azure spend, ECIF, consumption units, MACC, reservation recommendations, Saving Plan, Azure churn risk, Azure credits, and ACO. To use this action, use a full sentence rather than a company name alone. If multiple company names are given, then make separate call to the action one with each company name. If more than one fiscal year (FY) is mentioned, then make separate call to action one with each fiscal year or FY.")]
    public async Task<string> GetAccountConsumptionRevenue(string message)
    {
        return await CallAgent("ACR", message);
    }
    */

    
}
