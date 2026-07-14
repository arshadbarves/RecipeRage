using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Match-end: victory/defeat audio + economy reward.
    /// </summary>
    public sealed class ResultsPhase
    {
        private readonly IMatchContext _matchContext;
        private readonly IEconomyService _economyService;
        private readonly IEventBus _eventBus;

        public ResultsPhase(
            IEventBus eventBus,
            IEconomyService economyService = null,
            IMatchContext matchContext = null)
        {
            _matchContext = matchContext;
            _economyService = economyService;
            _eventBus = eventBus;
        }

        public void Enter()
        {
            _matchContext?.Refresh();

            bool won = false;
            MatchResultSync resultSync = _matchContext?.MatchResultSync;
            if (resultSync != null && resultSync.CurrentResult.HasResult)
            {
                won = !resultSync.CurrentResult.IsDraw && resultSync.CurrentResult.WinningTeamId == 0;
            }

            _eventBus?.Publish(new MusicEvent(won ? MusicTrack.Victory : MusicTrack.Defeat));
            _eventBus?.Publish(new SFXEvent(SFXType.MatchEnd));

            AwardMatchReward();
        }

        public void Exit()
        {
        }

        private void AwardMatchReward()
        {
            if (_economyService is not EconomyService economy)
            {
                return;
            }

            MatchResultSync resultSync = _matchContext?.MatchResultSync;
            if (resultSync == null || !resultSync.CurrentResult.HasResult)
            {
                return;
            }

            MatchResultState result = resultSync.CurrentResult;
            ScoreManager scoreManager = _matchContext?.ScoreManager;

            int localTeamId = 0;
            bool won = !result.IsDraw && result.WinningTeamId == localTeamId;
            int score = scoreManager?.GetScore(localTeamId) ?? 0;

            economy.AwardMatchReward(won, score);
        }
    }
}
