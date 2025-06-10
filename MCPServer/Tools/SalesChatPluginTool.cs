using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MCPServer.Services;

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

    private async Task<string> CallAgent(string plannerKey, string message)
    {
        try
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                _logger.LogInformation("McpServer Request Headers: {Headers}", string.Join(",", request.Headers.Select(h => $"{h.Key}")));
                var envHeader = request.Headers["x-ms-environment-id"].FirstOrDefault();
                if (envHeader != "892d452a-23d7-eb34-aece-cfc2c7dde70a")
                {
                    _logger.LogWarning("Request blocked: Invalid or missing x-ms-environment-id header. Value received: {EnvHeader}", envHeader);
                    throw new UnauthorizedAccessException("Invalid or missing x-ms-environment-id header.");
                }
            }
            else
            {
                _logger.LogInformation("No HttpContext or Request available in tool.");
            }
            var response = await _salesAgentPluginApiService.ChatWithAgentAsync(plannerKey, message);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error : {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Summarize information about profile/highlights of an account")]
    public async Task<string> SummarizeAccountProfile(string message)
    {
        return await CallAgent("Account Summary", message);
    }

    [McpServerTool, Description("Account News Highlights for the Account")]
    public async Task<string> GetAccountNews(string message)
    {
        return await CallAgent("DraupAccountNews", message);
    }

    [McpServerTool, Description("Summarize the raw data about list of top competitors of an account in a way that is presentable to a user")]
    public async Task<string> GetAccountCompetitor(string message)
    {
        return await CallAgent("Account Competitor", message );
    }

    [McpServerTool, Description("Fetches and displays the list of account team members working on an account")]
    public async Task<string> GetAccountTeam(string message)
    {
        return await CallAgent("GetAccountTeamM365", message);
    }

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



    
}
