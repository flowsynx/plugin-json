using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Operations.Map;
using Xunit;

namespace FlowSynx.Plugin.Json.UnitTests.Operatons
{
    public class MapOperationTests
    {
        [Fact]
        public async Task ExecuteAsync_NoMappings_Throws()
        {
            var op = new MapOperation();
            var parameters = new MapParameters { Data = new PluginContext("id","Data") { Content = "{}" }, Mappings = null };
            await Assert.ThrowsAsync<InvalidOperationException>(() => op.ExecuteAsync(parameters, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_ObjectMapping_Works()
        {
            var json = "{ \"a\": 1, \"b\": { \"c\": 2 } }";
            var op = new MapOperation();
            var parameters = new MapParameters
            {
                Data = new PluginContext("id","Data") { Content = json },
                Mappings = new Dictionary<string, string>
                {
                    { "X", "$.a" },
                    { "Y", "$.b.c" }
                },
                Indented = false
            };

            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.NotNull(ctx);
            Assert.Equal("Json", ctx!.Format);
            Assert.Equal("{\"X\":\"1\",\"Y\":\"2\"}", ctx.Content);
            Assert.NotNull(ctx.StructuredData);
            Assert.Single(ctx.StructuredData!);
            Assert.Equal("String", ctx.StructuredData![0]["X"]);
        }

        [Fact]
        public async Task ExecuteAsync_ArrayMapping_Works()
        {
            var json = "[ { \"a\": 1 }, { \"a\": 3 } ]";
            var op = new MapOperation();
            var parameters = new MapParameters
            {
                Data = new PluginContext("id","Data") { Content = json },
                Mappings = new Dictionary<string, string> { { "X", "$.a" } },
                Indented = true
            };

            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.NotNull(ctx);
            Assert.Contains("\n", ctx!.Content!);
            Assert.NotNull(ctx.StructuredData);
            Assert.Equal(2, ctx.StructuredData!.Count);
        }
    }
}
