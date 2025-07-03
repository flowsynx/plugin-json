using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Models;
using FlowSynx.Plugins.Json.Services;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSynx.Plugin.Json.UnitTests.Services;

public class TransformOperationHandlerTests
{
    private readonly Mock<IGuidProvider> _guidProviderMock;
    private readonly TransformOperationHandler _handler;

    public TransformOperationHandlerTests()
    {
        _guidProviderMock = new Mock<IGuidProvider>();
        _guidProviderMock.Setup(g => g.NewGuid()).Returns(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        _handler = new TransformOperationHandler(_guidProviderMock.Object);
    }

    [Fact]
    public void Handle_Should_Return_Flattened_Json_When_Flatten_Is_True()
    {
        // Arrange
        var json = JObject.Parse(@"{
            'user': {
                'name': 'Alice',
                'address': {
                    'city': 'Wonderland',
                    'zip': '12345'
                }
            }
        }");

        var inputParameter = new InputParameter
        {
            Flatten = true,
            Indented = false
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        var expectedContent = "{\"user.name\":\"Alice\",\"user.address.city\":\"Wonderland\",\"user.address.zip\":\"12345\"}";

        Assert.Equal("11111111-1111-1111-1111-111111111111.json", result.Id);
        Assert.Equal("Data", result.SourceType);
        Assert.Equal("Json", result.Format);
        Assert.Equal(expectedContent, result.Content);
    }

    [Fact]
    public void Handle_Should_Return_Original_Json_When_Flatten_Is_False()
    {
        // Arrange
        var json = JObject.Parse(@"{
            'name': 'Bob',
            'age': 40
        }");

        var inputParameter = new InputParameter
        {
            Flatten = false,
            Indented = true
        };

        // Act
        var result = (PluginContext)_handler.Handle(json, inputParameter);

        // Assert
        var expectedContent = JsonConvert.SerializeObject(json, Formatting.Indented);

        Assert.Equal("11111111-1111-1111-1111-111111111111.json", result.Id);
        Assert.Equal(expectedContent, result.Content);
    }

    [Fact]
    public void Handle_Should_Throw_When_Flatten_And_Input_Is_Not_JObject()
    {
        // Arrange
        var json = JArray.Parse(@"[1, 2, 3]");
        var inputParameter = new InputParameter
        {
            Flatten = true,
            Indented = false
        };

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => _handler.Handle(json, inputParameter));
        Assert.Equal("Flatten is only supported for JSON objects.", ex.Message);
    }

    [Fact]
    public void FlattenJson_Should_Flatten_Nested_Objects()
    {
        // Arrange
        var nestedJson = JObject.Parse(@"{
            'level1': {
                'level2': {
                    'key': 'value'
                }
            }
        }");

        var flattenMethod = typeof(TransformOperationHandler)
            .GetMethod("FlattenJson", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var flattened = (JObject)flattenMethod.Invoke(_handler, new object[] { nestedJson });

        // Assert
        Assert.Equal("value", flattened["level1.level2.key"]);
    }
}