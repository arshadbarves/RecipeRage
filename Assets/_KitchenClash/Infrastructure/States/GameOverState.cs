using KitchenClash.Application;
using KitchenClash.Application.State;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;

namespace KitchenClash.Infrastructure.States
{
    public class GameOverState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly IMatchContext _matchContext;
        private readonly IEconomyService _economyService;
        private readonly IEventBus _eventBus;

        public GameOverState(IUIService uiService, IMatchContext matchContext, IEconomyService economyService, IEventBus eventBus)
        {
            _uiService = uiService;
            _matchContext = matchContext;
            _economyService = economyService;
            _eventBus = eventBus;
        }

        public override void Enter()
        {
            base.Enter();
            _matchContext.Refresh();

            // Determine victory or defeat for music
            bool won = false;
            var resultSync = _matchContext.MatchResultSync;
            if (resultSync != null && resultSync.CurrentResult.HasResult)
            {
                won = !resultSync.CurrentResult.IsDraw && resultSync.CurrentResult.WinningTeamId == 0;
            }
            _eventBus?.Publish(new MusicEvent(won ? MusicTrack.Victory : MusicTrack.Defeat));
            _eventBus?.Publish(new SFXEvent(SFXType.MatchEnd));

            AwardMatchReward();
        }

        private void AwardMatchReward()
        {
            if (_economyService is not EconomyService economy) return;

            var resultSync = _matchContext.MatchResultSync;
            if (resultSync == null || !resultSync.CurrentResult.HasResult) return;

            var result = resultSync.CurrentResult;
            var scoreManager = _matchContext.ScoreManager;

            // Local player assumed team 0 — same convention as ResultsScreen
            int localTeamId = 0;
            bool won = !result.IsDraw && result.WinningTeamId == localTeamId;
            int score = scoreManager?.GetScore(localTeamId) ?? 0;

            economy.AwardMatchReward(won, score);
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}
