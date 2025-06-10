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
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                // Log method, path, headers, query, and body (if needed)
                _logger.LogInformation("Request Method: {Method}", request.Method);
                _logger.LogInformation("Request Path: {Path}", request.Path);
                foreach (var header in request.Headers)
                {
                    _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
                }
                foreach (var query in request.Query)
                {
                    _logger.LogInformation("Query: {Key} = {Value}", query.Key, query.Value);
                }
                // Optionally log body (for POST/PUT) - not shown here for GET

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

    
}