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
    private readonly IMcpServer _mcpServer;

    public SalesChatPluginTool(SalesAgentPluginApiService salesAgentPluginApiService, IHttpContextAccessor httpContextAccessor, ILogger<SalesChatPluginTool> logger)
    {
        _salesAgentPluginApiService = salesAgentPluginApiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    private async Task<object> CallAgentObject(string plannerKey, string message, string token)
    {
        try
        {
            var response = await _salesAgentPluginApiService.ChatWithAgentObjectAsync(plannerKey, message, token);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error : {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
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
    
    [McpServerTool, Description("Summarize information about profile/highlights of an account")]
    public async Task<string> SummarizeAccountProfile( IMcpServer thisServer,string userPrompt,string context,  CancellationToken cancellationToken)
    {
        _logger.LogInformation("Summarizing account profile for user prompt: {UserPrompt}", userPrompt, context);
        return await CallAgent("Account Summary", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }



    [McpServerTool, Description("Account News Highlights for the Account")]
    public async Task<string> GetAccountNews(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching account news for input: {Input}", userPrompt, context);
        return await CallAgent("DraupAccountNews", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }

    [McpServerTool, Description("Summarize the raw data about list of top competitors of an account in a way that is presentable to a user")]
    public async Task<string> GetAccountCompetitor(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        return await CallAgent("Account Competitor", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }

    [McpServerTool, Description("Fetches and displays the list of account team members working on an account")]
    public async Task<string> GetAccountTeam(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {   
        return await CallAgent("GetAccountTeamM365", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }
    
    /*
    [McpServerTool, Description("Data related to Agreement, Enrollment, deal book and license details for a specific agreement Id/agreement Number or Account/TPID. It provides detailed insights on various elements of agreement such as - applied discounts, price details, quantity, related SKUs details, license estates, amendment details, product family details.")]
    public async Task<string> GetAgreementDetails(string message)
    {
        return await CallAgent("Commercial Executive Plugin", message);
    }
    
    [McpServerTool, Description("This plugin provides the categories and list of prompts on the basis of logged in user. This plugin returns prompts as actions. This plugin can not summarize the response. There is only of input for this plugin where user wants to Explore prompts . This plugin cannot summarize response.")]
    public async Task<object> GetLeadingPrompts(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        return await CallAgentObject("LeadingPrompts", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }
    */

    
}
