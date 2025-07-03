using FlowSynx.Plugins.Json.Models;
using Newtonsoft.Json.Linq;

namespace FlowSynx.Plugins.Json.Services;

internal interface IJsonOperationHandler
{
    object Handle(JToken json, InputParameter inputParameter);
}