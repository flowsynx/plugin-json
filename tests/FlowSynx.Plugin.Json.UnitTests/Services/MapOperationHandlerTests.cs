using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using FlowSynx.Plugins.Json.Services;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FlowSynx.Plugin.Json.UnitTests.Services;


public class MapOperationHandlerTests
{
    private readonly Mock<IGuidProvider> _guidProviderMock;
    private readonly MapOperationHandler _handler;

    public MapOperationHandlerTests()
    {
        _guidProviderMock = new Mock<IGuidProvider>();
        _guidProviderMock.Setup(g => g.NewGuid()).Returns(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        _handler = new MapOperationHandler(_guidProviderMock.Object);
    }

    [Fact]
    public void Handle_Should_Map_JObject_Correctly()
    {
        // Arrange
        var json = JObject.Parse(@"{
            'name': 'John',
            'details': {
                'age': 30,
                'city': 'New York'
            }
        }");

        var inputParameter = new InputParameter
        {
            Indented = false,
            Mappings = new Dictionary<string, string>
            {
                ["FullName"] = "$.name",
                ["Age"] = "$.details.age",
                ["City"] = "$.details.city"
            }
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        var expectedContent = "{\"FullName\":\"John\",\"Age\":\"30\",\"City\":\"New York\"}";
        Assert.Equal("11111111-1111-1111-1111-111111111111.json", result.Id);
        Assert.Equal("Data", result.SourceType);
        Assert.Equal("Json", result.Format);
        Assert.Equal(expectedContent, result.Content);
    }

    [Fact]
    public void Handle_Should_Map_JArray_Correctly()
    {
        // Arrange
        var json = JArray.Parse(@"[
            { 'name': 'Alice', 'age': 25 },
            { 'name': 'Bob', 'age': 35 }
        ]");

        var inputParameter = new InputParameter
        {
            Indented = true,
            Mappings = new Dictionary<string, string>
            {
                ["UserName"] = "$.name",
                ["UserAge"] = "$.age"
            }
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        var expectedContent = JsonConvert.SerializeObject(new[]
        {
            new Dictionary<string, object?> { ["UserName"] = "Alice", ["UserAge"] = "25" },
            new Dictionary<string, object?> { ["UserName"] = "Bob", ["UserAge"] = "35" }
        }, Formatting.Indented);

        Assert.Equal("11111111-1111-1111-1111-111111111111.json", result.Id);
        Assert.Equal(expectedContent, result.Content);
    }

    [Fact]
    public void Handle_Should_Throw_When_Mappings_Are_Null()
    {
        // Arrange
        var json = JObject.Parse("{}");
        var inputParameter = new InputParameter
        {
            Indented = false,
            Mappings = null
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _handler.Handle(json, inputParameter));
        Assert.Equal("Mappings not defined in specifications.", ex.Message);
    }

    [Fact]
    public void Handle_Should_Throw_For_Unsupported_JToken_Types()
    {
        // Arrange
        var json = JValue.CreateString("some string");
        var inputParameter = new InputParameter
        {
            Indented = false,
            Mappings = new Dictionary<string, string>()
        };

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _handler.Handle(json, inputParameter));
        Assert.Equal("HandleMap only supports JSON objects or arrays of objects.", ex.Message);
    }
}