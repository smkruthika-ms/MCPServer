using ModelContextProtocol.Server;
using System.ComponentModel;
using MCPServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public sealed class AccountNewsTool
{
    private readonly DataverseApiService _dataverseApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccountNewsTool> _logger;

    public AccountNewsTool(DataverseApiService dataverseApiService, IHttpContextAccessor httpContextAccessor, ILogger<AccountNewsTool> logger)
    {
        _dataverseApiService = dataverseApiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    [McpServerTool, Description("Account News Highlights for the Account")]
    public async Task<string> GetAccountNews(string accountName)
    {
        try
        {
            var news = await _dataverseApiService.GetAccountNewsAsync(accountName);
            return news;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAccountNews: {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
    }
}
