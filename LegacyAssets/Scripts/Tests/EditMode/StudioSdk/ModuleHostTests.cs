using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class ModuleHostTests
    {
        private sealed class FakeModule : IPlaycenterModule
        {
            public string Id { get; }
            public float Weight { get; }
            public int Runs;
            public ModuleResult ResultToReturn = ModuleResult.Ok();
            public FakeModule(string id, float weight = 1f) { Id = id; Weight = weight; }
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                Runs++;
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ResultToReturn);
            }
        }

        private sealed class RecordingModule : IPlaycenterModule
        {
            private readonly List<string> _order;
            public string Id { get; }
            public float Weight => 1f;
            public RecordingModule(string id, List<string> order) { Id = id; _order = order; }
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                _order.Add(Id);
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ModuleResult.Ok());
            }
        }

        [Test]
        public async Task RunAsync_AllOk_ReturnsNullAndRunsInOrder()
        {
            var order = new List<string>();
            var m1 = new RecordingModule("a", order);
            var m2 = new RecordingModule("b", order);
            var host = new ModuleHost();
            var reg = new ServiceRegistry().Build();
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            var ctx = new ModuleContext(reg, progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1, m2 }, ctx, CancellationToken.None);

            Assert.IsNull(fail);
            CollectionAssert.AreEqual(new[] { "a", "b" }, order);
            Assert.AreEqual(1f, progress.Overall01, 0.001f);
        }

        [Test]
        public async Task RunAsync_WhenModuleFails_StopsAndReturnsFailure()
        {
            var m1 = new FakeModule("a");
            var m2 = new FakeModule("b")
            {
                ResultToReturn = ModuleResult.Fail(BootFailureCode.Offline, "down")
            };
            var m3 = new FakeModule("c");
            var host = new ModuleHost();
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f), ("c", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1, m2, m3 }, ctx, CancellationToken.None);

            Assert.IsNotNull(fail);
            Assert.AreEqual(BootFailureCode.Offline, fail.Code);
            Assert.AreEqual("b", fail.FailedModuleId);
            Assert.AreEqual(1, m1.Runs);
            Assert.AreEqual(1, m2.Runs);
            Assert.AreEqual(0, m3.Runs);
        }

        [Test]
        public async Task RunAsync_WhenCancelled_ReturnsCancelled()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var host = new ModuleHost();
            var m1 = new FakeModule("a");
            var progress = new BootProgress(new[] { ("a", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);

            BootFailure fail = await host.RunAsync(new IPlaycenterModule[] { m1 }, ctx, cts.Token);

            Assert.IsNotNull(fail);
            Assert.AreEqual(BootFailureCode.Cancelled, fail.Code);
            Assert.AreEqual(0, m1.Runs);
        }

        [Test]
        public async Task RetryFromAsync_ReRunsFromFailedModule()
        {
            var m1 = new FakeModule("a");
            var m2 = new FakeModule("b")
            {
                ResultToReturn = ModuleResult.Fail(BootFailureCode.Offline, "down")
            };
            var host = new ModuleHost();
            var modules = new IPlaycenterModule[] { m1, m2 };
            var progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            var ctx = new ModuleContext(new ServiceRegistry().Build(), progress);
            await host.RunAsync(modules, ctx, CancellationToken.None);

            m2.ResultToReturn = ModuleResult.Ok();
            progress = new BootProgress(new[] { ("a", 1f), ("b", 1f) });
            ctx = new ModuleContext(new ServiceRegistry().Build(), progress);
            BootFailure fail = await host.RetryFromAsync("b", modules, ctx, CancellationToken.None);

            Assert.IsNull(fail);
            Assert.AreEqual(1, m1.Runs); // not re-run
            Assert.AreEqual(2, m2.Runs);
        }
    }
}
