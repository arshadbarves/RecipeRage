using System;
using RecipeRage.Core.GameFramework.State.States;
using RecipeRage.Core.Patterns;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Manages game states and transitions between them
    /// </summary>
    public class GameStateManager : MonoBehaviourSingleton<GameStateManager>
    {
        /// <summary>
        /// Event triggered when a state transition occurs
        /// </summary>
        public event Action<IState, IState> OnStateChanged;
        
        /// <summary>
        /// The state machine that manages game states
        /// </summary>
        private IStateMachine _stateMachine;
        
        /// <summary>
        /// Cached instances of game states
        /// </summary>
        private LoadingState _loadingState;
        private MainMenuState _mainMenuState;
        private MatchmakingState _matchmakingState;
        private GameplayState _gameplayState;
        
        /// <summary>
        /// The current active state
        /// </summary>
        public IState CurrentState => _stateMachine?.CurrentState;
        
        /// <summary>
        /// The previous state before the current one
        /// </summary>
        public IState PreviousState => _stateMachine?.PreviousState;
        
        /// <summary>
        /// Initialize the game state manager
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Create the state machine
            _stateMachine = new StateMachine();
            
            // Create state instances
            _loadingState = new LoadingState();
            _mainMenuState = new MainMenuState();
            _matchmakingState = new MatchmakingState();
            _gameplayState = new GameplayState();
            
            // Subscribe to state machine events
            _stateMachine.OnStateChanged += HandleStateChanged;
            
            // Subscribe to specific state events
            _loadingState.OnLoadingComplete += HandleLoadingComplete;
            _matchmakingState.OnMatchmakingComplete += HandleMatchmakingComplete;
            _gameplayState.OnGameEnded += HandleGameEnded;
        }
        
        /// <summary>
        /// Initialize the state machine with the loading state
        /// </summary>
        private void Start()
        {
            // Initialize the state machine with the loading state
            _stateMachine.Initialize(_loadingState);
        }
        
        /// <summary>
        /// Update the current state
        /// </summary>
        private void Update()
        {
            // Update the state machine
            _stateMachine?.Update();
        }
        
        /// <summary>
        /// Clean up when the object is destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Unsubscribe from state machine events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
            
            // Unsubscribe from specific state events
            if (_loadingState != null)
            {
                _loadingState.OnLoadingComplete -= HandleLoadingComplete;
            }
            
            if (_matchmakingState != null)
            {
                _matchmakingState.OnMatchmakingComplete -= HandleMatchmakingComplete;
            }
            
            if (_gameplayState != null)
            {
                _gameplayState.OnGameEnded -= HandleGameEnded;
            }
        }
        
        #region State Transitions
        
        /// <summary>
        /// Change to the loading state
        /// </summary>
        public void ChangeToLoadingState()
        {
            _stateMachine.ChangeState(_loadingState);
        }
        
        /// <summary>
        /// Change to the main menu state
        /// </summary>
        public void ChangeToMainMenuState()
        {
            _stateMachine.ChangeState(_mainMenuState);
        }
        
        /// <summary>
        /// Change to the matchmaking state
        /// </summary>
        public void ChangeToMatchmakingState()
        {
            _stateMachine.ChangeState(_matchmakingState);
        }
        
        /// <summary>
        /// Change to the gameplay state
        /// </summary>
        /// <param name="gameMode">The game mode to use</param>
        public void ChangeToGameplayState(GameMode.GameModeBase gameMode)
        {
            // Set the game mode before changing state
            _gameplayState.SetGameMode(gameMode);
            _stateMachine.ChangeState(_gameplayState);
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle state machine state changed event
        /// </summary>
        /// <param name="previousState">The previous state</param>
        /// <param name="currentState">The current state</param>
        private void HandleStateChanged(IState previousState, IState currentState)
        {
            Debug.Log($"State changed from {(previousState != null ? previousState.GetType().Name : "null")} to {currentState.GetType().Name}");
            
            // Forward the event
            OnStateChanged?.Invoke(previousState, currentState);
        }
        
        /// <summary>
        /// Handle loading complete event
        /// </summary>
        private void HandleLoadingComplete()
        {
            Debug.Log("Loading complete, transitioning to main menu");
            
            // Transition to main menu when loading is complete
            ChangeToMainMenuState();
        }
        
        /// <summary>
        /// Handle matchmaking complete event
        /// </summary>
        /// <param name="success">Whether matchmaking was successful</param>
        private void HandleMatchmakingComplete(bool success)
        {
            if (success)
            {
                Debug.Log("Matchmaking successful, transitioning to gameplay");
                
                // Transition to loading state before gameplay
                ChangeToLoadingState();
                
                // TODO: Set up game mode based on matchmaking result
                // This would be done after loading is complete
            }
            else
            {
                Debug.Log("Matchmaking canceled, returning to main menu");
                
                // Return to main menu if matchmaking was canceled
                ChangeToMainMenuState();
            }
        }
        
        /// <summary>
        /// Handle game ended event
        /// </summary>
        private void HandleGameEnded()
        {
            Debug.Log("Game ended, returning to main menu");
            
            // Return to main menu when the game ends
            ChangeToMainMenuState();
        }
        
        #endregion
    }
}
