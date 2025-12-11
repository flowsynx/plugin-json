using FlowSynx.PluginCore;

namespace FlowSynx.Plugins.Json.Operations.Map;

internal class MapParameters
{
    [OperationParameterMetadata(Description = "The JSON data to extract information from.", IsRequired = true)]
    public object? Data { get; set; }

    [OperationParameterMetadata(Description = "A dictionary defining the mappings from source fields to target fields.", IsRequired = true)]
    public Dictionary<string, string>? Mappings { get; set; }

    [OperationParameterMetadata(Description = "Whether to indent the output JSON for readability.", IsRequired = false)]
    public bool Indented { get; set; } = false;
}