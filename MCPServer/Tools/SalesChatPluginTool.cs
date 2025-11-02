using MCPServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;
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
    public async Task<string> SummarizeAccountProfile(IHttpContextAccessor accessor, McpServer thisServer,string userPrompt,string context, CancellationToken cancellationToken, string plannerKey = "Account Summary")
    {
        _logger.LogInformation("Summarizing account profile for user prompt: {UserPrompt}, context: {context}, plannerKey = {plannerKey}", userPrompt, context, plannerKey);
        return await CallAgent(plannerKey, userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }



    [McpServerTool, Description("Account News Highlights for the Account")]
    public async Task<string> GetAccountNews(IHttpContextAccessor accessor, McpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken, string plannerKey = "DraupAccountNews")
    {
        _logger.LogInformation("Fetching account news for input: {Input}, context: {context}, plannerKey = {plannerKey}", userPrompt, context, plannerKey);
        // Minimal snippet to dump (almost) everything from IHttpContextAccessor.
        // Paste this inside a method where you have access to `IHttpContextAccessor accessor`.
        // Produces a string; you can Console.WriteLine / log it as needed.

     

        var ctx = accessor.HttpContext;
        if (ctx == null)
        {
            Console.WriteLine("HttpContext is null");
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== REQUEST ===");
            sb.AppendLine($"Protocol: {ctx.Request.Protocol}");
            sb.AppendLine($"Method:   {ctx.Request.Method}");
            sb.AppendLine($"Scheme:   {ctx.Request.Scheme}");
            sb.AppendLine($"Host:     {ctx.Request.Host}");
            sb.AppendLine($"PathBase: {ctx.Request.PathBase}");
            sb.AppendLine($"Path:     {ctx.Request.Path}");
            sb.AppendLine($"QueryStr: {ctx.Request.QueryString}");

            sb.AppendLine("Query:");
            foreach (var kv in ctx.Request.Query)
                _ = sb.AppendLine($"  {kv.Key}: {string.Join(", ", kv.Value.ToArray())}");

            sb.AppendLine("Route Values:");
            if (ctx.Request.RouteValues != null && ctx.Request.RouteValues.Count > 0)
            {
                foreach (var kv in ctx.Request.RouteValues)
                    sb.AppendLine($"  {kv.Key}: {kv.Value}");
            }
            else sb.AppendLine("  (none)");

            sb.AppendLine("Headers:");
            foreach (var h in ctx.Request.Headers)
                sb.AppendLine($"  {h.Key}: {h.Value}");

            sb.AppendLine("Cookies:");
            if (ctx.Request.Cookies is { Count: > 0 })
            {
                foreach (var c in ctx.Request.Cookies)
                    sb.AppendLine($"  {c.Key}: {c.Value}");
            }
            else sb.AppendLine("  (none)");

            try
            {
                if (ctx.Request.HasFormContentType)
                {
                    var form = ctx.Request.ReadFormAsync().GetAwaiter().GetResult();
                    sb.AppendLine("Form Fields:");
                    foreach (var f in form)
                        _ = sb.AppendLine($"  {f.Key}: {string.Join(", ", f.Value.ToArray())}");

                    if (form.Files?.Count > 0)
                    {
                        sb.AppendLine("Form Files:");
                        foreach (var file in form.Files)
                            sb.AppendLine($"  {file.Name}: {file.FileName} ({file.ContentType}, {file.Length} bytes)");
                    }
                }
            }
            catch { /* ignore form read errors */ }

            try
            {
                ctx.Request.EnableBuffering(); // requires Microsoft.AspNetCore.Http
                if (ctx.Request.Body.CanSeek)
                {
                    ctx.Request.Body.Position = 0;
                    using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
                    var bodyText = reader.ReadToEnd();
                    sb.AppendLine("Body:");
                    sb.AppendLine(string.IsNullOrEmpty(bodyText) ? "  (empty)" : bodyText);
                    ctx.Request.Body.Position = 0; // reset for downstream
                }
                else
                {
                    sb.AppendLine("Body: (not seekable)");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Body: (error reading) {ex.Message}");
            }

            sb.AppendLine("\n=== USER ===");
            var identity = ctx.User?.Identity;
            sb.AppendLine($"Authenticated: {identity?.IsAuthenticated ?? false}");
            sb.AppendLine($"Name:         {identity?.Name ?? "(null)"}");
            sb.AppendLine("Claims:");
            if (ctx.User?.Claims != null && ctx.User.Claims.Any())
            {
                foreach (Claim claim in ctx.User.Claims)
                    sb.AppendLine($"  {claim.Type}: {claim.Value}");
            }
            else sb.AppendLine("  (none)");

            sb.AppendLine("\n=== SESSION ===");
            if (ctx.Session != null)
            {
                try
                {
                    sb.AppendLine("Keys:");
                    foreach (var key in ctx.Session.Keys)
                    {
                        string? val = null;
                        try { val = ctx.Session.GetString(key); } catch { /* ignore */ }
                        sb.AppendLine($"  {key}: {val}");
                    }
                }
                catch { sb.AppendLine("  (error reading session)"); }
            }
            else sb.AppendLine("  (no session)");

            sb.AppendLine("\n=== ITEMS ===");
            if (ctx.Items != null && ctx.Items.Count > 0)
            {
                foreach (var kv in ctx.Items)
                    sb.AppendLine($"  {kv.Key}: {kv.Value}");
            }
            else sb.AppendLine("  (none)");

            sb.AppendLine("\n=== CONNECTION ===");
            var conn = ctx.Connection;
            sb.AppendLine($"RemoteIp:   {conn?.RemoteIpAddress}");
            sb.AppendLine($"RemotePort: {conn?.RemotePort}");
            sb.AppendLine($"LocalIp:    {conn?.LocalIpAddress}");
            sb.AppendLine($"LocalPort:  {conn?.LocalPort}");
            sb.AppendLine($"ClientCert: {conn?.ClientCertificate?.Subject ?? "(none)"}");

            sb.AppendLine("\n=== FEATURES ===");
            var features = ctx.Features;
            foreach (var f in features)
                sb.AppendLine($"  {f.Key.FullName}: {f.Value?.GetType().FullName}");

            sb.AppendLine("\n=== RESPONSE (current state) ===");
            sb.AppendLine($"StatusCode: {ctx.Response?.StatusCode}");
            sb.AppendLine("Headers:");
            if (ctx.Response?.Headers != null && ctx.Response.Headers.Count > 0)
            {
                foreach (var h in ctx.Response.Headers)
                    sb.AppendLine($"  {h.Key}: {h.Value}");
            }
            else sb.AppendLine("  (none)");

            var dump = sb.ToString();
            _logger.LogInformation("Fetching account news for input: {Input}, context: {context}, plannerKey = {plannerKey} dump={dump}", userPrompt, context, plannerKey, dump);

        }

        return await CallAgent(plannerKey, userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }

    [McpServerTool, Description("Summarize the raw data about list of top competitors of an account in a way that is presentable to a user")]
    public async Task<string> GetAccountCompetitor(McpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        return await CallAgent("Account Competitor", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }

    [McpServerTool, Description("Fetches and displays the list of account team members working on an account")]
    public async Task<string> GetAccountTeam(McpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {   
        return await CallAgent("GetAccountTeamM365", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }
    
    [McpServerTool, Description("This plugin provides the categories and list of prompts on the basis of logged in user. This plugin returns prompts as actions. This plugin can not summarize the response. There is only of input for this plugin where user wants to Explore prompts . This plugin cannot summarize response.")]
    public async Task<object> GetLeadingPrompts(McpServer thisServer, string userPrompt, string context, CancellationToken cancellationToken)
    {
        return await CallAgentObject("LeadingPrompts", userPrompt, thisServer.ServerOptions.ServerInfo.Name);
    }
    

    
}
