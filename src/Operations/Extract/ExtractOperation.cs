using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Helpers;
using FlowSynx.Plugins.Json.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Operations.Extract;

internal class ExtractOperation : IPluginOperation<ExtractParameters, PluginContext>
{
    private readonly IGuidProvider _guidProvider = new GuidProvider();

    public string Name => "Extract";
    public string Description => "Extracts data from a JSON content.";

    public async Task<PluginContext?> ExecuteAsync(ExtractParameters parameters, CancellationToken cancellationToken)
    {
        string? path = parameters.Path;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path parameter is required for extract.");

        var helper = new ParseDataHelper(_guidProvider);

        var jsonCtx = helper.ParseDataToContext(parameters.Data);
        if (string.IsNullOrWhiteSpace(jsonCtx.Content))
            throw new ArgumentException("Input JSON content cannot be empty.");
        var jsonToken = JToken.Parse(jsonCtx.Content);
        var tokens = jsonToken.SelectTokens(path).ToList();
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
            Content = result.ToString(parameters.Indented ? Formatting.Indented : Formatting.None),
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
                else if (item is JValue jv)
                    list.Add(new Dictionary<string, object> { { "$value", jv.Type.ToString() } });
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
        if (token is JValue jv2)
        {
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "$value", jv2.Type.ToString() } }
            };
        }
        return null;
    }
}