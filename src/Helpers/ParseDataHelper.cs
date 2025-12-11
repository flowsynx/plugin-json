using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Services;

namespace FlowSynx.Plugins.Json.Helpers;

internal class ParseDataHelper
{
    private readonly IGuidProvider _guidProvider;

    public ParseDataHelper(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
    }

    public PluginContext ParseDataToContext(object? data)
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
