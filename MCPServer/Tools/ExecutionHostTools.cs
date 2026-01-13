using ModelContextProtocol.Server;
using System.ComponentModel;
using MCPServer.Services;
using MCPServer.Models;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public sealed class ExecutionHostTools
{
    private readonly ExecutionHostService _executionHostService;

    public ExecutionHostTools(ExecutionHostService executionHostService)
    {
        _executionHostService = executionHostService;
    }

    [McpServerTool, Description("The plugin is designed to provide information on competitors, including top competitors, key account competitors, and competitors for specific TPIDs. Who are the top competitors?")]
    public async Task<HttpResponseData> Account_Competitor(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Account_Competitor", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This skill can help answer partner related queries and search partners with specific skills, endorsement. Can answer partner related questions about GSI, prioritized, QRP for solution area, solution play or customer segment endorsement.")]
    public async Task<HttpResponseData> GetPartnerDetails(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("GetPartnerDetails", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides users insight and detail on Customer Engagement workflows through the CEHub platform. Provides context for executive briefings, innovation hubs and immersive experience engagements.")]
    public async Task<HttpResponseData> CEHub_Copilot(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("CEHub_Copilot", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides summary of marketing interactions (MI) and activities, including insights. Users can fetch and view a summary of marketing interactions, as well as specific insights and data for a particular TPID.")]
    public async Task<HttpResponseData> MI_Summary(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("MI_Summary", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin can provide recommended programs and offers, eligible programs and offers, available programs and offers for a given opportunity id. Can show recommended, eligible or AI recommended offers and programs.")]
    public async Task<HttpResponseData> Deal_Assistance_Programs_Recommender(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Deal_Assistance_Programs_Recommender", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This skill answers questions on Azure metrics including Azure Consumed Revenue(ACR), Microsoft Azure Consumption Commitment (MACC), MACC Recommendations, AHB (Azure Hybrid Benefit) impact, credit, RI (Reserved Instances), SP (Savings Plan), ACD (Azure Commitment Discount).")]
    public async Task<HttpResponseData> ACR(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("ACR", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides the categories and list of prompts on the basis of logged in user. Returns prompts as actions. Can show personalized prompt picks and explore prompts curated for you.")]
    public async Task<HttpResponseData> LeadingPrompts(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("LeadingPrompts", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This action can answer anything related to opportunities and pipelines. Provides information on recent opportunities, top opptys, hygiene issues, hygiene summary, deal team of opportunity, opportunity owners, renewal opportunities.")]
    public async Task<HttpResponseData> Generic_Opportunity_Plugin(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Generic_Opportunity_Plugin", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin answers questions about Microsoft's public product terms and licensing documents and will supply a cited response. Questions can be about commercial products and will focus on general product terms or licensing scenarios.")]
    public async Task<HttpResponseData> Commercial_Licensing_QA(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Commercial_Licensing_QA", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides account profiles (name, TPID, GPID, AccountID, opportunities, priorities, estimated revenue) and general info. Handles account ownership queries and region-based account lookups. Gives account team structures and insights.")]
    public async Task<HttpResponseData> Account_V2(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Account_V2", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin offers a comprehensive collection of resources on products and solutions required by Microsoft sellers including Azure and Microsoft 365, including success stories and battlecards. Provides content, decks, Content Hub, and MSX Content.")]
    public async Task<HttpResponseData> Content(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Content", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides earnings, attainment, ITA, actuals, quota and payline details for seller's bucket for the fiscal year. Compute estimated earnings and attainment based on bucket name and potential revenue. Shows payouts and projected payouts.")]
    public async Task<HttpResponseData> Earnings_Estimation(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Earnings_Estimation", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("The plugin provides customer engaged contacts (employees or stakeholders or executives) in an account. Provides sales engagement summary of contact and account. Can provide when the contact was last contacted or engaged.")]
    public async Task<HttpResponseData> GetEngagedContacts(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("GetEngagedContacts", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Provides guidance and insights for sellers on various topics and sales strategies. Helps with seller enablement and best practices.")]
    public async Task<HttpResponseData> SellerICGuide(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("SellerICGuide", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides the list of recommendations based on user ask. Recommendation in this context is a CRM entity. Supports actions on recommendations restricted to Accept, Decline and Draft Email.")]
    public async Task<HttpResponseData> FetchRecommendationsV2(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("FetchRecommendationsV2", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin can provide context and key points for meeting with a customer or an account. Assists with preparing for upcoming meetings with customers and accounts including details like open support cases.")]
    public async Task<HttpResponseData> Pre_meeting_Brief(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Pre_meeting_Brief", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Provides commercial insights and executive-level information for business operations. Helps with commercial strategy and decision-making.")]
    public async Task<HttpResponseData> Commercial_Executive_Plugin(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Commercial_Executive_Plugin", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Provides recommendations and insights for RAIN (Revenue Acceleration and Intelligence Network) activities. Helps with sales strategies and customer engagement.")]
    public async Task<HttpResponseData> RAIN_Copilot(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("RAIN_Copilot", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This action provides detailed insights on milestones associated with both opportunities and accounts. Handles queries related to milestone progress such as recent updates, status-based milestones (at-risk, on-track, blocked).")]
    public async Task<HttpResponseData> GenericMilestonePlugin(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("GenericMilestonePlugin", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This action can answer anything related to Billed pipeline metrics. Provides information on Committed Pipeline, Upside Pipeline, Qualified Pipeline, Win rate, Close rate, Actual Revenue Annualized, and Billed forecasts.")]
    public async Task<HttpResponseData> CXP_Billed_Pipeline_Skill(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("CXP_Billed_Pipeline_Skill", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This skill is for enabling natural language querying over Github Pipeline and Product Usage data. Provides details on Github opportunities, Account ownerships, pipeline details and product usage.")]
    public async Task<HttpResponseData> Github_Account_Skill(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Github_Account_Skill", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Returns a list of upsell and cross-sell recommendations for an account based on Stanley Model and Rooms of the House (ROTH). Provides solution play recommendations for ROTH, solution plays, Azure Landscape, and growth opportunities.")]
    public async Task<HttpResponseData> ROTHV3(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("ROTHV3", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This tool handles create, update operations of opportunities along with setting up hygiene agent for opportunities. Supports creating new opportunities and updating opportunity details.")]
    public async Task<HttpResponseData> Opportunity_Actions(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Opportunity_Actions", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Provides commercial action capabilities for executive-level operations. Helps with commercial decisions, actions, and business operations.")]
    public async Task<HttpResponseData> CommercialExecutiveActionsPlugin(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("CommercialExecutiveActionsPlugin", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("Provides guidance and operational details for programs and offers including ECIF, AMM, Azure Innovate, Solution Assessment, Fastrack, Azure Access, ACO, Azure Migrate, ESI, and Customer Advocacy.")]
    public async Task<HttpResponseData> Deal_Assistance_Program_Guide(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Deal_Assistance_Program_Guide", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This is a generic contact plugin that will help answer queries on customer contacts based on variety of filters such as account, job role, country, etc. Provides comprehensive 360 degree view of contacts with engagement information.")]
    public async Task<HttpResponseData> GenericContactPlugin(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("GenericContactPlugin", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin can provide information on offers, delivery and deployment options, and value realization strategies for various sectors including business apps, Data and AI, Modern Work, Infrastructure, and Security.")]
    public async Task<HttpResponseData> Realized_Value_Catalog(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("Realized_Value_Catalog", userPrompt, token, cancellationToken);
    }

    [McpServerTool, Description("This plugin provides the Summary of recommendations or Suggestions owned by a user. Can also summarize or research a particular recommendation and show recommendation details.")]
    public async Task<HttpResponseData> GetRecommendationSummary(IMcpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        return await _executionHostService.CallExecutionHostAsync("GetRecommendationSummary", userPrompt, token, cancellationToken);
    }
}
