using Gameplay.Cooking;
using Gameplay.Scoring;
using Core.Logging;
using UnityEngine;

namespace Gameplay.GameModes.Logic
{
    /// <summary>
    /// Classic game mode implementation.
    /// Time-based cooking competition with score goal.
    /// </summary>
    public class ClassicGameModeLogic : IGameModeLogic
    {
        private GameMode _config;
        private OrderManager _orderManager;
        private ScoreManager _scoreManager;
        private GamePhase _currentPhase = GamePhase.Waiting;
        private float _phaseStartTime;
        private float _phaseDuration;

        public GamePhase CurrentPhase => _currentPhase;
        public float TimeRemaining => Mathf.Max(0f, _phaseDuration - (Time.time - _phaseStartTime));

        public void Initialize(GameMode config, OrderManager orderManager, ScoreManager scoreManager)
        {
            _config = config;
            _orderManager = orderManager;
            _scoreManager = scoreManager;



            // if (_scoreManager != null)
            // {
            //     _scoreManager.SetTargetScore(_config.TargetScore);
            // }

            GameLogger.Log($"ClassicGameModeLogic initialized: {_config.DisplayName}");
        }

        public void OnMatchStart()
        {
            GameLogger.Log("Classic match starting with warmup phase");
            StartWarmupPhase(10f); // 10 second warmup
        }

        public void OnMatchEnd()
        {
            GameLogger.Log("Classic match ending");
            _currentPhase = GamePhase.GameOver;
            
            if (_orderManager != null)
            {
                _orderManager.StopGeneratingOrders();
            }
        }

        public void Update(float deltaTime)
        {
            // Check for phase transitions
            if (_phaseDuration > 0 && TimeRemaining <= 0)
            {
                HandlePhaseTimeout();
            }

            // Check win conditions during Playing phase
            if (_currentPhase == GamePhase.Playing && _scoreManager != null)
            {
                if (_config.HasScoreLimit && CheckScoreWinCondition())
                {
                    GameLogger.Log("Score limit reached!");
                    OnMatchEnd();
                }
            }
        }

        private void StartWarmupPhase(float duration)
        {
            _currentPhase = GamePhase.Warmup;
            _phaseStartTime = Time.time;
            _phaseDuration = duration;
            GameLogger.Log($"Warmup phase started: {duration}s");
        }

        private void StartPlayingPhase()
        {
            _currentPhase = GamePhase.Playing;
            _phaseStartTime = Time.time;
            _phaseDuration = _config.HasTimeLimit ? _config.GameTime : 0f;

            if (_orderManager != null)
            {
                _orderManager.StartGeneratingOrders();
            }

            GameLogger.Log($"Playing phase started: {_phaseDuration}s");
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

        private bool CheckScoreWinCondition()
        {
            // TODO: Implement when ScoreManager.GetAllTeamScores() is available
            // Check if any team reached target score
            // var teams = _scoreManager.GetAllTeamScores();
            // foreach (var score in teams.Values)
            // {
            //     if (score >= _config.TargetScore)
            //     {
            //         return true;
            //     }
            // }
            return false;
        }
    }
}
