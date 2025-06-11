using MCPServer.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton<DataverseApiService>();
builder.Services.AddSingleton<SalesAgentPluginApiService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false // Set to true and configure ValidAudience for production
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Example: Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MCPServer started at {Time}", DateTime.UtcNow);

// Configure the HTTP request pipeline.
//app.MapMcp();
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
*/
app.MapGet("/sse", [Microsoft.AspNetCore.Authorization.Authorize] (HttpContext context) =>
{
    // Your existing /sse endpoint logic here
    // For example, return a simple response for now
    return Results.Ok(new { message = "Authenticated /sse endpoint reached." });
});

app.Run();