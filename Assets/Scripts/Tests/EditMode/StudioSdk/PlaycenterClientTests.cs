using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.SDK;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class PlaycenterClientTests
    {
        private sealed class OkModule : IPlaycenterModule
        {
            public string Id => "ok";
            public float Weight => 1f;
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
            {
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ModuleResult.Ok());
            }
        }

        private sealed class FailModule : IPlaycenterModule
        {
            public string Id => "bad";
            public float Weight => 1f;
            public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
                => Task.FromResult(ModuleResult.Fail(BootFailureCode.ForceUpdate, "update"));
        }

        private sealed class SpyEntry : IGameEntry
        {
            public int ReadyCount;
            public int FailCount;
            public BootFailure LastFailure;
            public PlaycenterClient ReadyClient;
            public Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct)
            {
                ReadyCount++;
                ReadyClient = client;
                return Task.CompletedTask;
            }
            public Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct)
            {
                FailCount++;
                LastFailure = failure;
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task RunAsync_WhenModulesSucceed_CallsOnPlaycenterReadyOnce()
        {
            var entry = new SpyEntry();
            var client = PlaycenterClient.Create(o =>
            {
                o.SetGameEntry(entry);
                o.AddModule(new OkModule());
                o.UseShell(new NullShellUi());
            });

            await client.RunAsync(CancellationToken.None);

            Assert.AreEqual(1, entry.ReadyCount);
            Assert.AreEqual(0, entry.FailCount);
            Assert.AreSame(client, entry.ReadyClient);
        }

        [Test]
        public async Task RunAsync_WhenModuleFails_CallsOnPlaycenterFailed_NotReady()
        {
            var entry = new SpyEntry();
            var client = PlaycenterClient.Create(o =>
            {
                o.SetGameEntry(entry);
                o.AddModule(new FailModule());
                o.UseShell(new NullShellUi());
            });

            await client.RunAsync(CancellationToken.None);

            Assert.AreEqual(0, entry.ReadyCount);
            Assert.AreEqual(1, entry.FailCount);
            Assert.AreEqual(BootFailureCode.ForceUpdate, entry.LastFailure.Code);
        }

        [Test]
        public void Create_WithoutGameEntry_ThrowsOnRun()
        {
            var client = PlaycenterClient.Create(o => o.AddModule(new OkModule()));
            Assert.ThrowsAsync<System.InvalidOperationException>(async () => await client.RunAsync(CancellationToken.None));
        }
    }
}
