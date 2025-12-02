namespace MCPServer.Services;

public class WidgetResourceService
{
    private readonly ILogger<WidgetResourceService> _logger;
    private readonly IWebHostEnvironment _environment;
    private string? _widgetHtml;
    private string? _widgetJs;
    private string? _widgetCss;

    public WidgetResourceService(ILogger<WidgetResourceService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async Task LoadWidgetAssetsAsync()
    {
        try
        {
            var widgetPath = Path.Combine(_environment.ContentRootPath, "..", "Widget", "dist");
            
            var jsPath = Path.Combine(widgetPath, "widget.js");
            var cssPath = Path.Combine(widgetPath, "widget.css");

            if (File.Exists(jsPath))
            {
                _widgetJs = await File.ReadAllTextAsync(jsPath);
                _logger.LogInformation("Loaded widget JavaScript from {Path}", jsPath);
            }
            else
            {
                _logger.LogWarning("Widget JavaScript not found at {Path}. Please run 'npm run build' in the Widget directory.", jsPath);
                _widgetJs = "console.error('Widget bundle not found. Please build the widget.');";
            }

            if (File.Exists(cssPath))
            {
                _widgetCss = await File.ReadAllTextAsync(cssPath);
                _logger.LogInformation("Loaded widget CSS from {Path}", cssPath);
            }
            else
            {
                _logger.LogWarning("Widget CSS not found at {Path}", cssPath);
                _widgetCss = "/* Widget styles not found */";
            }

            // Generate the complete HTML template
            _widgetHtml = GenerateWidgetHtml();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading widget assets");
            _widgetHtml = GenerateFallbackHtml();
        }
    }

    private string GenerateWidgetHtml()
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Sales Dashboard Widget</title>
    <style>{_widgetCss}</style>
</head>
<body>
    <div id=""widget-root""></div>
    <script type=""module"">{_widgetJs}</script>
</body>
</html>".Trim();
    }

    private string GenerateFallbackHtml()
    {
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Sales Dashboard Widget</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            padding: 20px;
            background: #fff;
            color: #1a1a1a;
        }
        .widget-container {
            max-width: 600px;
            margin: 0 auto;
        }
        h2 { margin-bottom: 16px; }
        pre {
            background: #f5f5f5;
            padding: 12px;
            border-radius: 6px;
            overflow-x: auto;
        }
    </style>
</head>
<body>
    <div class=""widget-container"">
        <h2>Sales Data</h2>
        <div id=""content""></div>
    </div>
    <script>
        const data = window.openai?.toolOutput || { message: 'No data available' };
        document.getElementById('content').innerHTML = '<pre>' + JSON.stringify(data, null, 2) + '</pre>';
    </script>
</body>
</html>".Trim();
    }

    public string GetWidgetHtml()
    {
        return _widgetHtml ?? GenerateFallbackHtml();
    }
}
