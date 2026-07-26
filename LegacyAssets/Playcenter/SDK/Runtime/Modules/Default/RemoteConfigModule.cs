using System;
using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.SDK
{
    public sealed class RemoteConfigModule : IPlaycenterModule
    {
        public string Id => "remote_config";
        public float Weight => 0.15f;

        public async Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            if (!context.Services.TryGet<IRemoteConfigService>(out var remoteConfig))
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Fail(BootFailureCode.RemoteConfig, "IRemoteConfigService is not registered.");
            }

            bool initialized;
            try
            {
                context.Progress.Report(Id, 0.4f);
                initialized = await remoteConfig.Initialize();
            }
            catch (Exception ex)
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Fail(BootFailureCode.RemoteConfig,
                    $"Remote config initialization threw: {ex.Message}");
            }

            if (!initialized)
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Fail(BootFailureCode.RemoteConfig,
                    "Remote config initialization returned false.");
            }

            // Best-effort refresh; ignore failure so boot is not blocked.
            try
            {
                context.Progress.Report(Id, 0.8f);
                await remoteConfig.RefreshConfig();
            }
            catch
            {
                // intentionally swallowed
            }

            context.Progress.Report(Id, 1f);
            return ModuleResult.Ok();
        }
    }
}
