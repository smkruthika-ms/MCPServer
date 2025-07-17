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
public sealed class ExtractContextTool
{
    private readonly SalesAgentPluginApiService _salesAgentPluginApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ExtractContextTool> _logger;
    private readonly IMcpServer _mcpServer;

    public ExtractContextTool(SalesAgentPluginApiService salesAgentPluginApiService, IHttpContextAccessor httpContextAccessor, ILogger<ExtractContextTool> logger)
    {
        _salesAgentPluginApiService = salesAgentPluginApiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
  

    private async Task<string> CallAgent(string plannerKey, string message, string token)
    {
        try
        {
            

            var response = await _salesAgentPluginApiService.ChatWithAgentAsync(plannerKey, message, token);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error : {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Extract context about an account")]
    public async Task<string> ExtractContextForAccount(IMcpServer thisServer, string input, CancellationToken cancellationToken)
    {
        return Environment.GetEnvironmentVariable("TPID") ?? "784852";
        //return await CallAgent("Account Summary", input, thisServer.ServerOptions.ServerInfo.Name);
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
