using FlowSynx.PluginCore;
using FlowSynx.PluginCore.Extensions;
using FlowSynx.PluginCore.Helpers;
using FlowSynx.Plugins.Json.Models;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json;

public class JsonPlugin : IPlugin
{
    private IPluginLogger? _logger;
    private bool _isInitialized;

    public PluginMetadata Metadata
    {
        get
        {
            return new PluginMetadata
            {
                Id = Guid.Parse("61519421-6eb9-466b-aaed-366098da1922"),
                Name = "Json",
                CompanyName = "FlowSynx",
                Description = Resources.PluginDescription,
                Version = new PluginVersion(1, 0, 0),
                Category = PluginCategory.Data,
                Authors = new List<string> { "FlowSynx" },
                Copyright = "© FlowSynx. All rights reserved.",
                Icon = "flowsynx.png",
                ReadMe = "README.md",
                RepositoryUrl = "https://github.com/flowsynx/plugin-json",
                ProjectUrl = "https://flowsynx.io",
                Tags = new List<string>() { "flowSynx", "json", "data", "data-platform" }
            };
        }
    }

    public PluginSpecifications? Specifications { get; set; }

    public Type SpecificationsType => typeof(JsonPluginSpecifications);

    private Dictionary<string, Func<JObject, InputParameter, object>> OperationMap => new(StringComparer.OrdinalIgnoreCase)
    {
        ["extract"] = HandleExtract,
        ["map"] = HandleMap,
        ["transform"] = HandleTransform
    };

    public IReadOnlyCollection<string> SupportedOperations => new[] { "extract", "map", "transform" };

    public Task Initialize(IPluginLogger logger)
    {
        if (ReflectionHelper.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public async Task<object?> ExecuteAsync(PluginParameters parameters, CancellationToken cancellationToken)
    {
        if (ReflectionHelper.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        if (!_isInitialized)
            throw new InvalidOperationException($"Plugin '{Metadata.Name}' v{Metadata.Version} is not initialized.");

        var inputParameter = parameters.ToObject<InputParameter>();
        var operation = inputParameter.Operation;

        if (OperationMap.TryGetValue(operation, out var handler))
        {
            var json = inputParameter.Json ?? throw new ArgumentException("Input JSON is required.");
            var jsonObj = JObject.Parse(json);

            return handler(jsonObj, inputParameter);
        }

        throw new NotSupportedException($"Json plugin: Operation '{operation}' is not supported.");
    }

    private object HandleExtract(JObject json, InputParameter inputParameter)
    {
        string? path = inputParameter.jsonPath;
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("jsonPath parameter is required for extract.");

        var token = json.SelectToken(path);
        return token?.ToString() ?? "null";
    }

    private object HandleMap(JObject json, InputParameter inputParameter)
    {
        if (inputParameter.Mappings == null)
            throw new InvalidOperationException("Mappings not defined in specifications.");

        var result = new Dictionary<string, object?>();
        foreach (var kvp in inputParameter.Mappings)
        {
            result[kvp.Key] = json.SelectToken(kvp.Value)?.ToString();
        }

        return result;
    }

    private object HandleTransform(JObject json, InputParameter inputParameter)
    {
        var result = json;

        if (inputParameter.Flatten)
            result = FlattenJson(json);

        return result;
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