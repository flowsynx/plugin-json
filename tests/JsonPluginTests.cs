using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json;
using FlowSynx.Plugins.Json.Services;
using Moq;

namespace FlowSynx.Plugin.Json.UnitTests;

public class JsonPluginTests
{
    private readonly JsonPlugin _plugin;
    private readonly Mock<IPluginLogger> _loggerMock;
    private readonly Mock<IReflectionGuard> _reflectionGuardMock;
    private readonly Mock<IGuidProvider> _guidProviderMock;

    public JsonPluginTests()
    {
        _guidProviderMock = new Mock<IGuidProvider>();
        _reflectionGuardMock = new Mock<IReflectionGuard>();
        _loggerMock = new Mock<IPluginLogger>();

        // Provide fixed GUID for tests
        _guidProviderMock.Setup(g => g.NewGuid()).Returns(Guid.NewGuid);

        // Setup default ReflectionGuard to false (not called via reflection)
        _reflectionGuardMock.Setup(r => r.IsCalledViaReflection()).Returns(false);

        // Inject mocks into JsonPlugin
        _plugin = new JsonPlugin(_guidProviderMock.Object, _reflectionGuardMock.Object);
    }

    [Fact]
    public async Task Initialize_SetsLoggerAndMarksAsInitialized()
    {
        // Act
        await _plugin.Initialize(_loggerMock.Object);

        // Assert
        Assert.True(GetPrivateField<bool>(_plugin, "_isInitialized"));
    }

    [Fact]
    public async Task Initialize_WithNullLogger_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _plugin.Initialize(null!));
    }

    [Fact]
    public async Task Initialize_WhenCalledViaReflection_ThrowsInvalidOperationException()
    {
        _reflectionGuardMock.Setup(r => r.IsCalledViaReflection()).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _plugin.Initialize(_loggerMock.Object));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        var parameters = CreateValidParameters("extract");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _plugin.ExecuteAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledViaReflection_ThrowsInvalidOperationException()
    {
        await _plugin.Initialize(_loggerMock.Object);
        _reflectionGuardMock.Setup(r => r.IsCalledViaReflection()).Returns(true);

        var parameters = CreateValidParameters("extract");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _plugin.ExecuteAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedOperation_ThrowsNotSupportedException()
    {
        await _plugin.Initialize(_loggerMock.Object);

        var parameters = CreateValidParameters("invalidOperation");

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _plugin.ExecuteAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullInputData_ThrowsArgumentNullException()
    {
        await _plugin.Initialize(_loggerMock.Object);

        var parameters = new PluginParameters
        {
            { "Operation", "extract" },
            { "Data", (object?)null }
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _plugin.ExecuteAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithListOfPluginContext_ThrowsNotSupportedException()
    {
        await _plugin.Initialize(_loggerMock.Object);

        var parameters = new PluginParameters
        {
            { "Operation", "extract" },
            { "Data", new List<PluginContext>() }
        };

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _plugin.ExecuteAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidExtractOperation_ReturnsExpectedResult()
    {
        await _plugin.Initialize(_loggerMock.Object);

        var parameters = CreateValidParameters("extract");
        parameters.TryAdd("Path", "$.key");
        var result = await _plugin.ExecuteAsync(parameters, CancellationToken.None);

        Assert.NotNull(result);
        // Optionally verify expected result content
    }

    [Fact]
    public void SupportedOperations_ShouldContainAllExpectedOperations()
    {
        var operations = _plugin.SupportedOperations;

        Assert.Contains("extract", operations);
        Assert.Contains("map", operations);
        Assert.Contains("transform", operations);
        Assert.Equal(3, operations.Count);
    }

    private PluginParameters CreateValidParameters(string operation)
    {
        return new PluginParameters
        {
            { "Operation", operation },
            { "Data", new PluginContext("Test", "Data")
                {
                    Content = "{ \"key\": \"value\" }"
                } 
            }
        };
    }

    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)field!.GetValue(obj)!;
    }
}