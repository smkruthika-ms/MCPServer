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
    public async Task<string> Search(string accountName)
    {
        String output = await _dataverseApiService.GetAccountAsync(accountName);
        return output;
    }
}