namespace FlowSynx.Plugins.Json.Models;

internal class InputParameter
{
    public string Operation { get; set; } = "extract";
    public object? Data { get; set; }
    public string? Path { get; set; }
    public Dictionary<string, string>? Mappings { get; set; }
    public bool Flatten { get; set; } = false;
    public bool Indented { get; set; } = false;
}