namespace MCPServer.Models;

/// <summary>
/// Response model for MCP tools that includes widget metadata
/// </summary>
public class WidgetToolResponse
{
    /// <summary>
    /// Concise structured content that both the model and widget can read
    /// </summary>
    public object? StructuredContent { get; set; }

    /// <summary>
    /// Optional text content for the model's narration
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Metadata exclusively for the widget (never sent to the model)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Widget configuration
    /// </summary>
    public WidgetConfig? WidgetConfig { get; set; }

    public WidgetToolResponse()
    {
        Metadata = new Dictionary<string, object>();
        WidgetConfig = new WidgetConfig();
    }
}

/// <summary>
/// Configuration for widget display and behavior
/// </summary>
public class WidgetConfig
{
    /// <summary>
    /// URI of the widget template to use
    /// </summary>
    public string OutputTemplate { get; set; } = "ui://widget/sales-dashboard.html";

    /// <summary>
    /// Whether the widget can call this tool
    /// </summary>
    public bool WidgetAccessible { get; set; } = true;

    /// <summary>
    /// Visibility: "public" (model and widget) or "private" (widget only)
    /// </summary>
    public string Visibility { get; set; } = "public";

    /// <summary>
    /// Message shown while tool is executing
    /// </summary>
    public string? InvokingMessage { get; set; }

    /// <summary>
    /// Message shown after tool completes
    /// </summary>
    public string? InvokedMessage { get; set; }

    /// <summary>
    /// Whether the widget prefers a border
    /// </summary>
    public bool PrefersBorder { get; set; } = true;
}
