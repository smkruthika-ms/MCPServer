using ModelContextProtocol.Protocol;

namespace MCPServer.Gateway;

/// <summary>
/// Represents a downstream MCP server configuration
/// </summary>
public class DownstreamMcpServer
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, string> Headers { get; set; } = new();
    
    // Map of tool names to this server with optional parameter mappings
    public List<ToolConfig> Tools { get; set; } = new();
    
    // Legacy: Simple tool name list (for backward compatibility)
    public List<string> ToolNames { get; set; } = new();

    // MCP endpoint path (default is /sse for JSON-RPC)
    public string McpEndpoint { get; set; } = "/";

    public string GetMcpUrl() => $"{BaseUrl.TrimEnd('/')}{McpEndpoint}";
}

/// <summary>
/// Tool configuration with optional parameter mapping
/// </summary>
public class ToolConfig
{
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional: Maps gateway parameter names to downstream tool parameter names
    /// Example: { "query": "user_prompt", "conversationId": "conversation_id" }
    /// </summary>
    public Dictionary<string, string>? ParameterMapping { get; set; }
    
    /// <summary>
    /// Optional: Default values to always include in downstream call
    /// Example: { "request_source": "MCP_Gateway" }
    /// </summary>
    public Dictionary<string, object>? DefaultParameters { get; set; }
}

