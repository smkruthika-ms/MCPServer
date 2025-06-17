using MCPServer.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

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
            ValidAudience = "api://5d437e13-722e-4a8d-8f4f-3158cf570e94",
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Example: Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MCPServer started at {Time}", DateTime.UtcNow);
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        logger.LogInformation("Authorization header received: {AuthHeader}", authHeader);
    }
    else
    {
        logger.LogWarning("No Authorization header received.");
    }
    await next();
});
// Configure the HTTP request pipeline.
app.MapMcp();
/*
app.MapGet("/stream-products", async (HttpContext context, DataverseApiService dataverseApiService, string productName, string region) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("StreamProducts");
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        logger.LogInformation("Authorization header received: {AuthHeader}", authHeader);
    }
    else
    {
        logger.LogWarning("No Authorization header received.");
    }
    logger.LogInformation("/stream-products called with productName={ProductName}, region={Region}", productName, region);
    context.Response.Headers["Content-Type"] = "application/x-ndjson";
    var product = await dataverseApiService.StreamProductsAsync(productName, region);
    var json = JsonSerializer.Serialize(product);
    await context.Response.WriteAsync(json + "\n");
    await context.Response.Body.FlushAsync();
    logger.LogInformation("/stream-products completed for productName={ProductName}, region={Region}", productName, region);
});

app.MapGet("/sse", (HttpContext context) =>
{
    //var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("StreamProducts");
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        logger.LogInformation("Authorization header received: {AuthHeader}", authHeader);
    }
    else
    {
        logger.LogWarning("No Authorization header received.");
    }
    // Your existing /sse endpoint logic here
    // For example, return a simple response for now
    return Results.Ok(new { message = "Authenticated /sse endpoint reached." });
});
*/
app.Run();