using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Services;

internal class TransformOperationHandler : IJsonOperationHandler
{
    private readonly IGuidProvider _guidProvider;

    public TransformOperationHandler(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public object Handle(JToken json, InputParameter inputParameter)
    {
        JToken result;

        if (inputParameter.Flatten)
        {
            if (json is not JObject obj)
                throw new NotSupportedException("Flatten is only supported for JSON objects.");

            result = FlattenJson(obj);
        }
        else
        {
            result = json;
        }

        var transformedResult = result.ToString(inputParameter.Indented ? Formatting.Indented : Formatting.None);
        string filename = $"{_guidProvider.NewGuid()}.json";
        return new PluginContext(filename, "Data")
        {
            Format = "Json",
            Content = transformedResult
        };
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