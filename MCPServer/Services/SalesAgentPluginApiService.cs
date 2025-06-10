using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCPServer.Services;

public class SalesAgentPluginApiService
{
    public async Task<string> ChatWithAgentAsync(
        string plannerKey,
        string message)
    {
        var requestBody = new
        {
            InvokeAllFunctions = true,
            InvokeAllSkills = false,
            filterURLs= false,
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
            Source = "MCPServer"
        };    
        var jsonBody = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://salescopilotskeusuat.azurewebsites.net/api/Playground");
        request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        var token = Environment.GetEnvironmentVariable("SALES_AGENT_PLUGIN_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Sales Agent Plugin token is not set in environment variables.");
        }
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}
