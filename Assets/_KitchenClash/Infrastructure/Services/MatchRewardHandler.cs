using System.Collections.Generic;
using KitchenClash.Application;
using KitchenClash.Application.Config;
using KitchenClash.Domain;
using Playcenter.Services;
using Playcenter.Shell;
using UnityEngine;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>
    /// Session-scoped handler that awards economy rewards when a match ends.
    ///
    /// Depends on IWalletLedger (session-scoped via EconomyService) so that
    /// the reward path goes through the Playcenter wallet contract.
    /// Keeps GameOverState free of session-scoped dependencies.
    /// </summary>
    public sealed class MatchRewardHandler : IInitializable, System.IDisposable
    {
        private readonly IWalletLedger _ledger;
        private readonly IEventBus _eventBus;
        private readonly IAnalyticsService _analytics;

        public MatchRewardHandler(
            IWalletLedger ledger,
            IEventBus eventBus,
            IAnalyticsService analytics = null)
        {
            _ledger = ledger;
            _eventBus = eventBus;
            _analytics = analytics;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<MatchEndedEvent>(OnMatchEnded);
        }

        private void OnMatchEnded(MatchEndedEvent evt)
        {
            string reason = evt.Won ? "match_win" : "match_loss";
            int reward = evt.Won
                ? EconomyService.MatchWinReward + Mathf.FloorToInt(evt.LocalTeamScore * EconomyService.ScoreBonusCoinRate)
                : EconomyService.MatchLossReward;
            _ledger.Credit(CurrencyId.Coins, reward, reason);
            _analytics?.LogEvent(AnalyticsEvents.WalletCredit, new Dictionary<string, object>
            {
                { AnalyticsEvents.Params.Amount, reward },
                { AnalyticsEvents.Params.Currency, CurrencyId.Coins.ToString() },
                { AnalyticsEvents.Params.Reason, reason },
                { AnalyticsEvents.Params.Won, evt.Won },
                { AnalyticsEvents.Params.Score, evt.LocalTeamScore }
            });
            _eventBus.Publish(new MatchRewardEvent { CoinsAwarded = reward, Won = evt.Won, Score = evt.LocalTeamScore });
        }
    }
}
