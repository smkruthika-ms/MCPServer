using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MCPServer.Models;

namespace MCPServer.Services;

public class ExecutionHostService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExecutionHostService> _logger;
    private readonly HttpClient _httpClient;

    public ExecutionHostService(IConfiguration configuration, ILogger<ExecutionHostService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Calls the ExecutionHost API with the given planner key and message.
    /// Routes to either UAT or PROD based on environment configuration.
    /// </summary>
    public async Task<HttpResponseData> CallExecutionHostAsync(
        string plannerKey,
        string userPrompt,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determine if we're in development (UAT) or production
            bool isProduction = !_configuration.GetValue<bool>("EnableDevelopment", true);

            // Get the appropriate endpoint from configuration
            string endpoint = isProduction
                ? _configuration["ExecutionHost:Prod:Endpoint"] ?? "https://msxsalescopilot-fd-prod-a8gvc9gybfa0b9hn.z01.azurefd.net/cppsk/api/executionHost"
                : _configuration["ExecutionHost:UAT:Endpoint"] ?? "https://msxuat-fd-gxbmeya2c3gwbyhq.b02.azurefd.net/api/executionHost";

            // Build the request URL with planner key
            string url = $"{endpoint}/{plannerKey}?api-version=1";

            // Create the request body
            var requestBody = new ExecutionHostInput
            {
                message = userPrompt,
                inputs = new ExecutionHostInput.InputsData
                {
                    text = userPrompt,
                    text_3 = userPrompt
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Add authorization header if token is provided
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            _logger.LogInformation("Calling ExecutionHost API: {Url} with PlannerKey: {PlannerKey}", url, plannerKey);

            // Make the request
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

            _logger.LogInformation("ExecutionHost API response status: {StatusCode}", response.StatusCode);

            // Read the response content
            var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);

            // Create HttpResponseData to match Azure Functions format
            var responseData = new HttpResponseData((HttpStatusCode)response.StatusCode)
            {
                Body = responseContent
            };

            // Copy response headers
            foreach (var header in response.Content.Headers)
            {
                responseData.Headers[header.Key] = string.Join(", ", header.Value);
            }

            return responseData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ExecutionHost API with PlannerKey: {PlannerKey}", plannerKey);

            var errorResponse = new HttpResponseData(HttpStatusCode.InternalServerError);
            errorResponse.SetBodyAsString($"{{\"error\": \"{ex.Message}\"}}");
            return errorResponse;
        }
    }
}
