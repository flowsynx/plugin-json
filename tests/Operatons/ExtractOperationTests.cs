using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Operations.Extract;
using Xunit;

namespace FlowSynx.Plugin.Json.UnitTests.Operatons
{
    public class ExtractOperationTests
    {
        [Fact]
        public async Task ExecuteAsync_InvalidPath_Throws()
        {
            var op = new ExtractOperation();
            var parameters = new ExtractParameters { Data = new PluginContext("id","Data") { Content = "{}" }, Path = "  " };
            await Assert.ThrowsAsync<ArgumentException>(() => op.ExecuteAsync(parameters, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_NoTokens_ReturnsNullContent()
        {
            var op = new ExtractOperation();
            var pc = new PluginContext("id","Data") { Content = "{ \"a\": 1 }" };
            var parameters = new ExtractParameters { Data = pc, Path = "$.b" };
            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.NotNull(ctx);
            Assert.Equal("null", ctx!.Content);
            Assert.Null(ctx.StructuredData);
        }

        [Fact]
        public async Task ExecuteAsync_SingleToken_Unindented()
        {
            var op = new ExtractOperation();
            var pc = new PluginContext("id","Data") { Content = "{ \"a\": { \"b\": 2 } }" };
            var parameters = new ExtractParameters { Data = pc, Path = "$.a.b", Indented = false };
            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.Equal("2", ctx!.Content);
            Assert.NotNull(ctx.StructuredData);
            Assert.Single(ctx.StructuredData!);
        }

        [Fact]
        public async Task ExecuteAsync_MultipleTokens_Indented()
        {
            var op = new ExtractOperation();
            var pc = new PluginContext("id","Data") { Content = "{ \"a\": [ { \"b\": 1 }, { \"b\": 2 } ] }" };
            var parameters = new ExtractParameters { Data = pc, Path = "$.a[*]", Indented = true };
            var ctx = await op.ExecuteAsync(parameters, CancellationToken.None);
            Assert.NotNull(ctx);
            Assert.Contains("\n", ctx!.Content!); // indented
            Assert.NotNull(ctx.StructuredData);
            Assert.Equal(2, ctx.StructuredData!.Count);
        }
    }
}
