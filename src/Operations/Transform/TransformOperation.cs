using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Helpers;
using FlowSynx.Plugins.Json.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Operations.Transform;

internal class TransformOperation : IPluginOperation<TransformParameters, PluginContext>
{
    private readonly IGuidProvider _guidProvider = new GuidProvider();

    public string Name => "Transform";
    public string Description => "Transforms the input data into a different format.";

    public async Task<PluginContext?> ExecuteAsync(TransformParameters parameters, CancellationToken cancellationToken)
    {
        var helper = new ParseDataHelper(_guidProvider);

        // Parse the input content string, not the PluginContext itself
        var context = helper.ParseDataToContext(parameters.Data);
        var jsonToken = JToken.Parse(context.Content);

        JToken result;

        if (parameters.Flatten)
        {
            if (jsonToken is not JObject obj)
                throw new NotSupportedException("Flatten is only supported for JSON objects.");

            result = FlattenJson(obj);
        }
        else
        {
            result = jsonToken;
        }

        // Preserve original content formatting when not flattening; otherwise serialize according to Indented flag
        var transformedResult = parameters.Flatten
            ? result.ToString(parameters.Indented ? Formatting.Indented : Formatting.None)
            : context.Content;

        string filename = $"{_guidProvider.NewGuid()}.json";
        var structuredData = GetStructuredData(result);
        return new PluginContext(filename, "Data")
        {
            Format = "Json",
            Content = transformedResult,
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

    private JObject FlattenJson(JObject input)
    {
        var result = new JObject();

        void Flatten(JObject obj, string prefix)
        {
            foreach (var prop in obj.Properties())
            {
                var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                if (prop.Value is JObject nested)
                    Flatten(nested, path);
                else
                    result[path] = prop.Value;
            }
        }

        Flatten(input, "");
        return result;
    }
}