using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Application.Services;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Server-authoritative gameplay runtime owner for round start and match end.
    /// </summary>
    public class MatchEndController : NetworkBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private RoundTimer _roundTimer;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private GamePhaseSync _gamePhaseSync;
        [SerializeField] private MatchResultSync _matchResultSync;

        [Inject] private IMatchContext _matchContext;
        [Inject] private ISessionContext _sessionContext;

        private Gameplay.GameModes.GameMode _selectedGameMode;
        private bool _roundStarted;
        private bool _matchEnded;

        private void Awake()
        {
            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }

            ResolveDependencies();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResolveDependencies();

            if (!IsServer)
            {
                return;
            }

            SubscribeToEvents();
            StartRound();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                UnsubscribeFromEvents();
            }

            base.OnNetworkDespawn();
        }

        private void ResolveDependencies()
        {
            _matchContext?.Refresh();
            _roundTimer ??= GetComponent<RoundTimer>() ?? _matchContext?.RoundTimer;
            _scoreManager ??= GetComponent<ScoreManager>() ?? _matchContext?.ScoreManager;
            _gamePhaseSync ??= GetComponent<GamePhaseSync>() ?? _matchContext?.GamePhaseSync;
            _matchResultSync ??= GetComponent<MatchResultSync>() ?? _matchContext?.MatchResultSync;
        }

        private void SubscribeToEvents()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnTeamScoreChanged -= HandleTeamScoreChanged;
                _scoreManager.OnTeamScoreChanged += HandleTeamScoreChanged;
            }

            if (_roundTimer != null)
            {
                _roundTimer.OnTimerExpired -= HandleTimerExpired;
                _roundTimer.OnTimerExpired += HandleTimerExpired;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnTeamScoreChanged -= HandleTeamScoreChanged;
            }

            if (_roundTimer != null)
            {
                _roundTimer.OnTimerExpired -= HandleTimerExpired;
            }
        }

        private void StartRound()
        {
            if (_roundStarted)
            {
                return;
            }

            _selectedGameMode = _sessionContext?.GameModeService?.SelectedGameMode;
            if (_selectedGameMode == null)
            {
                GameLogger.LogError("[MatchEndController] Selected game mode is missing. Match end flow will stay inactive.");
                return;
            }

            _roundStarted = true;
            _matchEnded = false;
            _matchResultSync?.ClearResult();

            if (_gamePhaseSync != null)
            {
                _gamePhaseSync.SetPhase(Gameplay.GameModes.GamePhase.Playing);
            }
            else
            {
                GameLogger.LogError("[MatchEndController] GamePhaseSync not found.");
            }

            if (_selectedGameMode.HasTimeLimit)
            {
                if (_roundTimer != null)
                {
                    _roundTimer.StartTimer(_selectedGameMode.GameTime);
                }
                else
                {
                    GameLogger.LogError("[MatchEndController] RoundTimer not found.");
                }
            }
            else if (_roundTimer != null)
            {
                _roundTimer.StopTimer();
            }

            GameLogger.Log($"[MatchEndController] Round started for mode '{_selectedGameMode.Id}'");
        }

        private void HandleTeamScoreChanged(int teamId, int score)
        {
            if (!_roundStarted || _matchEnded || _selectedGameMode == null || _scoreManager == null)
            {
                return;
            }

            Gameplay.GameModes.MatchEndEvaluation evaluation = Gameplay.GameModes.MatchEndEvaluator.EvaluateScoreLimit(
                _scoreManager.GetTeamScores(),
                _selectedGameMode.HasScoreLimit,
                _selectedGameMode.TargetScore);

            if (!evaluation.ShouldEnd)
            {
                return;
            }

            EndMatch(Gameplay.GameModes.MatchEndReason.ScoreLimitReached, evaluation);
        }

        private void HandleTimerExpired()
        {
            if (!_roundStarted || _matchEnded)
            {
                return;
            }

            Gameplay.GameModes.MatchEndEvaluation evaluation = Gameplay.GameModes.MatchEndEvaluator.EvaluateFinalScores(_scoreManager?.GetTeamScores());
            EndMatch(Gameplay.GameModes.MatchEndReason.TimerExpired, evaluation);
        }

        private void EndMatch(Gameplay.GameModes.MatchEndReason reason, Gameplay.GameModes.MatchEndEvaluation evaluation)
        {
            if (_matchEnded)
            {
                return;
            }

            _matchEnded = true;

            if (_roundTimer != null)
            {
                _roundTimer.StopTimer();
            }

            if (_matchResultSync != null)
            {
                _matchResultSync.SetResult(Gameplay.GameModes.MatchResultState.FromEvaluation(reason, evaluation));
            }
            else
            {
                GameLogger.LogError("[MatchEndController] MatchResultSync not found.");
            }

            if (_gamePhaseSync != null)
            {
                _gamePhaseSync.SetPhase(Gameplay.GameModes.GamePhase.GameOver);
            }

            if (evaluation.IsDraw)
            {
                GameLogger.Log($"[MatchEndController] Match ended: draw at {evaluation.WinningScore} points.");
            }
            else
            {
                GameLogger.Log($"[MatchEndController] Match ended: Team {evaluation.WinningTeamId} won with {evaluation.WinningScore} points.");
            }
        }
    }
}
