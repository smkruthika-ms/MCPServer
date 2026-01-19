using System.Text.Json.Serialization;

namespace MCPServer.Models;

/// <summary>
/// Represents a Dataverse plugin that should be exposed as an MCP tool.
/// </summary>
public class PluginInfo
{
    /// <summary>OData ETag for versioning</summary>
    [JsonPropertyName("oDataETag")]
    public string? ODataETag { get; set; }

    /// <summary>Unique identifier for the plugin</summary>
    [JsonPropertyName("pluginId")]
    public string? PluginId { get; set; }

    /// <summary>Name of the plugin (used internally)</summary>
    [JsonPropertyName("pluginName")]
    public string? PluginName { get; set; }

    /// <summary>Function name exposed by the plugin</summary>
    [JsonPropertyName("functionName")]
    public string? FunctionName { get; set; }

    /// <summary>Friendly name for display</summary>
    [JsonPropertyName("friendlyName")]
    public string? FriendlyName { get; set; }

    /// <summary>Planner key derived from FriendlyName with spaces replaced by underscores</summary>
    public string? PlannerKey => FriendlyName?.Replace(" ", "_");

    /// <summary>Description of what the plugin does</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Skill name (grouping)</summary>
    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }

    /// <summary>Plugin type</summary>
    [JsonPropertyName("pluginType")]
    public string? PluginType { get; set; }

    /// <summary>Skill type</summary>
    [JsonPropertyName("skillType")]
    public string? SkillType { get; set; }

    /// <summary>Environment</summary>
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    /// <summary>Base HTTP endpoint for the plugin</summary>
    [JsonPropertyName("baseHttpEndpoint")]
    public string? BaseHttpEndpoint { get; set; }

    /// <summary>Method endpoint path (appended to BaseHttpEndpoint)</summary>
    [JsonPropertyName("methodEndpoint")]
    public string? MethodEndpoint { get; set; }

    /// <summary>HTTP method type (POST, GET, etc.)</summary>
    [JsonPropertyName("httpMethodType")]
    public int? HttpMethodType { get; set; }

    /// <summary>Input parameter description</summary>
    [JsonPropertyName("inputParameter")]
    public string? InputParameter { get; set; }

    /// <summary>Output parameter description</summary>
    [JsonPropertyName("outputParameter")]
    public string? OutputParameter { get; set; }

    /// <summary>Authentication scheme</summary>
    [JsonPropertyName("authScheme")]
    public string? AuthScheme { get; set; }

    /// <summary>User authentication scheme</summary>
    [JsonPropertyName("userAuthScheme")]
    public string? UserAuthScheme { get; set; }

    /// <summary>Required user tokens</summary>
    [JsonPropertyName("requiredUserTokens")]
    public string? RequiredUserTokens { get; set; }

    /// <summary>Resource ID for authentication</summary>
    [JsonPropertyName("resourceIdForAuth")]
    public string? ResourceIdForAuth { get; set; }

    /// <summary>Scope for invocation</summary>
    [JsonPropertyName("scopeForInvocation")]
    public string? ScopeForInvocation { get; set; }

    /// <summary>JSON with detailed plugin metadata</summary>
    [JsonPropertyName("pluginJson")]
    public string? PluginJson { get; set; }

    /// <summary>ICM handle</summary>
    [JsonPropertyName("icmHandle")]
    public string? IcmHandle { get; set; }

    /// <summary>User email</summary>
    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    /// <summary>Whether the plugin is currently active (0=active)</summary>
    [JsonPropertyName("stateCode")]
    public int? StateCode { get; set; }

    /// <summary>Status code (1 = active)</summary>
    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    /// <summary>Created on date</summary>
    [JsonPropertyName("createdOn")]
    public DateTime? CreatedOn { get; set; }

    /// <summary>Modified on date</summary>
    [JsonPropertyName("modifiedOn")]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>Version number</summary>
    [JsonPropertyName("versionNumber")]
    public int? VersionNumber { get; set; }

    /// <summary>Created by value</summary>
    [JsonPropertyName("createdByValue")]
    public string? CreatedByValue { get; set; }

    /// <summary>Modified by value</summary>
    [JsonPropertyName("modifiedByValue")]
    public string? ModifiedByValue { get; set; }

    /// <summary>Owner ID value</summary>
    [JsonPropertyName("ownerIdValue")]
    public string? OwnerIdValue { get; set; }

    /// <summary>Owning business unit value</summary>
    [JsonPropertyName("owningBusinessUnitValue")]
    public string? OwningBusinessUnitValue { get; set; }

    /// <summary>Owning user value</summary>
    [JsonPropertyName("owningUserValue")]
    public string? OwningUserValue { get; set; }

    /// <summary>Owning team value</summary>
    [JsonPropertyName("owningTeamValue")]
    public string? OwningTeamValue { get; set; }

    /// <summary>Publish metadata</summary>
    [JsonPropertyName("publishMetadata")]
    public string? PublishMetadata { get; set; }

    /// <summary>Publish metadata response</summary>
    [JsonPropertyName("publishMetadataResponse")]
    public string? PublishMetadataResponse { get; set; }

    /// <summary>Move metadata to next environment</summary>
    [JsonPropertyName("moveMetadataToNextEnv")]
    public string? MoveMetadataToNextEnv { get; set; }

    /// <summary>Deployed to production</summary>
    [JsonPropertyName("prodDeployed")]
    public bool ProdDeployed { get; set; }

    /// <summary>Deployed to UAT</summary>
    [JsonPropertyName("uatDeployed")]
    public bool UatDeployed { get; set; }

    /// <summary>Deployed to INT</summary>
    [JsonPropertyName("intDeployed")]
    public bool IntDeployed { get; set; }

    /// <summary>Deployed to dev environment</summary>
    [JsonPropertyName("isPluginDeployedToDevEnv")]
    public bool IsPluginDeployedToDevEnv { get; set; }

    /// <summary>Indicates if plugin is actionable</summary>
    [JsonPropertyName("isPluginActionable")]
    public int? IsPluginActionable { get; set; }

    /// <summary>Onboarded to sales chat</summary>
    [JsonPropertyName("onboardedToSalesChat")]
    public string? OnboardedToSalesChat { get; set; }

    /// <summary>Use FetchXML</summary>
    [JsonPropertyName("fetchXml")]
    public bool FetchXml { get; set; }

    /// <summary>Encrypt</summary>
    [JsonPropertyName("encrypt")]
    public bool Encrypt { get; set; }

    /// <summary>Dynamic parameter</summary>
    [JsonPropertyName("dynamicParam")]
    public bool DynamicParam { get; set; }

    /// <summary>Navigation URL</summary>
    [JsonPropertyName("navigationUrl")]
    public string? NavigationUrl { get; set; }

    /// <summary>Navigation parameters</summary>
    [JsonPropertyName("navigationParameters")]
    public string? NavigationParameters { get; set; }

    /// <summary>Prompt</summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>Format output prompt</summary>
    [JsonPropertyName("formatOutputPrompt")]
    public string? FormatOutputPrompt { get; set; }

    /// <summary>Expected user prompt</summary>
    [JsonPropertyName("expectedUserPrompt")]
    public string? ExpectedUserPrompt { get; set; }

    /// <summary>Parameter definition</summary>
    [JsonPropertyName("paramDefinition")]
    public string? ParamDefinition { get; set; }

    /// <summary>Column mapping</summary>
    [JsonPropertyName("columnMapping")]
    public string? ColumnMapping { get; set; }

    /// <summary>Output data attributes</summary>
    [JsonPropertyName("outputDataAttributes")]
    public string? OutputDataAttributes { get; set; }

    /// <summary>Temperature for LLM</summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>Top P for LLM</summary>
    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    /// <summary>Max token limit</summary>
    [JsonPropertyName("maxTokenLimit")]
    public int? MaxTokenLimit { get; set; }

    /// <summary>Frequency penalty</summary>
    [JsonPropertyName("frequencyPenalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>Presence penalty</summary>
    [JsonPropertyName("presencePenalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>Stop sequence</summary>
    [JsonPropertyName("stopSequence")]
    public string? StopSequence { get; set; }

    /// <summary>Result count limit</summary>
    [JsonPropertyName("resultCountLimit")]
    public int? ResultCountLimit { get; set; }

    /// <summary>P95 latency</summary>
    [JsonPropertyName("p95")]
    public double? P95 { get; set; }

    /// <summary>Auth scheme commands</summary>
    [JsonPropertyName("authSchemeCommands")]
    public string? AuthSchemeCommands { get; set; }

    /// <summary>Publish to MCS</summary>
    [JsonPropertyName("publishToMcs")]
    public string? PublishToMcs { get; set; }

    /// <summary>Request type</summary>
    [JsonPropertyName("requestType")]
    public string? RequestType { get; set; }

    /// <summary>Deployment stage</summary>
    [JsonPropertyName("deploymentStage")]
    public string? DeploymentStage { get; set; }

    /// <summary>L1 category</summary>
    [JsonPropertyName("l1Category")]
    public string? L1Category { get; set; }

    /// <summary>Additional links</summary>
    [JsonPropertyName("additionalLinks")]
    public string? AdditionalLinks { get; set; }

    /// <summary>Additional message</summary>
    [JsonPropertyName("additionalMessage")]
    public string? AdditionalMessage { get; set; }

    /// <summary>SSO registration ID</summary>
    [JsonPropertyName("ssoRegistrationId")]
    public string? SsoRegistrationId { get; set; }

    /// <summary>Skill ID</summary>
    [JsonPropertyName("skillId")]
    public string? SkillId { get; set; }

    /// <summary>Supported API function type</summary>
    [JsonPropertyName("supportedApiFunctionType")]
    public string? SupportedApiFunctionType { get; set; }

    /// <summary>Plugin version</summary>
    [JsonPropertyName("pluginVersion")]
    public string? PluginVersion { get; set; }

    /// <summary>MSP ID</summary>
    [JsonPropertyName("mspId")]
    public string? MspId { get; set; }

    /// <summary>ADO area path</summary>
    [JsonPropertyName("adoAreaPath")]
    public string? AdoAreaPath { get; set; }

    /// <summary>Pre plugin value</summary>
    [JsonPropertyName("prePluginValue")]
    public string? PrePluginValue { get; set; }

    /// <summary>Pre plugin 2 value</summary>
    [JsonPropertyName("prePlugin2Value")]
    public string? PrePlugin2Value { get; set; }

    /// <summary>Created on behalf by value</summary>
    [JsonPropertyName("createdOnBehalfByValue")]
    public string? CreatedOnBehalfByValue { get; set; }

    /// <summary>Modified on behalf by value</summary>
    [JsonPropertyName("modifiedOnBehalfByValue")]
    public string? ModifiedOnBehalfByValue { get; set; }

    /// <summary>Import sequence number</summary>
    [JsonPropertyName("importSequenceNumber")]
    public int? ImportSequenceNumber { get; set; }

    /// <summary>Overridden created on</summary>
    [JsonPropertyName("overriddenCreatedOn")]
    public DateTime? OverriddenCreatedOn { get; set; }

    /// <summary>Timezone rule version number</summary>
    [JsonPropertyName("timezoneRuleVersionNumber")]
    public int? TimezoneRuleVersionNumber { get; set; }

    /// <summary>UTC conversion timezone code</summary>
    [JsonPropertyName("utcConversionTimezoneCode")]
    public int? UtcConversionTimezoneCode { get; set; }

    /// <summary>Parsed plugin JSON details</summary>
    [JsonPropertyName("pluginJsonDetails")]
    public PluginJsonDetails? PluginJsonDetails { get; set; }

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
/// Response wrapper for Dataverse OData query or external plugin API
/// </summary>
public class PluginListResponse
{
    [JsonPropertyName("value")]
    public List<PluginInfo>? Value { get; set; }

    [JsonPropertyName("plugins")]
    public List<PluginInfo>? Plugins { get; set; }

    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; set; }

    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("errors")]
    public List<object>? Errors { get; set; }

    /// <summary>Gets the actual list of plugins, preferring "plugins" over "value"</summary>
    public List<PluginInfo> GetPlugins() => Plugins ?? Value ?? [];
}

/// <summary>
/// Parsed plugin JSON details
/// </summary>
public class PluginJsonDetails
{
    [JsonPropertyName("functionName")]
    public string? FunctionName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inputParam")]
    public string? InputParam { get; set; }

    [JsonPropertyName("outputParam")]
    public string? OutputParam { get; set; }

    [JsonPropertyName("baseHttpEndPoint")]
    public string? BaseHttpEndPoint { get; set; }

    [JsonPropertyName("methodEndPoint")]
    public string? MethodEndPoint { get; set; }

    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }

    [JsonPropertyName("httpMethodType")]
    public string? HttpMethodType { get; set; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("authScheme")]
    public string? AuthScheme { get; set; }

    [JsonPropertyName("userAuthScheme")]
    public string? UserAuthScheme { get; set; }

    [JsonPropertyName("resourceIdForAuth")]
    public string? ResourceIdForAuth { get; set; }

    [JsonPropertyName("icmHandle")]
    public string? IcmHandle { get; set; }

    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    [JsonPropertyName("skillScope")]
    public List<string>? SkillScope { get; set; }

    [JsonPropertyName("friendlyName")]
    public string? FriendlyName { get; set; }

    [JsonPropertyName("skillType")]
    public string? SkillType { get; set; }

    [JsonPropertyName("phrases")]
    public List<string>? Phrases { get; set; }

    [JsonPropertyName("testPrompts")]
    public List<TestPrompt>? TestPrompts { get; set; }

    [JsonPropertyName("prePlugins")]
    public List<PrePlugin>? PrePlugins { get; set; }

    [JsonPropertyName("postPlugins")]
    public List<object>? PostPlugins { get; set; }

    [JsonPropertyName("entities")]
    public List<object>? Entities { get; set; }

    [JsonPropertyName("userTokensRequired")]
    public List<string>? UserTokensRequired { get; set; }

    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    [JsonPropertyName("scenarioTypeOfSkill")]
    public List<string>? ScenarioTypeOfSkill { get; set; }

    [JsonPropertyName("isPluginActionable")]
    public string? IsPluginActionable { get; set; }
}

/// <summary>
/// Test prompt configuration
/// </summary>
public class TestPrompt
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("entityId")]
    public string? EntityId { get; set; }

    [JsonPropertyName("entityType")]
    public string? EntityType { get; set; }
}

/// <summary>
/// Pre-plugin configuration
/// </summary>
public class PrePlugin
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }
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
