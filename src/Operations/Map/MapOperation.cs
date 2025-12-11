using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Helpers;
using FlowSynx.Plugins.Json.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Operations.Map;

internal class MapOperation : IPluginOperation<MapParameters, PluginContext>
{
    private readonly IGuidProvider _guidProvider = new GuidProvider();

    public string Name => "Map";
    public string Description => "Maps data from one structure to another.";

    public async Task<PluginContext?> ExecuteAsync(MapParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Mappings == null)
            throw new InvalidOperationException("Mappings not defined in specifications.");

        var helper = new ParseDataHelper(_guidProvider);

        var context = helper.ParseDataToContext(parameters.Data);
        var jsonToken = JToken.Parse(context.Content);

        object mappedResult;
        List<Dictionary<string, object>>? structuredData = null;

        if (jsonToken is JArray array)
        {
            var mappedList = array
                .OfType<JObject>()
                .Select(obj => MapSingleObject(obj, parameters.Mappings))
                .ToList();
            mappedResult = mappedList;
            structuredData = mappedList
                .Select(obj => obj.ToDictionary(kvp => kvp.Key, kvp => (object)(kvp.Value?.GetType()?.Name ?? "null")))
                .ToList();
        }
        else if (jsonToken is JObject obj)
        {
            var mappedObj = MapSingleObject(obj, parameters.Mappings);
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
            
        string convertedJson = JsonConvert.SerializeObject(mappedResult, parameters.Indented ? Formatting.Indented : Formatting.None);
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
            if (value == null)
            {
                result[kvp.Key] = null;
                continue;
            }

            // Convert primitives to CLR strings to avoid leaking JValue into the result
            switch (value.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:
                    // Keep complex types as-is
                    result[kvp.Key] = value;
                    break;
                default:
                    // Ensure primitives are represented as strings (as expected by tests)
                    result[kvp.Key] = value.ToString();
                    break;
            }
        }

        return result;
    }
}