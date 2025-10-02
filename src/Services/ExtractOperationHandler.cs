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

        List<Dictionary<string, object>>? structuredData = null;
        if (tokens.Count == 0)
        {
            return new PluginContext(filename, "Data")
            {
                Format = "Json",
                Content = "null",
                StructuredData = null
            };
        }

        JToken result = tokens.Count == 1
            ? tokens[0]
            : new JArray(tokens);

        structuredData = GetStructuredData(result);

        return new PluginContext(filename, "Data")
        {
            Format = "Json",
            Content = result.ToString(inputParameter.Indented ? Formatting.Indented : Formatting.None),
            StructuredData = structuredData
        };
    }

    private List<Dictionary<string, object>>? GetStructuredData(JToken token)
    {
        if (token is JArray arr && arr.Count > 0)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var item in arr)
            {
                if (item is JObject obj)
                    list.Add(obj.Properties().ToDictionary(p => p.Name, p => (object)p.Value.Type.ToString()));
            }
            return list.Count > 0 ? list : null;
        }
        if (token is JObject obj2)
        {
            return new List<Dictionary<string, object>>
            {
                obj2.Properties().ToDictionary(p => p.Name, p => (object)p.Value.Type.ToString())
            };
        }
        return null;
    }
}