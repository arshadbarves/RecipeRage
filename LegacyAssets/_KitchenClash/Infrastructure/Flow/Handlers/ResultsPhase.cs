using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Match-end presentation: victory/defeat audio + publish <see cref="MatchEndedEvent"/>.
    /// Wallet credit is SESSION-only via <c>MatchRewardHandler</c> / <c>IWalletLedger</c> —
    /// this phase never mutates economy.
    /// </summary>
    public sealed class ResultsPhase
    {
        private readonly IMatchHudPort _matchHud;
        private readonly IEventBus _eventBus;
        private bool _publishedMatchEnded;

        public ResultsPhase(IEventBus eventBus, IMatchHudPort matchHud = null)
        {
            _eventBus = eventBus;
            _matchHud = matchHud;
        }

        public void Enter(MatchResultInfo result = null)
        {
            _matchHud?.Refresh();

            bool won = false;
            int score = 0;
            ResolveOutcome(result, out won, out score);

            _eventBus?.Publish(new MusicEvent(won ? MusicTrack.Victory : MusicTrack.Defeat));
            _eventBus?.Publish(new SFXEvent(SFXType.MatchEnd));

            // Once per results entry — SESSION MatchRewardHandler is the sole credit path.
            if (!_publishedMatchEnded && _eventBus != null)
            {
                _publishedMatchEnded = true;
                _eventBus.Publish(new MatchEndedEvent(won, score));
            }
        }

        public void Exit()
        {
            _publishedMatchEnded = false;
        }

        private void ResolveOutcome(MatchResultInfo result, out bool won, out int score)
        {
            if (result != null)
            {
                won = result.Won;
                score = result.LocalTeamScore;
                return;
            }

            if (_matchHud != null && _matchHud.HasMatchResult)
            {
                MatchResultSnapshot snap = _matchHud.CurrentMatchResult;
                int localTeamId = _matchHud.LocalTeamId;
                won = !snap.IsDraw && snap.WinningTeamId == localTeamId;
                score = _matchHud.GetTeamScore(localTeamId);
                return;
            }

            won = false;
            score = 0;
        }
    }
}
