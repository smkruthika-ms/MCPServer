using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Logging;

namespace MCPServer.Services;

public interface ISalesAgentPluginApiService
    {
    Task<string?> GetTokenAsync(IReadOnlyCollection<string> scopes);
    Task<string> ChatWithAgentAsync(
    string plannerKey,
    string message);
    }
