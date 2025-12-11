using System;
using System.Collections.Generic;
using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json.Helpers;
using FlowSynx.Plugins.Json.Services;
using Xunit;

namespace FlowSynx.Plugin.Json.UnitTests.Helpers
{
    public class ParseDataHelperTests
    {
        private class TestGuidProvider : IGuidProvider
        {
            public Guid NewGuid() => Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        [Fact]
        public void ParseDataToContext_Null_Throws()
        {
            var helper = new ParseDataHelper(new TestGuidProvider());
            Assert.Throws<ArgumentNullException>(() => helper.ParseDataToContext(null));
        }

        [Fact]
        public void ParseDataToContext_String_ReturnsContextWithContent()
        {
            var helper = new ParseDataHelper(new TestGuidProvider());
            var ctx = helper.ParseDataToContext("{ \"a\": 1 }");
            Assert.NotNull(ctx);
            Assert.Equal("{ \"a\": 1 }", ctx.Content);
        }

        [Fact]
        public void ParseDataToContext_PluginContext_PassesThrough()
        {
            var original = new PluginContext("id", "Data") { Content = "content" };
            var helper = new ParseDataHelper(new TestGuidProvider());
            var ctx = helper.ParseDataToContext(original);
            Assert.Same(original, ctx);
        }

        [Fact]
        public void ParseDataToContext_ListOfPluginContext_Throws()
        {
            var list = new List<PluginContext> { new PluginContext("id", "Data") };
            var helper = new ParseDataHelper(new TestGuidProvider());
            Assert.Throws<NotSupportedException>(() => helper.ParseDataToContext(list));
        }
    }
}
