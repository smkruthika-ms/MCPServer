using MCPServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ModelContextProtocol.HttpServer;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using WebSearchMCPServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services
    .AddMicrosoftMcpServer(builder.Configuration, options =>
    {
        options.ResourceHost = "https://mcpservernet.azurewebsites.net";
    })
    .WithHttpTransport(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.MaxIdleSessionCount = 10000;
        options.ConfigureSessionOptions = async (httpContext, serverOptions, cancellationToken) =>
        {
            var sanitizedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in httpContext.Request.Headers)
            {
                sanitizedHeaders[header.Key] = header.Value.ToString();
            }

            httpContext.Request.EnableBuffering();

            string bodyText;
            using (var reader = new StreamReader(
                       httpContext.Request.Body,
                       encoding: System.Text.Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: false,
                       bufferSize: 1024,
                       leaveOpen: true))
            {
                bodyText = await reader.ReadToEndAsync(cancellationToken);
                httpContext.Request.Body.Position = 0;
            }

            // optional: cap size to avoid huge logs
            const int MAX = 64 * 1024;
            if (bodyText.Length > MAX) bodyText = bodyText.Substring(0, MAX);
            sanitizedHeaders["body"] = bodyText;
            serverOptions.ServerInfo = new Implementation
            {
                Name = JsonSerializer.Serialize(sanitizedHeaders),
                Version = "1.0.0"
            };

            await Task.CompletedTask;
        };

        options.RunSessionHandler = (httpContext, mcpServer, cancellationToken) =>
        {
            Console.WriteLine($"Starting session for user: {httpContext.User.Identity?.Name}");
            return mcpServer.RunAsync(cancellationToken);
        };
    })
    .WithTools<SalesChatPluginTool>();

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
            "api://auth-eae1983a-8db2-4228-9ea9-633684a2ee22/972bf644-79c8-4dbc-9921-5040af077272",
            "afe3816a-f889-4e0f-8760-d131fa9116cf",
            "972bf644-79c8-4dbc-9921-5040af077272",
            "api://auth-10003cd8-923e-4b58-b208-1a2bebdb7a4e/972bf644-79c8-4dbc-9921-5040af077272",
            "api://auth-c18b376d-43e9-4eca-9b81-f29b32812d4a/972bf644-79c8-4dbc-9921-5040af077272"
        },
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//app.UseMicrosoftMcpServer(); // setup global middleware
//app.MapMicrosoftMcpServer(); // map endpoint routes

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" MCPServer started at {Time}", DateTime.UtcNow);

app.MapMcp();
//app.MapMcp().RequireAuthorization();

logger.LogInformation("[App Insights Test] Application Insights logging test at {Time}", DateTime.UtcNow);

app.Run();