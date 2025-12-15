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
        _logger.LogInformation("Starting ChatWithAgentAsync with PlannerKey: {PlannerKey} {TokenLength}", plannerKey, token?.Length ?? 0);
        var abcd = "{\"messages\":[\"{\\u0022type\\u0022:\\u0022AdaptiveCard\\u0022,\\u0022msteams\\u0022:{\\u0022width\\u0022:\\u0022full\\u0022},\\u0022version\\u0022:\\u00221.5\\u0022,\\u0022body\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Prompts curated for you\\u0022,\\u0022wrap\\u0022:true,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022size\\u0022:\\u0022Large\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022verticalScroll\\u0022:true,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022title\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Do I have any expiring recommendations?\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022title\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022title\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Open opportunities\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Open opportunities\\u0022,\\u0022displayName\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Show me my open opportunities\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Open opportunities\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022title\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022ACR consumption\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022ACR consumption\\u0022,\\u0022displayName\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022ACR consumption\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022title\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022displayName\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022title\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]}]}],\\u0022$schema\\u0022:\\u0022http://adaptivecards.io/schemas/adaptive-card.json\\u0022}\"],\"templates\":{\"0\":\"{\\u0022type\\u0022:\\u0022AdaptiveCard\\u0022,\\u0022msteams\\u0022:{\\u0022width\\u0022:\\u0022full\\u0022},\\u0022version\\u0022:\\u00221.5\\u0022,\\u0022body\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Prompts curated for Kruthika S M\\u0022,\\u0022wrap\\u0022:true,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022size\\u0022:\\u0022Large\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022verticalScroll\\u0022:true,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Contact - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022title\\u0022:\\u0022Show engaged contacts for Walmart Inc.\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Do I have any expiring recommendations?\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022title\\u0022:\\u0022Do I have any expiring recommendations?\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022displayName\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\uD83D\\uDC51\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline - FY26 Red Carpet\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022title\\u0022:\\u0022Give me the total pipeline in stage 1 for all of my accounts\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Open opportunities\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Open opportunities\\u0022,\\u0022displayName\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Show me my open opportunities\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Open opportunities\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022title\\u0022:\\u0022Show me my open opportunities\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022ACR consumption\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022ACR consumption\\u0022,\\u0022displayName\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022ACR consumption\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022title\\u0022:\\u0022Get latest ACR consumption for Walmart Inc.\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022Container\\u0022,\\u0022style\\u0022:\\u0022emphasis\\u0022,\\u0022selectAction\\u0022:{\\u0022type\\u0022:\\u0022Action.Submit\\u0022,\\u0022title\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022data\\u0022:{\\u0022promptId\\u0022:\\u0022\\u0022,\\u0022payload\\u0022:\\u0022\\u0022,\\u0022eventName\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022displayName\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022source\\u0022:\\u0022API\\u0022,\\u0022msteams\\u0022:{\\u0022type\\u0022:\\u0022imBack\\u0022,\\u0022value\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u200B\\u0022}}},\\u0022width\\u0022:\\u0022207px\\u0022,\\u0022minWidth\\u0022:\\u0022207px\\u0022,\\u0022height\\u0022:\\u0022stretch\\u0022,\\u0022minHeight\\u0022:\\u002290px\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022ColumnSet\\u0022,\\u0022columns\\u0022:[{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022auto\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022\\u2728\\u0022,\\u0022size\\u0022:\\u0022Medium\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022horizontalAlignment\\u0022:\\u0022Center\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022},{\\u0022type\\u0022:\\u0022Column\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022items\\u0022:[{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022Pipeline licensing\\u0022,\\u0022spacing\\u0022:\\u0022None\\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Small\\u0022,\\u0022weight\\u0022:\\u0022Bolder\\u0022,\\u0022width\\u0022:\\u0022stretch\\u0022,\\u0022maxLines\\u0022:1}],\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]},{\\u0022type\\u0022:\\u0022TextBlock\\u0022,\\u0022text\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022title\\u0022:\\u0022How much of the pipeline is associated with CSP licensing programs? \\u0022,\\u0022wrap\\u0022:true,\\u0022size\\u0022:\\u0022Default\\u0022,\\u0022isSubtle\\u0022:true,\\u0022spacing\\u0022:\\u0022Small\\u0022,\\u0022maxLines\\u0022:3}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}],\\u0022verticalContentAlignment\\u0022:\\u0022Top\\u0022}]}]}],\\u0022$schema\\u0022:\\u0022http://adaptivecards.io/schemas/adaptive-card.json\\u0022}\"},\"outputs\":{\"response\":\"Prompts curated for Kruthika S M, \\uD83D\\uDC51, Account - FY26 Red Carpet, Get latest news for account Walmart Inc., \\uD83D\\uDC51, Pipeline - FY26 Red Carpet, How much is the total qualified pipeline for tpid 784852  for current and next quarter? , \\uD83D\\uDC51, Pipeline - FY26 Red Carpet, Summarize opportunities for account Walmart Inc., \\u2728, Content, Show me content recommendations for Walmart Inc., \\u2728, Last opportunity, My last modified opportunity, \\u2728, Account team, Who is the account team for  Walmart Inc.?\",\"template_selector\":\"$.messages[0]\"},\"cards\":[{\"template_selector\":\"$.templates.0\",\"title\":\"Prompts curated for Kruthika S M\",\"subtitle\":\"\",\"url\":\"\",\"thumbnailUrl\":\"\"}]}";
        var deserialized = JsonSerializer.Deserialize<object>(abcd);

        return deserialized;
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
                var result1 = await confidentialClient
                .AcquireTokenOnBehalfOf(new[] { "api://5d437e13-722e-4a8d-8f4f-3158cf570e94/access_as_user" }, new UserAssertion(token))
                .ExecuteAsync();

                _logger.LogInformation("OBO token acquired successfully. Token Expiry: {ExpiryDate}", result1.ExpiresOn);
                // return result.AccessToken;
                AuthToken = result1.AccessToken;

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
        var resultJson = JsonSerializer.Deserialize<object>(result);

        // ✅ Return the actual object (so it's not stringified again)
        return resultJson;
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
            try
            {
                var confidentialClient = ConfidentialClientApplicationBuilder.Create("5d437e13-722e-4a8d-8f4f-3158cf570e94")
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


            var userAssertion = new UserAssertion(token.Trim());

            var result1 =  await confidentialClient
            .AcquireTokenOnBehalfOf(new[] { "api://5d437e13-722e-4a8d-8f4f-3158cf570e94/access_as_user" }, userAssertion)
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
