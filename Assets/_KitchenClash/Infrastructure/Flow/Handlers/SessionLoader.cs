using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.DI;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Creates the session scope and initializes economy/player data.
    /// Callers decide navigation (NotifyBootComplete vs CompleteSidePhase).
    /// </summary>
    public sealed class SessionLoader
    {
        private readonly SessionManager _sessionManager;
        private readonly ISessionContext _sessionContext;

        public SessionLoader(SessionManager sessionManager, ISessionContext sessionContext)
        {
            _sessionManager = sessionManager;
            _sessionContext = sessionContext;
        }

        public async UniTask LoadAsync(CancellationToken ct = default)
        {
            GameLogger.Log("[SessionLoader] Loading session data...");

            if (!_sessionManager.IsSessionActive)
            {
                _sessionManager.CreateSession();
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
