using System;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Implementation of the state machine that manages game states
    /// </summary>
    public class StateMachine : IStateMachine
    {
        /// <summary>
        /// Event triggered when a state transition occurs
        /// </summary>
        public event Action<IState, IState> OnStateChanged;
        
        /// <summary>
        /// The current active state
        /// </summary>
        public IState CurrentState { get; private set; }
        
        /// <summary>
        /// The previous state before the current one
        /// </summary>
        public IState PreviousState { get; private set; }
        
        /// <summary>
        /// Whether the state machine has been initialized
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Initialize the state machine with an initial state
        /// </summary>
        /// <param name="initialState">The initial state</param>
        public void Initialize(IState initialState)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("StateMachine is already initialized.");
                return;
            }
            
            CurrentState = initialState;
            _isInitialized = true;
            
            // Enter the initial state
            CurrentState.Enter();
        }
        
        /// <summary>
        /// Change to a new state if the transition is valid
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>True if the transition was successful</returns>
        public bool ChangeState(IState newState)
        {
            if (!_isInitialized)
            {
                Debug.LogError("StateMachine is not initialized. Call Initialize first.");
                return false;
            }
            
            if (newState == null)
            {
                Debug.LogError("Cannot change to a null state.");
                return false;
            }
            
            if (CurrentState == newState)
            {
                Debug.LogWarning("Attempting to change to the same state.");
                return false;
            }
            
            // Check if the current state allows the transition
            if (CurrentState != null && !CurrentState.CanTransitionTo(newState))
            {
                Debug.LogWarning($"Current state {CurrentState.GetType().Name} does not allow transition to {newState.GetType().Name}.");
                return false;
            }
            
            // Exit the current state
            if (CurrentState != null)
            {
                CurrentState.Exit();
            }
            
            // Store the previous state
            PreviousState = CurrentState;
            
            // Set and enter the new state
            CurrentState = newState;
            CurrentState.Enter();
            
            // Trigger the state changed event
            OnStateChanged?.Invoke(PreviousState, CurrentState);
            
            return true;
        }
        
        /// <summary>
        /// Update the current state
        /// </summary>
        public void Update()
        {
            if (!_isInitialized || CurrentState == null)
            {
                return;
            }
            
            CurrentState.Update();
        }
    }
}
