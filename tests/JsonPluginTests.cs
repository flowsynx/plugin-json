using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowSynx.PluginCore;
using FlowSynx.Plugins.Json;
using FlowSynx.Plugins.Json.Services;
using Xunit;

namespace FlowSynx.Plugin.Json.UnitTests
{
    public class JsonPluginTests
    {
        private class FakeReflectionGuard : IReflectionGuard
        {
            public bool IsCalledViaReflection() => false;
        }

        private class FakeGuidProvider : IGuidProvider
        {
            public Guid NewGuid() => Guid.Parse("00000000-0000-0000-0000-000000000002");
        }

        private JsonPlugin CreatePlugin() => new JsonPlugin(new FakeGuidProvider(), new FakeReflectionGuard());

        [Fact]
        public void Metadata_HasExpectedValues()
        {
            var plugin = CreatePlugin();
            var md = plugin.Metadata;
            Assert.Equal("Json", md.Name);
            Assert.Equal("FlowSynx", md.CompanyName);
            Assert.Equal(PluginCategory.Data, md.Category);
        }

        [Fact]
        public async Task InitializeAsync_SetsSpecifications()
        {
            var plugin = CreatePlugin();
            var logger = new TestLogger();
            await plugin.InitializeAsync(logger, new Dictionary<string, object?>());
            Assert.NotNull(plugin.Specifications);
        }

        [Fact]
        public async Task ExecuteAsync_BeforeInit_Throws()
        {
            var plugin = CreatePlugin();
            await Assert.ThrowsAsync<InvalidOperationException>(() => plugin.ExecuteAsync("extract", new PluginParameters(), CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteAsync_UnsupportedOperation_Throws()
        {
            var plugin = CreatePlugin();
            var logger = new TestLogger();
            await plugin.InitializeAsync(logger, null);
            await Assert.ThrowsAsync<NotSupportedException>(() => plugin.ExecuteAsync("unknown", new PluginParameters(), CancellationToken.None));
        }

        private class TestLogger : IPluginLogger
        {
            public void Log(PluginLoggerLevel level, string message) { }
            public void Log(PluginLoggerLevel level, string message, Exception ex) { }
            public void Log(PluginLoggerLevel level, string message, IDictionary<string, object> data) { }
        }
    }
}
