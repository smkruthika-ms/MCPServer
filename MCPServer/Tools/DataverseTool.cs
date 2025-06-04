using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using MCPServer.Services;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
public sealed class WebSearchTool
{
    private readonly DataverseApiService _dataverseApiService;

    public WebSearchTool(DataverseApiService dataverseApiService)
    {
        this._dataverseApiService = dataverseApiService;
    }

    [McpServerTool, Description("Executes a search for product on Dataverse")]
    public async Task<JsonElement> Search(string productName, string region = "All")
    {
        try
        {
            var a = await _dataverseApiService.StreamProductsAsync(productName, region);
            return JsonSerializer.SerializeToElement(a);
        }
        catch (Exception ex)
        {
            var error = new { error = ex.Message, stack = ex.StackTrace };
            return JsonSerializer.SerializeToElement(error);
        }
    }
}