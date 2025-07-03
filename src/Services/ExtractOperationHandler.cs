using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Services;

internal class ExtractOperationHandler : IJsonOperationHandler
{
    private readonly IGuidProvider _guidProvider;

    public ExtractOperationHandler(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public object Handle(JToken json, InputParameter inputParameter)
    {
        string? path = inputParameter.Path;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path parameter is required for extract.");

        var tokens = json.SelectTokens(path).ToList();
        string filename = $"{_guidProvider.NewGuid()}.json";

        if (tokens.Count == 0)
        {
            return new PluginContext(filename, "Data")
            {
                Format = "Json",
                Content = "null"
            };
        }

        JToken result = tokens.Count == 1
            ? tokens[0]
            : new JArray(tokens);

        return new PluginContext(filename, "Data")
        {
            Format = "Json",
            Content = result.ToString(inputParameter.Indented ? Formatting.Indented : Formatting.None)
        };
    }
}