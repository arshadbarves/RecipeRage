using System;
using RecipeRage.Core.GameMode;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for active gameplay
    /// </summary>
    public class GameplayState : GameState
    {
        /// <summary>
        /// Event triggered when the game is paused or unpaused
        /// </summary>
        public event Action<bool> OnGamePaused;
        
        /// <summary>
        /// Event triggered when the game ends
        /// </summary>
        public event Action OnGameEnded;
        
        /// <summary>
        /// The current game mode
        /// </summary>
        public GameModeBase CurrentGameMode { get; private set; }
        
        /// <summary>
        /// Whether the game is paused
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// Initialize the list of allowed state transitions
        /// </summary>
        protected override void InitializeAllowedTransitions()
        {
            AllowTransitionTo<MainMenuState>();
            AllowTransitionTo<LoadingState>();
        }
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Reset gameplay state
            IsPaused = false;
            
            // Show gameplay UI
            Debug.Log("Showing gameplay UI");
            
            // TODO: Implement gameplay UI display
            
            // Start the game if a game mode is set
            if (CurrentGameMode != null)
            {
                StartGame();
            }
            else
            {
                Debug.LogError("No game mode set for gameplay state.");
            }
        }
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (IsPaused || CurrentGameMode == null)
            {
                return;
            }
            
            // Game-specific updates are handled by the game mode
        }
        
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            
            // End the game if it's still active
            if (CurrentGameMode != null && CurrentGameMode.IsGameActive)
            {
                EndGame();
            }
            
            // Hide gameplay UI
            Debug.Log("Hiding gameplay UI");
            
            // TODO: Implement gameplay UI hiding
        }
        
        /// <summary>
        /// Set the game mode for this gameplay session
        /// </summary>
        /// <param name="gameMode">The game mode to use</param>
        public void SetGameMode(GameModeBase gameMode)
        {
            if (gameMode == null)
            {
                Debug.LogError("Cannot set null game mode.");
                return;
            }
            
            CurrentGameMode = gameMode;
            
            // If we're already in the gameplay state, start the game
            if (IsActive && !CurrentGameMode.IsGameActive)
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// Start the game with the current game mode
        /// </summary>
        private void StartGame()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogError("Cannot start game: No game mode set.");
                return;
            }
            
            Debug.Log($"Starting game with mode: {CurrentGameMode.GetType().Name}");
            
            // Subscribe to game mode events
            CurrentGameMode.OnGameStart += OnGameModeStarted;
            CurrentGameMode.OnGameEnd += OnGameModeEnded;
            CurrentGameMode.OnGameStateChanged += OnGameModeStateChanged;
            
            // Start the game
            if (!CurrentGameMode.StartGame())
            {
                Debug.LogError("Failed to start game.");
            }
        }
        
        /// <summary>
        /// End the current game
        /// </summary>
        private void EndGame()
        {
            if (CurrentGameMode == null)
            {
                return;
            }
            
            Debug.Log("Ending game");
            
            // Unsubscribe from game mode events
            CurrentGameMode.OnGameStart -= OnGameModeStarted;
            CurrentGameMode.OnGameEnd -= OnGameModeEnded;
            CurrentGameMode.OnGameStateChanged -= OnGameModeStateChanged;
            
            // End the game
            if (CurrentGameMode.IsGameActive)
            {
                CurrentGameMode.EndGame();
            }
            
            // Notify listeners that the game has ended
            OnGameEnded?.Invoke();
        }
        
        /// <summary>
        /// Toggle the pause state of the game
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!IsPaused);
        }
        
        /// <summary>
        /// Set the pause state of the game
        /// </summary>
        /// <param name="paused">Whether the game should be paused</param>
        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
            {
                return;
            }
            
            IsPaused = paused;
            
            Debug.Log(IsPaused ? "Game paused" : "Game resumed");
            
            // Pause/unpause game systems
            Time.timeScale = IsPaused ? 0f : 1f;
            
            // Notify listeners of pause state change
            OnGamePaused?.Invoke(IsPaused);
            
            // TODO: Show/hide pause menu
        }
        
        #region Game Mode Event Handlers
        
        private void OnGameModeStarted()
        {
            Debug.Log("Game mode started");
        }
        
        private void OnGameModeEnded()
        {
            Debug.Log("Game mode ended");
            
            // Notify listeners that the game has ended
            OnGameEnded?.Invoke();
        }
        
        private void OnGameModeStateChanged(GameState newState)
        {
            Debug.Log($"Game mode state changed to: {newState}");
        }
        
        #endregion
    }
}
