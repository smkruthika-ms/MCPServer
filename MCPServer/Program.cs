using MCPServer.Services;
using MCPServer.Extensions;
using MCPServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSearchMCPServer.Tools;

var builder = WebApplication.CreateBuilder(args);

// Configure PluginFilter options from appsettings.json
builder.Services.Configure<PluginFilterOptions>(
    builder.Configuration.GetSection(PluginFilterOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();  // Add memory cache for plugin registry caching
builder.Services.AddSingleton<SalesAgentPluginApiService>();
builder.Services.AddHttpClient<ExecutionHostService>();
builder.Services.AddSingleton<ExecutionHostService>();

// Register Dataverse API service and plugin registry as Singletons for cross-scope token access
builder.Services.AddSingleton<DataverseApiService>();
builder.Services.AddHttpClient<DataversePluginRegistryService>();
builder.Services.AddSingleton<IPluginRegistryService, DataversePluginRegistryService>();

// Add services to the container.
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Set session timeout to 30 minutes
        options.IdleTimeout = TimeSpan.FromMinutes(30);

        // Limit idle sessions to 10,000
        options.MaxIdleSessionCount = 10000;
        

        // Configure per-session options
        options.ConfigureSessionOptions =  (httpContext, serverOptions, cancellationToken) =>
        {
            // Extract token from Authorization header SECURELY
            // We capture it in a closure for the handlers - NOT passed via serverOptions to avoid logging
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            string? sessionToken = null;
            
            if (!string.IsNullOrEmpty(authHeader))
            {
                sessionToken = authHeader.Replace("Bearer ", "");
                Console.WriteLine($"[REQUEST] Token extracted from Authorization header (length: {sessionToken?.Length ?? 0})");
                Console.WriteLine($"[REQUEST] Full Token received from MCP client: {sessionToken}");
            }
            else
            {
                Console.WriteLine("[REQUEST] No Authorization header found");
            }
            
            // Set server info WITHOUT the token (safe for logging)
            serverOptions.ServerInfo = new Implementation
            {
                Name = "MCPServer",
                Version = "1.0.0"
            };
            
            // Configure dynamic tool handlers for per-session tool loading
            // The sessionToken is captured in the closure - NOT stored in serverOptions
            var pluginRegistryForHandlers = httpContext.RequestServices.GetRequiredService<IPluginRegistryService>();
            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var handlerLogger = loggerFactory.CreateLogger("DynamicToolHandlers");
            
            serverOptions.Handlers = new McpServerHandlers
            {
                // Handle tools/list requests dynamically with authentication token
                ListToolsHandler = async (request, ct) =>
                {
                    try
                    {
                        // Use the captured sessionToken from the closure (not from serverOptions)
                        Console.WriteLine($"[HANDLER] ListToolsHandler called - Token: {(!string.IsNullOrEmpty(sessionToken) ? "YES" : "NO")}");
                        
                        var tools = new List<Tool>();
                        
                        // Add static tool: ExtractContextForAccount
                        tools.Add(new Tool
                        {
                            Name = "ExtractContextForAccount",
                            Description = "Extract context about an account",
                            InputSchema = JsonSerializer.Deserialize<JsonElement>(@"{
                                ""type"": ""object"",
                                ""properties"": {
                                    ""input"": { ""type"": ""string"", ""description"": ""The input to extract context for"" }
                                },
                                ""required"": [""input""]
                            }")
                        });
                        
                        // Add dynamic tools from plugins - use sessionToken from closure (secure, not logged)
                        var plugins = await pluginRegistryForHandlers.GetAllPluginsAsync(sessionToken, ct);
                        
                        foreach (var plugin in plugins)
                        {
                            if (string.IsNullOrWhiteSpace(plugin.PlannerKey))
                                continue;
                                
                            // Use PlannerKey (FriendlyName with spaces replaced)
                            var toolName = plugin.PlannerKey;
                            var toolDescription = plugin.Description ?? plugin.FriendlyName ?? "No description available";
                            
                            // Build JSON Schema for input parameters - inputs is a string that will be
                            // deserialized into { inputs: { text: "...", text_3: "..." } } when calling the API
                            var schemaJson = JsonSerializer.Serialize(new
                            {
                                type = "object",
                                properties = new Dictionary<string, object>
                                {
                                    ["inputs"] = new 
                                    { 
                                        type = "string",
                                        description = "The user query - a natural language input that specifies the desired intent."
                                    }
                                },
                                required = new[] { "inputs" }
                            });
                            var inputSchema = JsonSerializer.Deserialize<JsonElement>(schemaJson);
                            
                            tools.Add(new Tool
                            {
                                Name = toolName,
                                Description = toolDescription,
                                InputSchema = inputSchema
                            });
                        }
                        
                        Console.WriteLine($"[HANDLER] ListToolsHandler returning {tools.Count} tools");
                        return new ListToolsResult { Tools = tools };
                    }
                    catch (Exception ex)
                    {
                        handlerLogger.LogError(ex, "Error in ListToolsHandler");
                        Console.WriteLine($"[HANDLER] ListToolsHandler ERROR: {ex.Message}");
                        return new ListToolsResult { Tools = [] };
                    }
                },
                
                // Handle tools/call requests by invoking the actual plugin
                CallToolHandler = async (request, ct) =>
                {
                    try
                    {
                        var toolName = request.Params?.Name ?? throw new ArgumentException("Tool name is required");
                        Console.WriteLine($"[HANDLER] CallToolHandler called for tool: {toolName}");
                        
                        // Handle static tool: ExtractContextForAccount
                        if (toolName == "ExtractContextForAccount")
                        {
                            var input = "";
                            if (request.Params?.Arguments != null && request.Params.Arguments.TryGetValue("input", out var inputElement))
                            {
                                input = inputElement.GetString() ?? "";
                            }
                            var tpid = Environment.GetEnvironmentVariable("TPID") ?? "784852";
                            Console.WriteLine($"[HANDLER] ExtractContextForAccount returning TPID: {tpid}");
                            return new CallToolResult
                            {
                                Content = new List<ContentBlock>
                                {
                                    new TextContentBlock { Text = tpid }
                                }
                            };
                        }
                        
                        // Extract inputs as a string
                        // The string will be used for both text and text_3 when calling the API
                        string inputs = "";
                        if (request.Params?.Arguments != null && 
                            request.Params.Arguments.TryGetValue("inputs", out var inputsElement))
                        {
                            // inputs is now a string
                            inputs = inputsElement.GetString() ?? "";
                        }
                        
                        Console.WriteLine($"[HANDLER] CallToolHandler extracted inputs length: {inputs.Length}");
                        
                        // Invoke the plugin - pass sessionToken securely via parameter
                        Console.WriteLine($"[HANDLER] CallToolHandler invoking with token length: {sessionToken?.Length ?? 0}");
                        var result = await pluginRegistryForHandlers.InvokePluginAsync(toolName, inputs, sessionToken, ct);
                        var resultText = result?.ToString() ?? "null";
                        
                        Console.WriteLine($"[HANDLER] CallToolHandler completed for tool: {toolName}");
                        
                        return new CallToolResult
                        {
                            Content = new List<ContentBlock>
                            {
                                new TextContentBlock { Text = resultText }
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        handlerLogger.LogError(ex, "Error in CallToolHandler");
                        Console.WriteLine($"[HANDLER] CallToolHandler ERROR: {ex.Message}");
                        
                        return new CallToolResult
                        {
                            IsError = true,
                            Content = new List<ContentBlock>
                            {
                                new TextContentBlock { Text = $"Error: {ex.Message}" }
                            }
                        };
                    }
                }
            };

            return Task.CompletedTask;
        };

        // Custom session handling
        options.RunSessionHandler = (httpContext, mcpServer, cancellationToken) =>
        {
            // Perform custom logic before running the session
            Console.WriteLine($"Starting session for user: {httpContext.User.Identity?.Name}");

            // Run the session
            return mcpServer.RunAsync(cancellationToken);
        };
    });
    // IMPORTANT: Do NOT use .WithTools<>() here - we handle ALL tools via McpServerHandlers
    // The SDK's built-in tools handler would override our custom ListToolsHandler
    // registered in ConfigureSessionOptions above - this enables per-session tool loading with auth token


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   
    .AddJwtBearer(options =>
    {
     options.Authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
                    ValidIssuer = $"https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/",

        ValidateAudience = true,
        ValidAudiences = new[]
        {
            "https://microsoft.onmicrosoft.com/copilotnonprod",
            "00000003-0000-0000-c000-000000000000",
            "bb893c22-978d-4cd4-a6f7-bb6cc0d6e6ce",
            "api://5d437e13-722e-4a8d-8f4f-3158cf570e21f8",
            "https://apihub.azure.com",
            "96ff4394-9197-43aa-b393-6a41652e21f8",
            "https://mcpservernet.azurewebsites.net",
            "54a6fe0f-d031-4f90-9c54-d607a980122d",
            "api://b1ba5194-92ca-46b8-9d7a-38d6a9053f6b",
            "api://1aaf894a-0fdb-487c-ace5-cc90e423ff5c",
            "b1ba5194-92ca-46b8-9d7a-38d6a9053f6b",
            "f15bee8d-71c5-461c-b87d-ffef42876fad",
            "e9555580-b18d-4b00-9ff8-e0f54159da6d",
            "77795025-5b1d-41a2-a77a-e73beb44a9da",
            "c6894284-ffe9-46fe-b977-f7d275ad4ff9",
            "b84e16bc-c715-4f04-8d01-4de55ce8119d",
            "api://5d437e13-722e-4a8d-8f4f-3158cf570e94",
            "http://msxsalescopilot01.crm.dynamics.com/",
            "api://auth-3961aecd-bb0c-4b61-a86e-05df8631ff35/5d437e13-722e-4a8d-8f4f-3158cf570e94",
            "api://auth-eae1983a-8db2-4228-9ea9-633684a2ee22/972bf644-79c8-4dbc-9921-5040af077272",
            "afe3816a-f889-4e0f-8760-d131fa9116cf",
            "972bf644-79c8-4dbc-9921-5040af077272",
            "api://auth-10003cd8-923e-4b58-b208-1a2bebdb7a4e/972bf644-79c8-4dbc-9921-5040af077272"
        },
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Example: Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" MCPServer started at {Time}", DateTime.UtcNow);

// Middleware to log Authorization header and decode JWT claims for debugging


// Configure the HTTP request pipeline.
//app.MapMcp();
app.MapMcp().RequireAuthorization();
/*
// Example: Streamable HTTP endpoint for /sse
app.MapGet("/sse", [Microsoft.AspNetCore.Authorization.Authorize] async (HttpContext context) =>
{
    context.Response.Headers.Add("Cache-Control", "no-cache");
    context.Response.ContentType = "application/x-ndjson";

    // Example: send 5 events, one per second
    for (int i = 1; i <= 5; i++)
    {
        var eventData = JsonSerializer.Serialize(new { message = $"Event {i}", timestamp = DateTime.UtcNow });
        await context.Response.WriteAsync(eventData + "\n");
        await context.Response.Body.FlushAsync();
        await Task.Delay(1000); // simulate streaming
    }
});
*/
// Example log to verify Application Insights integration
logger.LogInformation("[App Insights Test] Application Insights logging test at {Time}", DateTime.UtcNow);

app.Run();