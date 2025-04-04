using System;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for active gameplay.
    /// </summary>
    public class GameplayState : GameState
    {
        /// <summary>
        /// Event triggered when the game ends.
        /// </summary>
        public event Action OnGameEnded;
        
        /// <summary>
        /// The current game mode.
        /// </summary>
        private object _gameMode; // Will be replaced with actual GameModeBase type
        
        /// <summary>
        /// Flag to track if the game is active.
        /// </summary>
        private bool _isGameActive;
        
        /// <summary>
        /// Game timer in seconds.
        /// </summary>
        private float _gameTimer;
        
        /// <summary>
        /// Game duration in seconds.
        /// </summary>
        private float _gameDuration = 300f; // 5 minutes default
        
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Check if game mode is set
            if (_gameMode == null)
            {
                Debug.LogError("[GameplayState] Game mode not set. Cannot start gameplay.");
                EndGame();
                return;
            }
            
            // Reset game state
            _isGameActive = true;
            _gameTimer = 0f;
            
            // Start the game
            Debug.Log("[GameplayState] Starting gameplay");
            
            // Show gameplay UI
            // TODO: Implement gameplay UI activation
            
            // Initialize game systems
            // TODO: Implement game systems initialization
        }
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            
            // End the game if still active
            if (_isGameActive)
            {
                _isGameActive = false;
            }
            
            // Hide gameplay UI
            // TODO: Implement gameplay UI deactivation
            
            // Clean up game systems
            // TODO: Implement game systems cleanup
        }
        
        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // If game is not active, do nothing
            if (!_isGameActive)
            {
                return;
            }
            
            // Update game timer
            _gameTimer += Time.deltaTime;
            
            // Check for game end condition (time limit)
            if (_gameTimer >= _gameDuration)
            {
                Debug.Log("[GameplayState] Game time limit reached");
                EndGame();
                return;
            }
            
            // Update game systems
            // TODO: Implement game systems update
        }
        
        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // If game is not active, do nothing
            if (!_isGameActive)
            {
                return;
            }
            
            // Update physics-based game systems
            // TODO: Implement physics-based game systems update
        }
        
        /// <summary>
        /// Sets the game mode for this gameplay session.
        /// </summary>
        /// <param name="gameMode">The game mode to use</param>
        public void SetGameMode(object gameMode) // Will be replaced with actual GameModeBase type
        {
            _gameMode = gameMode;
            Debug.Log($"[GameplayState] Game mode set: {_gameMode}");
            
            // Set game duration based on game mode
            // TODO: Get game duration from game mode
        }
        
        /// <summary>
        /// Ends the current game.
        /// </summary>
        public void EndGame()
        {
            if (!_isGameActive)
            {
                return;
            }
            
            _isGameActive = false;
            Debug.Log("[GameplayState] Game ended");
            
            // Show game end UI
            // TODO: Implement game end UI activation
            
            // Trigger the game ended event
            OnGameEnded?.Invoke();
        }
        
        /// <summary>
        /// Gets the remaining game time in seconds.
        /// </summary>
        /// <returns>Remaining game time in seconds</returns>
        public float GetRemainingTime()
        {
            return Mathf.Max(0, _gameDuration - _gameTimer);
        }
        
        /// <summary>
        /// Gets the elapsed game time in seconds.
        /// </summary>
        /// <returns>Elapsed game time in seconds</returns>
        public float GetElapsedTime()
        {
            return _gameTimer;
        }
    }
}
