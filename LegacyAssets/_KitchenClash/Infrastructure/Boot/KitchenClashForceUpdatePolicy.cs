using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Infrastructure.Flow.Handlers;
using Playcenter.SDK;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Boot
{
    /// <summary>
    /// Wraps the existing <see cref="ForceUpdateChecker"/> so the Playcenter SDK module pipeline
    /// can evaluate whether a forced app update is required using game-side remote-config values.
    /// </summary>
    public sealed class KitchenClashForceUpdatePolicy : IForceUpdatePolicy
    {
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IEventBus _eventBus;

        public KitchenClashForceUpdatePolicy(IRemoteConfigService remoteConfigService, IEventBus eventBus)
        {
            _remoteConfigService = remoteConfigService;
            _eventBus = eventBus;
        }

        public async Task<ForceUpdateDecision> EvaluateAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var checker = new ForceUpdateChecker(_remoteConfigService, _eventBus);
            // ForceUpdateChecker is UniTask-based; bridge to Task for the pure SDK policy contract.
            bool required = await checker.CheckForUpdateAsync().AsTask();
            return new ForceUpdateDecision(required, checker.UpdateMessage, checker.UpdateUrl);
        }
    }
}
