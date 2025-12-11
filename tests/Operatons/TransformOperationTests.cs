using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Operations.Transform;

namespace FlowSynx.Plugin.Json.UnitTests.Operatons
{
    public class TransformOperationTests
    {
        [Fact]
        public async Task ExecuteAsync_Passthrough_WhenNotFlatten()
        {
            var json = "{ \"a\": 1 }";
            var op = new TransformOperation();
            var parameters = new TransformParameters { Data = new PluginContext("id","Data") { Content = json }, Flatten = false, Indented = false };
            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.Equal(json, ctx!.Content);
            Assert.Equal("Json", ctx.Format);
            Assert.NotNull(ctx.StructuredData);
        }

        [Fact]
        public async Task ExecuteAsync_Flatten_Object_Works()
        {
            var json = "{ \"a\": { \"b\": { \"c\": 1 } }, \"x\": 2 }";
            var op = new TransformOperation();
            var parameters = new TransformParameters { Data = new PluginContext("id","Data") { Content = json }, Flatten = true, Indented = false };
            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.NotNull(ctx);
            Assert.Equal("{\"a.b.c\":1,\"x\":2}", ctx!.Content);
        }

        [Fact]
        public async Task ExecuteAsync_Flatten_Array_Throws()
        {
            var json = "[1,2,3]";
            var op = new TransformOperation();
            var parameters = new TransformParameters { Data = new PluginContext("id","Data") { Content = json }, Flatten = true };
            await Assert.ThrowsAsync<NotSupportedException>(() => op.ExecuteAsync(parameters, CancellationToken.None));
        }
    }
}
