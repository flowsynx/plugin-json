using FlowSynx.PluginCore;
using FlowSynx.PluginCore.Extensions;
using FlowSynx.Plugins.Json.Models;
using FlowSynx.Plugins.Json.Services;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json;

public class JsonPlugin : IPlugin
{
    private readonly IGuidProvider _guidProvider;
    private readonly IReflectionGuard _reflectionGuard;
    private IPluginLogger? _logger;
    private bool _isInitialized;

    public JsonPlugin() : this(new GuidProvider(), new DefaultReflectionGuard()) { }

    internal JsonPlugin(IGuidProvider guidProvider, IReflectionGuard reflectionGuard)
    {
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
        _reflectionGuard = reflectionGuard ?? throw new ArgumentNullException(nameof(reflectionGuard));
    }

    public PluginMetadata Metadata => new()
    {
        Id = Guid.Parse("61519421-6eb9-466b-aaed-366098da1922"),
        Name = "Json",
        CompanyName = "FlowSynx",
        Description = Resources.PluginDescription,
        Version = new Version(1, 1, 0),
        Category = PluginCategory.Data,
        Authors = new List<string> { "FlowSynx" },
        Copyright = "© FlowSynx. All rights reserved.",
        Icon = "flowsynx.png",
        ReadMe = "README.md",
        RepositoryUrl = "https://github.com/flowsynx/plugin-json",
        ProjectUrl = "https://flowsynx.io",
        Tags = new List<string>() { "flowSynx", "json", "data", "data-platform" },
        MinimumFlowSynxVersion = new Version(1, 1, 1),
    };

    public PluginSpecifications? Specifications { get; set; }

    public Type SpecificationsType => typeof(JsonPluginSpecifications);

    private Dictionary<string, IJsonOperationHandler> OperationMap => new(StringComparer.OrdinalIgnoreCase)
    {
        ["extract"] = new ExtractOperationHandler(_guidProvider),
        ["map"] = new MapOperationHandler(_guidProvider),
        ["transform"] = new TransformOperationHandler(_guidProvider)
    };

    public IReadOnlyCollection<string> SupportedOperations => OperationMap.Keys;

    public Task Initialize(IPluginLogger logger)
    {
        if (_reflectionGuard.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public Task<object?> ExecuteAsync(PluginParameters parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_reflectionGuard.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        if (!_isInitialized)
            throw new InvalidOperationException($"Plugin '{Metadata.Name}' v{Metadata.Version} is not initialized.");

        var inputParameter = parameters.ToObject<InputParameter>();
        if (!OperationMap.TryGetValue(inputParameter.Operation, out var handler))
        {
            throw new NotSupportedException($"Operation '{inputParameter.Operation}' is not supported.");
        }

        var context = ParseDataToContext(inputParameter.Data);
        var json = context.Content ?? throw new ArgumentException("Input JSON is required.");

        var jsonToken = JToken.Parse(json);
        return Task.FromResult(handler.Handle(jsonToken, inputParameter));
    }

    private PluginContext ParseDataToContext(object? data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        return data switch
        {
            PluginContext singleContext => singleContext,
            IEnumerable<PluginContext> => throw new NotSupportedException("List of PluginContext is not supported."),
            string strData => new PluginContext(_guidProvider.NewGuid().ToString(), "Data") { Content = strData },
            _ => throw new NotSupportedException("Unsupported input data format.")
        };
    }
}