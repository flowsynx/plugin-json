﻿using FlowSynx.PluginCore;
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