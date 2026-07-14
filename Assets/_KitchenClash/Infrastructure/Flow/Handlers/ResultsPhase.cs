using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Match-end: victory/defeat audio + economy reward.
    /// Depends only on Application ports (IMatchHudPort / IEconomyService) —
    /// never on Network MonoBehaviours.
    /// </summary>
    public sealed class ResultsPhase
    {
        private readonly IMatchHudPort _matchHud;
        private readonly IEconomyService _economyService;
        private readonly IEventBus _eventBus;

        public ResultsPhase(
            IEventBus eventBus,
            IEconomyService economyService = null,
            IMatchHudPort matchHud = null)
        {
            _matchHud = matchHud;
            _economyService = economyService;
            _eventBus = eventBus;
        }

        public void Enter()
        {
            _matchHud?.Refresh();

            bool won = false;
            if (_matchHud != null && _matchHud.HasMatchResult)
            {
                MatchResultSnapshot result = _matchHud.CurrentMatchResult;
                won = !result.IsDraw && result.WinningTeamId == _matchHud.LocalTeamId;
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
            if (_economyService == null || _matchHud == null || !_matchHud.HasMatchResult)
            {
                return;
            }

            MatchResultSnapshot result = _matchHud.CurrentMatchResult;
            int localTeamId = _matchHud.LocalTeamId;
            bool won = !result.IsDraw && result.WinningTeamId == localTeamId;
            int score = _matchHud.GetTeamScore(localTeamId);

            _economyService.AwardMatchReward(won, score);
        }
    }
}
