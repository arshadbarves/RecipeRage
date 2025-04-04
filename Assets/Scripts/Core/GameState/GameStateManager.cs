using System.Collections.Generic;
using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Core.GameState
{
    /// <summary>
    /// Manages game states and transitions between them
    /// </summary>
    public class GameStateManager : IGameStateManager
    {
        private readonly Dictionary<GameStateType, IGameState> _states = new Dictionary<GameStateType, IGameState>();
        private readonly Stack<IGameState> _stateStack = new Stack<IGameState>();
        
        /// <summary>
        /// The current active game state
        /// </summary>
        public IGameState CurrentState { get; private set; }
        
        /// <summary>
        /// The previous game state
        /// </summary>
        public IGameState PreviousState { get; private set; }
        
        /// <summary>
        /// Initialize the game state manager
        /// </summary>
        public void Initialize()
        {
            LogHelper.Info("GameStateManager", "Initializing game state manager");
            
            // Register default states
            RegisterDefaultStates();
            
            // Start with the splash screen state
            ChangeState(GameStateType.Splash);
        }
        
        /// <summary>
        /// Change to a new game state
        /// </summary>
        /// <param name="newStateType">The type of the new state</param>
        public void ChangeState(GameStateType newStateType)
        {
            if (!_states.TryGetValue(newStateType, out IGameState newState))
            {
                LogHelper.Error("GameStateManager", $"State {newStateType} not found");
                return;
            }
            
            // Exit current state if it exists
            if (CurrentState != null)
            {
                LogHelper.Info("GameStateManager", $"Exiting state {CurrentState.StateType}");
                CurrentState.Exit();
            }
            
            // Store previous state
            PreviousState = CurrentState;
            
            // Enter new state
            CurrentState = newState;
            LogHelper.Info("GameStateManager", $"Entering state {CurrentState.StateType}");
            CurrentState.Enter();
        }
        
        /// <summary>
        /// Register a new game state
        /// </summary>
        /// <param name="state">The state to register</param>
        public void RegisterState(IGameState state)
        {
            if (state == null)
            {
                LogHelper.Error("GameStateManager", "Cannot register null state");
                return;
            }
            
            if (_states.ContainsKey(state.StateType))
            {
                LogHelper.Warning("GameStateManager", $"State {state.StateType} already registered. Overwriting.");
                _states[state.StateType] = state;
            }
            else
            {
                _states.Add(state.StateType, state);
                LogHelper.Debug("GameStateManager", $"State {state.StateType} registered");
            }
        }
        
        /// <summary>
        /// Pause the current state and push it onto the stack
        /// </summary>
        /// <param name="newStateType">The type of the new state to enter</param>
        public void PushState(GameStateType newStateType)
        {
            if (!_states.TryGetValue(newStateType, out IGameState newState))
            {
                LogHelper.Error("GameStateManager", $"State {newStateType} not found");
                return;
            }
            
            // Pause current state if it exists
            if (CurrentState != null)
            {
                LogHelper.Info("GameStateManager", $"Pausing state {CurrentState.StateType}");
                CurrentState.Pause();
                _stateStack.Push(CurrentState);
            }
            
            // Enter new state
            CurrentState = newState;
            LogHelper.Info("GameStateManager", $"Entering state {CurrentState.StateType}");
            CurrentState.Enter();
        }
        
        /// <summary>
        /// Pop the current state and resume the previous state
        /// </summary>
        public void PopState()
        {
            if (_stateStack.Count == 0)
            {
                LogHelper.Warning("GameStateManager", "Cannot pop state: stack is empty");
                return;
            }
            
            // Exit current state
            if (CurrentState != null)
            {
                LogHelper.Info("GameStateManager", $"Exiting state {CurrentState.StateType}");
                CurrentState.Exit();
            }
            
            // Resume previous state
            CurrentState = _stateStack.Pop();
            LogHelper.Info("GameStateManager", $"Resuming state {CurrentState.StateType}");
            CurrentState.Resume();
        }
        
        /// <summary>
        /// Update the current state
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }
        
        /// <summary>
        /// Fixed update the current state
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
        
        /// <summary>
        /// Register default states
        /// </summary>
        private void RegisterDefaultStates()
        {
            // TODO: Register concrete state implementations
            // This will be implemented as we create the state classes
            
            LogHelper.Info("GameStateManager", "Default states registered");
        }
    }
}
