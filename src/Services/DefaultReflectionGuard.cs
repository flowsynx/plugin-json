using FlowSynx.PluginCore.Helpers;

namespace FlowSynx.Plugins.Json.Services;

internal class DefaultReflectionGuard : IReflectionGuard
{
    public bool IsCalledViaReflection() => ReflectionHelper.IsCalledViaReflection();
}