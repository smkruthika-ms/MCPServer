using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using MCPServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public sealed class WebSearchTool
{
    private readonly DataverseApiService _dataverseApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<WebSearchTool> _logger;

    public WebSearchTool(DataverseApiService dataverseApiService, IHttpContextAccessor httpContextAccessor, ILogger<WebSearchTool> logger)
    {
        this._dataverseApiService = dataverseApiService;
        this._httpContextAccessor = httpContextAccessor;
        this._logger = logger;
    }

    [McpServerTool, Description("Executes a search for product on Dataverse")]
    public async Task<JsonElement> Search(string productName, string region = "All")
    {
        try
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                _logger.LogInformation("Authorization header received in tool: {AuthHeader}", authHeader);
            }
            else
            {
                _logger.LogInformation("No Authorization header received in tool.");
            }
            var a = await _dataverseApiService.StreamProductsAsync(productName, region);
            return JsonSerializer.SerializeToElement(a);
        }
        catch (Exception ex)
        {
            var error = new { error = ex.Message, stack = ex.StackTrace };
            _logger.LogError(ex, "Error in Search: {Message}", ex.Message);
            return JsonSerializer.SerializeToElement(error);
        }
    }

    [McpServerTool, Description("Summarize information about profile/highlights of an account")]
    public async Task<string> SummarizeAccountProfile(string accountName)
    {
        try
        {
            var summary = await _dataverseApiService.GetAccountAsync(accountName);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SummarizeAccountProfile: {Message}", ex.Message);
            return $"Error: {ex.Message}";
        }
    }
}