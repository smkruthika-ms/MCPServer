using System.Text.Json.Serialization;

namespace MCPServer.Models;

/// <summary>
/// Represents a Dataverse plugin that should be exposed as an MCP tool.
/// </summary>
public class PluginInfo
{
    /// <summary>Unique identifier for the plugin</summary>
    [JsonPropertyName("msp_pluginid")]
    public string? PluginId { get; set; }

    /// <summary>Name of the plugin (used internally)</summary>
    [JsonPropertyName("msp_pluginname")]
    public string? PluginName { get; set; }

    /// <summary>Friendly name for display</summary>
    [JsonPropertyName("msp_friendlyname")]
    public string? FriendlyName { get; set; }

    /// <summary>Description of what the plugin does</summary>
    [JsonPropertyName("msp_description")]
    public string? Description { get; set; }

    /// <summary>Input parameter description</summary>
    [JsonPropertyName("msp_inputparameter")]
    public string? InputParameter { get; set; }

    /// <summary>Output parameter description</summary>
    [JsonPropertyName("msp_outputparameter")]
    public string? OutputParameter { get; set; }

    /// <summary>Function name exposed by the plugin</summary>
    [JsonPropertyName("msp_functionname")]
    public string? FunctionName { get; set; }

    /// <summary>Planner key for plugin execution</summary>
    [JsonPropertyName("msp_plannerkey")]
    public string? PlannerKey { get; set; }

    /// <summary>Base HTTP endpoint for the plugin</summary>
    [JsonPropertyName("msp_basehttpendpoint")]
    public string? BaseHttpEndpoint { get; set; }

    /// <summary>Method endpoint path (appended to BaseHttpEndpoint)</summary>
    [JsonPropertyName("msp_methodendpoint")]
    public string? MethodEndpoint { get; set; }

    /// <summary>HTTP method type (POST, GET, etc.)</summary>
    [JsonPropertyName("msp_httpmethodtype")]
    public int? HttpMethodType { get; set; }

    /// <summary>Whether the plugin is currently active</summary>
    [JsonPropertyName("statecode")]
    public int? StateCode { get; set; }

    /// <summary>Status code (1 = active)</summary>
    [JsonPropertyName("statuscode")]
    public int? StatusCode { get; set; }

    /// <summary>JSON with detailed plugin metadata</summary>
    [JsonPropertyName("msp_pluginjson")]
    public string? PluginJson { get; set; }

    /// <summary>Skill name (grouping)</summary>
    [JsonPropertyName("msp_skillname")]
    public string? SkillName { get; set; }

    /// <summary>Indicates if plugin is actionable</summary>
    [JsonPropertyName("msp_ispluginactionable")]
    public int? IsPluginActionable { get; set; }

    /// <summary>Check if plugin is active and actionable</summary>
    public bool IsActive => StateCode == 0 && StatusCode == 1;

    /// <summary>
    /// Constructs the full HTTP endpoint URL for invoking the plugin
    /// </summary>
    public string GetPluginEndpoint()
    {
        var baseEndpoint = BaseHttpEndpoint?.TrimEnd('/') ?? "https://salescopilotpluginsnonprod.microsoft.com/uat/v2/skillstudio";
        var methodEndpoint = MethodEndpoint ?? "plugin";
        return $"{baseEndpoint}/{methodEndpoint}";
    }
}

/// <summary>
/// Response wrapper for Dataverse OData query
/// </summary>
public class PluginListResponse
{
    [JsonPropertyName("value")]
    public List<PluginInfo> Value { get; set; } = [];

    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; set; }
}

/// <summary>
/// Request body for plugin invocation
/// </summary>
public class PluginInvocationRequest
{
    [JsonPropertyName("input")]
    public Dictionary<string, object?>? Input { get; set; }

    [JsonPropertyName("FunctionName")]
    public string? FunctionName { get; set; }
}

/// <summary>
/// Response from plugin invocation
/// </summary>
public class PluginInvocationResponse
{
    [JsonPropertyName("output")]
    public object? Output { get; set; }

    [JsonPropertyName("StatusCode")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }
}
