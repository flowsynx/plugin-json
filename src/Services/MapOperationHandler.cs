using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FlowSynx.Plugins.Json.Services;

internal class MapOperationHandler : IJsonOperationHandler
{
    private readonly IGuidProvider _guidProvider;

    public MapOperationHandler(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public object Handle(JToken json, InputParameter inputParameter)
    {
        if (inputParameter.Mappings == null)
            throw new InvalidOperationException("Mappings not defined in specifications.");

        object mappedResult;
        List<Dictionary<string, object>>? structuredData = null;

        if (json is JArray array)
        {
            var mappedList = array
                .OfType<JObject>()
                .Select(obj => MapSingleObject(obj, inputParameter.Mappings))
                .ToList();
            mappedResult = mappedList;
            structuredData = mappedList
                .Select(obj => obj.ToDictionary(kvp => kvp.Key, kvp => (object)(kvp.Value?.GetType()?.Name ?? "null")))
                .ToList();
        }
        else if (json is JObject obj)
        {
            var mappedObj = MapSingleObject(obj, inputParameter.Mappings);
            mappedResult = mappedObj;
            structuredData = new List<Dictionary<string, object>>
            {
                mappedObj.ToDictionary(kvp => kvp.Key, kvp => (object)(kvp.Value?.GetType()?.Name ?? "null"))
            };
        }
        else
        {
            throw new NotSupportedException("HandleMap only supports JSON objects or arrays of objects.");
        }

        string convertedJson = JsonConvert.SerializeObject(mappedResult, inputParameter.Indented ? Formatting.Indented : Formatting.None);
        string filename = $"{_guidProvider.NewGuid()}.json";
        return new PluginContext(filename, "Data")
        {
            Format = "Json",
            Content = convertedJson,
            StructuredData = structuredData
        };
    }

    private Dictionary<string, object?> MapSingleObject(JObject obj, Dictionary<string, string> mappings)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kvp in mappings)
        {
            var value = obj.SelectToken(kvp.Value);
            result[kvp.Key] = value?.Type == JTokenType.Object || value?.Type == JTokenType.Array
                ? value
                : value?.ToString();
        }

        return result;
    }
}