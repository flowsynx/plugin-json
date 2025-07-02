namespace FlowSynx.Plugins.Json.Models;

internal class InputParameter
{
    public string Operation { get; set; } = "extract";
    public string? Json { get; set; }
    public string? jsonPath { get; set; }
    public Dictionary<string, string>? Mappings { get; set; }
    public bool Flatten { get; set; } = false;
}