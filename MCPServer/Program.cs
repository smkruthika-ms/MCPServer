using MCPServer.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton<DataverseApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapMcp();

app.MapGet("/stream-products", async (HttpContext context, DataverseApiService dataverseApiService, string productName, string region) =>
{
    context.Response.Headers.Add("Content-Type", "application/x-ndjson");
    var product = await dataverseApiService.StreamProductsAsync(productName, region);
    var json = JsonSerializer.Serialize(product);
    await context.Response.WriteAsync(json + "\n");
    await context.Response.Body.FlushAsync();
    
});

app.Run();