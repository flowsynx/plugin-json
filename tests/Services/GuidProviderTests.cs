using FlowSynx.Plugins.Json.Services;

namespace FlowSynx.Plugin.Json.UnitTests.Services;

public class GuidProviderTests
{
    [Fact]
    public void NewGuid_ShouldReturnUniqueGuid()
    {
        // Arrange
        var guidProvider = new GuidProvider();

        // Act
        var guid1 = guidProvider.NewGuid();
        var guid2 = guidProvider.NewGuid();

        // Assert
        Assert.NotEqual(guid1, guid2);
    }

    [Fact]
    public void NewGuid_ShouldReturnNonEmptyGuid()
    {
        // Arrange
        var guidProvider = new GuidProvider();

        // Act
        var guid = guidProvider.NewGuid();

        // Assert
        Assert.NotEqual(Guid.Empty, guid);
    }
}