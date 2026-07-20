using KitchenClash.Application.Config;
using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Server-side coordinator that wires the active mode's IModeWinCondition to
    /// ScoreChangedEvent and the round timer (Last Plate Standing).
    ///
    /// Instantiated by MatchRuntimeSceneBinder / MatchLifetimeScope, one per match.
    /// When a win condition is met it publishes MatchWonEvent and calls MatchEndController.
    /// </summary>
    public sealed class MatchWinConditionCoordinator : NetworkBehaviour
    {
        // ── Injected ──
        [Inject] private IEventBus      _eventBus;
        [Inject] private IConfigService _cfg;
        [Inject] private IMatchContext  _matchContext;

        // ── State ──
        private KitchenClash.Application.IModeWinCondition _winCondition;
        private string _activeModeId;
        private bool   _matchOver;

        // ── Score accumulators (for TugOfWar bar) ──
        private int _scoreA;
        private int _scoreB;

        // ─────────────────────────────────────────────────────────────────
        // Initialisation
        // ─────────────────────────────────────────────────────────────────

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            _eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            _eventBus.Subscribe<RoundTimerExpiredEvent>(OnRoundTimerExpired);
        }

        public override void OnNetworkDespawn()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
                _eventBus.Unsubscribe<RoundTimerExpiredEvent>(OnRoundTimerExpired);
            }
            base.OnNetworkDespawn();
        }

        /// <summary>Call once after match scope is ready with the active mode id.</summary>
        public void SetMode(string modeId)
        {
            _activeModeId = modeId;
            _winCondition = modeId switch
            {
                "rush_service"        => new KitchenClash.Application.TugOfWarWinCondition(_cfg),
                "hells_kitchen"       => new KitchenClash.Application.RaceToScoreWinCondition(_cfg),
                "last_plate_standing" => new KitchenClash.Application.BestOfRoundsWinCondition(_cfg),
                _                     => new KitchenClash.Application.RaceToScoreWinCondition(_cfg),
            };

            _winCondition.Reset();
            _scoreA = 0;
            _scoreB = 0;
            _matchOver = false;

            GameLogger.Log($"[WinCondition] Mode set to '{modeId}' → {_winCondition.GetType().Name}");
        }

        // ─────────────────────────────────────────────────────────────────
        // Score event handler (server-only)
        // ─────────────────────────────────────────────────────────────────

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            if (!IsServer || _matchOver || _winCondition == null) return;

            _scoreA = e.TeamAScore;
            _scoreB = e.TeamBScore;

            KitchenClash.Application.ModeWinResult result;

            if (_winCondition is KitchenClash.Application.TugOfWarWinCondition tugWin)
            {
                result = tugWin.EvaluateDelta(e.Team, e.Delta);
            }
            else if (_winCondition is KitchenClash.Application.BestOfRoundsWinCondition boRounds)
            {
                boRounds.RecordRoundDish(e.Team);
                result = boRounds.EvaluateRound();
            }
            else
            {
                result = _winCondition.Evaluate(_scoreA, _scoreB);
            }

            if (result.HasWinner)
                TriggerMatchWon(result.Winner);
        }

        // ─────────────────────────────────────────────────────────────────
        // Round timer expiry (Last Plate Standing only)
        // ─────────────────────────────────────────────────────────────────

        private void OnRoundTimerExpired(RoundTimerExpiredEvent e)
        {
            if (!IsServer || _matchOver) return;
            if (_winCondition is not KitchenClash.Application.BestOfRoundsWinCondition boRounds) return;

            var result = boRounds.EvaluateRoundExpiry();
            if (result.HasWinner)
                TriggerMatchWon(result.Winner);
        }

        // ─────────────────────────────────────────────────────────────────
        // Match won
        // ─────────────────────────────────────────────────────────────────

        private void TriggerMatchWon(TeamId winner)
        {
            if (_matchOver) return;
            _matchOver = true;

            GameLogger.Log($"[WinCondition] Match won by Team {winner}");
            // MatchEndController subscribes to MatchWonEvent and calls EndMatch.
            _eventBus?.Publish(new MatchWonEvent(winner));
        }
    }

    // ── Supporting events ──────────────────────────────────────────────

    public sealed class RoundTimerExpiredEvent
    {
        public int RoundNumber { get; }
        public RoundTimerExpiredEvent(int round) => RoundNumber = round;
    }

    public sealed class MatchWonEvent
    {
        public TeamId WinningTeam { get; }
        public MatchWonEvent(TeamId team) => WinningTeam = team;
    }
}
