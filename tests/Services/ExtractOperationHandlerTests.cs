using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using FlowSynx.Plugins.Json.Services;
using Moq;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugin.Json.UnitTests.Services;

public class ExtractOperationHandlerTests
{
    private readonly Mock<IGuidProvider> _guidProviderMock;
    private readonly ExtractOperationHandler _handler;

    public ExtractOperationHandlerTests()
    {
        _guidProviderMock = new Mock<IGuidProvider>();
        _guidProviderMock.Setup(g => g.NewGuid()).Returns(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        _handler = new ExtractOperationHandler(_guidProviderMock.Object);
    }

    [Fact]
    public void Handle_ShouldThrowArgumentException_WhenPathIsNull()
    {
        // Arrange
        var json = JToken.Parse(@"{ ""name"": ""test"" }");
        var inputParameter = new InputParameter
        {
            Path = null
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _handler.Handle(json, inputParameter));
        Assert.Equal("Path parameter is required for extract.", ex.Message);
    }

    [Fact]
    public void Handle_ShouldReturnSingleToken_WhenPathMatchesOne()
    {
        // Arrange
        var json = JToken.Parse(@"{ ""name"": ""John"" }");
        var inputParameter = new InputParameter
        {
            Path = "$.name",
            Indented = false
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json", result.Id);
        Assert.Equal("Json", result.Format);
        Assert.Equal("\"John\"", result.Content); // Should be raw JSON string
    }

    [Fact]
    public void Handle_ShouldReturnArray_WhenPathMatchesMultiple()
    {
        // Arrange
        var json = JToken.Parse(@"{ ""items"": [1, 2, 3] }");
        var inputParameter = new InputParameter
        {
            Path = "$.items[*]",
            Indented = false
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json", result.Id);
        Assert.Equal("Json", result.Format);
        Assert.Equal("[1,2,3]", result.Content); // Array format
    }

    [Fact]
    public void Handle_ShouldReturnNullContent_WhenNoTokensMatch()
    {
        // Arrange
        var json = JToken.Parse(@"{ ""name"": ""John"" }");
        var inputParameter = new InputParameter
        {
            Path = "$.missing",
            Indented = false
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json", result.Id);
        Assert.Equal("Json", result.Format);
        Assert.Equal("null", result.Content);
    }

    [Fact]
    public void Handle_ShouldReturnIndentedJson_WhenIndentedIsTrue()
    {
        // Arrange
        var json = JToken.Parse(@"{ ""person"": { ""name"": ""John"" } }");
        var inputParameter = new InputParameter
        {
            Path = "$.person",
            Indented = true
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json", result.Id);
        Assert.Equal("Json", result.Format);
        Assert.Contains(Environment.NewLine, result.Content); // Should be pretty-printed
        Assert.Contains("  \"name\": \"John\"", result.Content); // Indented JSON
    }
}