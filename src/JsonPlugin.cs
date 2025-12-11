using FlowSynx.PluginCore;
using FlowSynx.PluginCore.Extensions;
using FlowSynx.Plugins.Json.Operations.Extract;
using FlowSynx.Plugins.Json.Operations.Map;
using FlowSynx.Plugins.Json.Operations.Transform;
using FlowSynx.Plugins.Json.Services;

namespace FlowSynx.Plugins.Json;

public class JsonPlugin : IPlugin
{
    private readonly IGuidProvider _guidProvider;
    private readonly IReflectionGuard _reflectionGuard;
    private JsonPluginSpecifications? _specifications = null;
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
        Version = new Version(1, 2, 0),
        Category = PluginCategory.Data,
        Authors = new List<string> { "FlowSynx" },
        Copyright = "© FlowSynx. All rights reserved.",
        Icon = "flowsynx.png",
        ReadMe = "README.md",
        RepositoryUrl = "https://github.com/flowsynx/plugin-json",
        ProjectUrl = "https://flowsynx.io",
        Tags = new List<string>() { "flowSynx", "json", "data", "data-platform" },
        MinimumFlowSynxVersion = new Version(1, 3, 0),
    };

    public IPluginSpecifications? Specifications => _specifications;

    public IReadOnlyCollection<IPluginOperation> SupportedOperations { get; } = new IPluginOperation[]
    {
        new ExtractOperation(),
        new MapOperation(),
        new TransformOperation()
    };

    public Task InitializeAsync(IPluginLogger logger, IDictionary<string, object?>? specifications)
    {
        if (_reflectionGuard.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var jsonSpecifications = new JsonPluginSpecifications();
        if (specifications != null)
            jsonSpecifications.FromDictionary(specifications);

        jsonSpecifications.Validate();
        _specifications = jsonSpecifications;

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public async Task<object?> ExecuteAsync(
        string? operationName,
        PluginParameters parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_reflectionGuard.IsCalledViaReflection())
            throw new InvalidOperationException(Resources.ReflectionBasedAccessIsNotAllowed);

        if (!_isInitialized)
            throw new InvalidOperationException($"Plugin '{Metadata.Name}' v{Metadata.Version} is not initialized.");

        var operation = SupportedOperations
            .FirstOrDefault(op => string.Equals(op.Name, operationName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotSupportedException($"Operation '{operationName}' is not supported.");

        return operation.Name.ToLowerInvariant() switch
        {
            "extract" => await ((ExtractOperation)operation)
                            .ExecuteAsync(parameters.ToObject<ExtractParameters>(), cancellationToken),

            "map" => await ((MapOperation)operation)
                            .ExecuteAsync(parameters.ToObject<MapParameters>(), cancellationToken),

            "transform" => await ((TransformOperation)operation)
                            .ExecuteAsync(parameters.ToObject<TransformParameters>(), cancellationToken),

            _ => throw new InvalidOperationException($"Unsupported operation: {operation.Name}")
        };
    }
}