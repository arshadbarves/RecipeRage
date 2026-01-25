using Gameplay.Cooking;
using Gameplay.Scoring;
using Gameplay.Stations;
using Core.Logging;
using UnityEngine;

namespace Gameplay.GameModes.Logic
{
    /// <summary>
    /// Competitive "Kitchen Wars" game mode implementation.
    /// Teams compete to complete orders. Includes rage mechanics.
    /// </summary>
    public class CompetitiveGameModeLogic : IGameModeLogic
    {
        private GameMode _config;
        private OrderManager _orderManager;
        private ScoreManager _scoreManager;
        
        private GamePhase _currentPhase = GamePhase.Waiting;
        private float _phaseStartTime;
        private float _phaseDuration;

        // Competitive settings
        private const float WARMUP_DURATION = 10f;
        private const float OVERTIME_DURATION = 30f; // Optional overtime if scores are close?

        public GamePhase CurrentPhase => _currentPhase;
        public float TimeRemaining => Mathf.Max(0f, _phaseDuration - (Time.time - _phaseStartTime));

        public void Initialize(GameMode config, OrderManager orderManager, ScoreManager scoreManager)
        {
            _config = config;
            _orderManager = orderManager;
            _scoreManager = scoreManager;

            GameLogger.Log($"CompetitiveGameModeLogic initialized: {_config.DisplayName}");
        }

        public void OnMatchStart()
        {
            GameLogger.Log("Competitive match starting!");
            // Reset scores for all teams
            if (_scoreManager != null)
            {
                _scoreManager.ResetScores();
            }

            StartWarmupPhase();
        }

        public void OnMatchEnd()
        {
            GameLogger.Log("Competitive match ending");
            _currentPhase = GamePhase.GameOver;
            
            if (_orderManager != null)
            {
                _orderManager.StopGeneratingOrders();
            }
            
            DetermineWinner();
        }

        public void Update(float deltaTime)
        {
            if (_phaseDuration > 0 && TimeRemaining <= 0)
            {
                HandlePhaseTimeout();
            }
        }

        private void StartWarmupPhase()
        {
            _currentPhase = GamePhase.Warmup;
            _phaseStartTime = Time.time;
            _phaseDuration = WARMUP_DURATION;
            GameLogger.Log($"Warmup phase: {WARMUP_DURATION}s");
        }

        private void StartPlayingPhase()
        {
            _currentPhase = GamePhase.Playing;
            _phaseStartTime = Time.time;
            _phaseDuration = _config.HasTimeLimit ? _config.GameTime : 300f; // Default 5 mins

            if (_orderManager != null)
            {
                _orderManager.StartGeneratingOrders();
                // Specifically for Kitchen Wars, we might want shared orders or team-specific
            }

            // Register for events (Trash Attack, etc.)
            if (_scoreManager != null)
            {
                _scoreManager.OnTeamComboAchieved += OnTeamCombo; 
            }

            GameLogger.Log($"Playing phase: {_phaseDuration}s");
        }

        private void HandlePhaseTimeout()
        {
            switch (_currentPhase)
            {
                case GamePhase.Warmup:
                    StartPlayingPhase();
                    break;
                case GamePhase.Playing:
                    OnMatchEnd();
                    break;
            }
        }

        private void DetermineWinner()
        {
            if (_scoreManager == null) return;

            int team0Score = _scoreManager.GetScore(0);
            int team1Score = _scoreManager.GetScore(1);

            GameLogger.Log($"Final Scores - Team 0: {team0Score}, Team 1: {team1Score}");

            if (team0Score > team1Score)
            {
                GameLogger.Log("Team 0 Wins!");
                // Trigger win sequence
            }
            else if (team1Score > team0Score)
            {
                GameLogger.Log("Team 1 Wins!");
            }
            else
            {
                GameLogger.Log("Draw!");
                // Maybe trigger Overtime here?
            }
        }

        // Cache sinks
        private SinkStation[] _teamSinks;

        private void InitializeSinks()
        {
            _teamSinks = new SinkStation[2]; // Max 2 teams
            var sinks = UnityEngine.Object.FindObjectsByType<SinkStation>(FindObjectsSortMode.None);
            foreach (var sink in sinks)
            {
                if (sink.TeamId >= 0 && sink.TeamId < 2)
                {
                    _teamSinks[sink.TeamId] = sink;
                }
            }
        }

        // --- Rage Mechanics Handlers ---

        private void OnTeamCombo(int teamId, int comboCount)
        {
            // "Trash Attack": High combo sends dirty dishes to enemy
            // Enemy team ID assumption: 2 teams, 0 and 1. Enemy is 1 - teamId.
            int enemyTeamId = 1 - teamId;
            
            if (_teamSinks == null) InitializeSinks();

            if (enemyTeamId >= 0 && enemyTeamId < _teamSinks.Length && _teamSinks[enemyTeamId] != null)
            {
                // Send 1 dirty dish per combo level? Or just 1?
                // Rules say "Every time Team A delivers a perfect dish"
                // Let's use Combo for now as proxy, or ScoreManager Perfect Dish event
                // If using direct combo:
                _teamSinks[enemyTeamId].AddDirtyPlatesServerRpc(1);
                
                GameLogger.Log($"Trash Attack! Team {teamId} sent dirty dishes to Team {enemyTeamId}");
            }
        }
    }
}
