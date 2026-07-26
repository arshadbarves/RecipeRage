using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Creates the session scope and initializes economy/player data.
    /// Callers decide navigation (ReturnHome vs CompleteSidePhase / EnterSidePhase).
    /// </summary>
    public sealed class SessionLoader
    {
        private readonly ISessionLifecycle _sessionLifecycle;
        private readonly ISessionContext _sessionContext;

        public SessionLoader(ISessionLifecycle sessionLifecycle, ISessionContext sessionContext)
        {
            _sessionLifecycle = sessionLifecycle;
            _sessionContext = sessionContext;
        }

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            GameLogger.Log("[SessionLoader] Loading session data...");

            if (!_sessionLifecycle.IsSessionActive)
            {
                _sessionLifecycle.CreateSession();
            }

            ct.ThrowIfCancellationRequested();

            IEconomyService economyService = _sessionContext.EconomyService;
            economyService?.Initialize();
            ct.ThrowIfCancellationRequested();

            IPlayerDataService playerDataService = _sessionContext.PlayerDataService;
            playerDataService?.Initialize();
            ct.ThrowIfCancellationRequested();

            await UniTask.Delay(300, cancellationToken: ct);
            GameLogger.Log("[SessionLoader] Session load complete.");
        }
    }
}
