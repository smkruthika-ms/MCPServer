using MCPServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSearchMCPServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Set session timeout to 30 minutes
        options.IdleTimeout = TimeSpan.FromMinutes(30);

        // Limit idle sessions to 10,000
        options.MaxIdleSessionCount = 10000;


        /* NOTE: Uncomment this to send token to downstream tools
         * This logs the token to application insight, consider hashing it 
        options.ConfigureSessionOptions =  (httpContext, serverOptions, cancellationToken) =>
        {
            var sanitizedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in httpContext.Request.Headers)
            {
                if (!header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    sanitizedHeaders[header.Key] = header.Value.ToString();
                }
            }

            sanitizedHeaders["body"] =  httpContext.Request.Body.ToString();
            
            
            // Extract token from Authorization header
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.Replace("Bearer ", "");

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var appId = jwtToken.Payload["appid"]?.ToString() ?? "";
                var aud = jwtToken.Audiences != null ? string.Join(", ", jwtToken.Audiences) : "";
                var iss = jwtToken.Issuer ?? "";

                serverOptions.ServerInfo = new Implementation
                {
                    // TODO: This logs the token to app insights, consider hashing 
                    Name = token,
                    Version = "1.0.0"
                };
            }
            else
            {
                serverOptions.ServerInfo = new Implementation
                {
                    Name = "tokenless",
                    Version = "1.0.0"
                };
            }


            return Task.CompletedTask;
        };
        */

        // Custom session handling
        options.RunSessionHandler = (httpContext, mcpServer, cancellationToken) =>
        {
            // Perform custom logic before running the session
            Console.WriteLine($"Starting session for user: {httpContext.User.Identity?.Name}");

            // Run the session
            return mcpServer.RunAsync(cancellationToken);
        };
    })
    .WithTools<SalesChatPluginTool>()
    .WithTools<ExtractContextTool>();

builder.Services.AddSingleton<DataverseApiService>();
builder.Services.AddSingleton<SalesAgentPluginApiService>();


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
            "afe3816a-f889-4e0f-8760-d131fa9116cf"
        },
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" MCPServer started at {Time}", DateTime.UtcNow);

app.MapMcp().RequireAuthorization();

logger.LogInformation("[App Insights Test] Application Insights logging test at {Time}", DateTime.UtcNow);

app.Run();