using FlowSynx.PluginCore;

namespace FlowSynx.Plugins.Json.Operations.Extract;

internal class ExtractParameters
{
    [OperationParameterMetadata(Description = "The JSON data to extract information from.", IsRequired = true)]
    public object? Data { get; set; }

    [OperationParameterMetadata(Description = "The JSON path to extract specific data.", IsRequired = true)]
    public string? Path { get; set; }

    [OperationParameterMetadata(Description = "Whether to indent the output JSON for readability.", IsRequired = false)]
    public bool Indented { get; set; } = false;
}