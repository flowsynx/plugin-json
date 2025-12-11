using FlowSynx.PluginCore;

namespace FlowSynx.Plugins.Json.Operations.Transform;

internal class TransformParameters
{
    [OperationParameterMetadata(Description = "The JSON data to extract information from.", IsRequired = true)]
    public object? Data { get; set; }

    [OperationParameterMetadata(Description = "Whether to flatten nested JSON objects into a single level.", IsRequired = false)]
    public bool Flatten { get; set; } = false;

    [OperationParameterMetadata(Description = "Whether to indent the output JSON for readability.", IsRequired = false)]
    public bool Indented { get; set; } = false;
}