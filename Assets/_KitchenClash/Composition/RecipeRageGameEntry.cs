using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Config;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Flow.Handlers;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Composition
{
    /// <summary>
    /// SDK game-entry handoff for RecipeRage. Called by PlaycenterClient after all boot modules pass.
    /// <para>
    /// Ready path:
    ///   1. If auth ProductUserId is empty → enter Login side phase via IAppFlow.
    ///   2. Else → SessionLoader.LoadAsync then IAppFlow.ReturnHome().
    /// </para>
    /// <para>
    /// Bridge note: SDK services are the SAME instances as the game's VContainer singletons —
    /// <c>PlaycenterSdkBootstrap</c> registers the live resolved objects into the SDK
    /// <c>ServiceRegistry</c> (<c>o.Services.AddSingleton&lt;T&gt;(instance)</c>), so modules and game
    /// systems share one object graph. Any port a game system needs that is NOT already in
    /// VContainer should be resolved here via <c>client.Services.Get&lt;T&gt;()</c> — this entry point
    /// is the single sanctioned bridge between the SDK service registry and game systems.
    /// </para>
    /// No BootSequence logic is recreated here — NTP, RC, force-update, and maintenance are owned
    /// by the SDK module pipeline that ran before this method is invoked.
    /// </summary>
    public sealed class RecipeRageGameEntry : IGameEntry
    {
        private readonly IAuthService _authService;
        private readonly ISessionLifecycle _sessionLifecycle;
        private readonly ISessionContext _sessionContext;
        private readonly IAppFlow _appFlow;
        private readonly IAnalyticsService _analytics;

        public RecipeRageGameEntry(
            IAuthService authService,
            ISessionLifecycle sessionLifecycle,
            ISessionContext sessionContext,
            IAppFlow appFlow,
            IAnalyticsService analytics = null)
        {
            _authService = authService;
            _sessionLifecycle = sessionLifecycle;
            _sessionContext = sessionContext;
            _appFlow = appFlow;
            _analytics = analytics;
        }

        public async Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            bool isAuthenticated = !string.IsNullOrEmpty(_authService.ProductUserId);

            if (!isAuthenticated)
            {
                GameLogger.LogInfo("[RecipeRageGameEntry] Not authenticated — entering Login side phase.");
                _appFlow.EnterSidePhase(FlowPhaseId.Login);
                return;
            }

            GameLogger.Log("[RecipeRageGameEntry] Auth OK — loading session.");
            var sessionLoader = new SessionLoader(_sessionLifecycle, _sessionContext);
            await sessionLoader.LoadAsync(ct);
            ct.ThrowIfCancellationRequested();

            _analytics?.LogEvent(AnalyticsEvents.LoginSuccess,
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { AnalyticsEvents.Params.Method, "existing_session" },
                    { AnalyticsEvents.Params.Phase, "sdk_ready" }
                });

            GameLogger.Log("[RecipeRageGameEntry] Session loaded — navigating Home.");
            _appFlow.ReturnHome();
        }

        public Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct)
        {
            _analytics?.LogEvent(AnalyticsEvents.BootGateOffline,
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { AnalyticsEvents.Params.Reason, failure?.Code.ToString() ?? "unknown" },
                    { AnalyticsEvents.Params.Phase, "sdk_boot" }
                });
            return Task.CompletedTask;
        }
    }
}
