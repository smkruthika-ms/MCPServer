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
    
    // Map of tool names to this server
    public List<string> ToolNames { get; set; } = new();

    // MCP endpoint path (default is /sse for JSON-RPC)
    public string McpEndpoint { get; set; } = "/";

    public string GetMcpUrl() => $"{BaseUrl.TrimEnd('/')}{McpEndpoint}";
}
