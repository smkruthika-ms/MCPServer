namespace MCPServer.Models;

/// <summary>
/// Configuration options for filtering plugins
/// </summary>
public class PluginFilterOptions
{
    public const string SectionName = "PluginFilter";
    
    /// <summary>
    /// List of planner keys to ignore (these plugins won't be converted to MCP tools)
    /// </summary>
    public List<string> IgnoredPlannerKeys { get; set; } = new();
}
