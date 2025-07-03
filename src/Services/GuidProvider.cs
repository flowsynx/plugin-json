namespace FlowSynx.Plugins.Json.Services;

internal class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}