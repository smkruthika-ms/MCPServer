using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MCPServer.Models;
using ModelContextProtocol.Server;

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
            _logger.LogInformation("Token check - Token is provided: {TokenProvided}", !string.IsNullOrEmpty(token));
            _logger.LogInformation("Full Token value: {TokenFull}", token ?? "No token");
            
            if (!string.IsNullOrEmpty(token))
            {
                // Remove "Bearer " prefix (case-insensitive) and trim all whitespace
                var cleanToken = token;
                if (cleanToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    cleanToken = cleanToken.Substring(7); // Remove "Bearer " (7 characters)
                }
                cleanToken = cleanToken.Trim();
                
                // Remove first and last character (quotes or other wrappers)
                if (cleanToken.Length > 2)
                {
                    cleanToken = cleanToken.Substring(1, cleanToken.Length - 2);
                }
                
                _logger.LogInformation("Clean Token (without Bearer prefix and wrapper characters): {CleanToken}", cleanToken);
                
                if (cleanToken.Length > 0)
                {
                    _logger.LogInformation("Setting Authorization header with Bearer token");
                    
                    // Create Authorization header with proper format: "Bearer <token>"
                    // The AuthenticationHeaderValue constructor takes scheme ("Bearer") and the token
                    // It automatically adds a space between scheme and token
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                    
                    _logger.LogInformation("Authorization header set: Bearer {TokenStart}...{TokenEnd}", 
                        cleanToken.Substring(0, Math.Min(30, cleanToken.Length)),
                        cleanToken.Length > 30 ? cleanToken.Substring(cleanToken.Length - 20) : "");
                    _logger.LogInformation("Full Authorization Header: {AuthHeader}", 
                        _httpClient.DefaultRequestHeaders.Authorization?.ToString() ?? "Not set");
                }
                else
                {
                    _logger.LogWarning("Token provided but empty after cleaning");
                }
            }
            else
            {
                _logger.LogWarning("No token provided - request will be sent without Authorization header");
            }

            _logger.LogInformation("Calling ExecutionHost API: {Url} with PlannerKey: {PlannerKey}", url, plannerKey);

            // Make the request
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

            _logger.LogInformation("ExecutionHost API response status: {StatusCode}", response.StatusCode);

            // Read the response content
            var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            // Log response body if available
            if (response.Content.Headers.ContentLength > 0)
            {
                responseContent.Seek(0, System.IO.SeekOrigin.Begin);
                using (var reader = new System.IO.StreamReader(responseContent, leaveOpen: true))
                {
                    var responseBodyText = await reader.ReadToEndAsync(cancellationToken);
                    _logger.LogInformation("ExecutionHost API response body: {ResponseBody}", responseBodyText);
                    responseContent.Seek(0, System.IO.SeekOrigin.Begin);
                }
            }

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

    /// <summary>
    /// Invokes a dynamic plugin via ExecutionHost.
    /// Extracts the token from McpServer and calls the plugin's planner key.
    /// </summary>
    public async Task<HttpResponseData> InvokeDynamicPluginAsync(
        McpServer thisServer,
        string plannerKey,
        string userPrompt,
        string context,
        CancellationToken cancellationToken = default)
    {
        // Extract token from server (same pattern as ExecutionHostTools)
        var token = thisServer?.ServerOptions?.ServerInfo?.Name ?? string.Empty;
        
        // Call the plugin via ExecutionHost
        return await CallExecutionHostAsync(plannerKey, userPrompt, token, cancellationToken);
    }
}
