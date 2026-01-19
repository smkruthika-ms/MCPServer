using MCPServer.Services;
using MCPServer.Extensions;
using MCPServer.DynamicTools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSearchMCPServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();  // Add memory cache for plugin registry caching
builder.Services.AddSingleton<SalesAgentPluginApiService>();
builder.Services.AddHttpClient<ExecutionHostService>();
builder.Services.AddSingleton<ExecutionHostService>();

// Add scoped service for passing token across requests
builder.Services.AddScoped<AuthTokenContext>();

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
            Console.WriteLine("=== REQUEST RECEIVED ===");
            var sanitizedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in httpContext.Request.Headers)
            {
                    sanitizedHeaders[header.Key] = header.Value.ToString();
            }

            //sanitizedHeaders["body"] =  httpContext.Request.Body.ToString();
            
            
            // Extract token from Authorization header
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            string? extractedToken = null;
            if (!string.IsNullOrEmpty(authHeader))
            {
                extractedToken = authHeader.Replace("Bearer ", "");
                Console.WriteLine($"[REQUEST] Authorization header found, extracting token...");

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(extractedToken);

                var appId = jwtToken.Payload["appid"]?.ToString() ?? "";
                var aud = jwtToken.Audiences != null ? string.Join(", ", jwtToken.Audiences) : "";
                var iss = jwtToken.Issuer ?? "";

                serverOptions.ServerInfo = new Implementation
                {
                    Name = JsonSerializer.Serialize(extractedToken),
                    Version = "1.0.0"
                };
            }
            else
            {
                Console.WriteLine($"[REQUEST] NO Authorization header found");
                serverOptions.ServerInfo = new Implementation
                {
                    Name = JsonSerializer.Serialize(sanitizedHeaders),
                    Version = "1.0.0"
                };
            }
            
            // Store token in scoped AuthTokenContext for use in plugin loading
            try
            {
                var tokenContext = httpContext.RequestServices.GetRequiredService<AuthTokenContext>();
                tokenContext.Token = extractedToken;
                Console.WriteLine($"[REQUEST] Token stored in AuthTokenContext: {(!string.IsNullOrEmpty(extractedToken) ? "YES ✓" : "NO ✗")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REQUEST] ERROR storing token: {ex.Message}");
            }


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
    })
    .WithTools<ExtractContextTool>()
    .WithTools<DynamicMcpToolProvider>()
    .WithDynamicPlugins(builder.Services);  // Register dynamic plugin services


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

// Middleware to extract and store token BEFORE DynamicMcpToolProvider is resolved
app.Use(async (context, next) =>
{
    Console.WriteLine("[TOKEN MIDDLEWARE] Extracting token from Authorization header...");
    var authHeader = context.Request.Headers["Authorization"].ToString();
    string? extractedToken = null;
    
    if (!string.IsNullOrEmpty(authHeader))
    {
        extractedToken = authHeader.Replace("Bearer ", "");
        Console.WriteLine($"[TOKEN MIDDLEWARE] Token extracted, storing in AuthTokenContext...");
        
        try
        {
            var tokenContext = context.RequestServices.GetRequiredService<AuthTokenContext>();
            tokenContext.Token = extractedToken;
            Console.WriteLine($"[TOKEN MIDDLEWARE] Token stored successfully ✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TOKEN MIDDLEWARE] ERROR storing token: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine($"[TOKEN MIDDLEWARE] NO Authorization header found");
    }
    
    await next();
});

// Middleware to ensure DynamicMcpToolProvider is enumerated per-request (AFTER token is set)
app.Use(async (context, next) =>
{
    Console.WriteLine("[MIDDLEWARE] Request pipeline - Triggering tool enumeration");
    var provider = context.RequestServices.GetRequiredService<DynamicMcpToolProvider>();
    Console.WriteLine("[MIDDLEWARE] DynamicMcpToolProvider resolved - forcing enumeration to load tools");
    
    // Force enumeration to trigger GetEnumerator() and load tools fresh
    try
    {
        var toolCount = provider.Count();  // This triggers GetEnumerator() and loads all tools
        Console.WriteLine($"[MIDDLEWARE] Tools enumeration complete - {toolCount} tools available at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
        
        // Store the tool count in the HttpContext items so we can access it later
        context.Items["DynamicToolCount"] = toolCount;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[MIDDLEWARE] ERROR during tool enumeration: {ex.Message}");
    }
    
    await next();
    
    // Log after response to see if MCP framework used our tools
    if (context.Items.TryGetValue("DynamicToolCount", out var count))
    {
        Console.WriteLine($"[MIDDLEWARE] Response complete - Had enumerated {count} dynamic tools");
    }
});

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