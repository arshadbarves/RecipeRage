using System;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Implementation of a state machine that manages game states.
    /// </summary>
    public class StateMachine : IStateMachine
    {
        /// <summary>
        /// Event triggered when the state changes.
        /// </summary>
        public event Action<IState, IState> OnStateChanged;
        
        /// <summary>
        /// The current active state.
        /// </summary>
        public IState CurrentState { get; private set; }
        
        /// <summary>
        /// The previous state before the current one.
        /// </summary>
        public IState PreviousState { get; private set; }
        
        /// <summary>
        /// Flag to track if the state machine has been initialized.
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Initializes the state machine with an initial state.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        public void Initialize(IState initialState)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[StateMachine] State machine already initialized.");
                return;
            }
            
            CurrentState = initialState;
            _isInitialized = true;
            
            // Enter the initial state
            CurrentState?.Enter();
            
            Debug.Log($"[StateMachine] Initialized with state: {initialState?.GetType().Name}");
        }
        
        /// <summary>
        /// Changes to a new state.
        /// </summary>
        /// <param name="newState">The new state to change to</param>
        public void ChangeState(IState newState)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[StateMachine] Cannot change state before initialization. Call Initialize first.");
                return;
            }
            
            if (newState == null)
            {
                Debug.LogError("[StateMachine] Cannot change to a null state.");
                return;
            }
            
            if (CurrentState == newState)
            {
                Debug.LogWarning($"[StateMachine] Already in state {newState.GetType().Name}. State change ignored.");
                return;
            }
            
            // Exit the current state
            CurrentState?.Exit();
            
            // Store the previous state
            PreviousState = CurrentState;
            
            // Set and enter the new state
            CurrentState = newState;
            CurrentState.Enter();
            
            // Trigger the state changed event
            OnStateChanged?.Invoke(PreviousState, CurrentState);
            
            Debug.Log($"[StateMachine] State changed from {PreviousState?.GetType().Name} to {CurrentState.GetType().Name}");
        }
        
        /// <summary>
        /// Updates the current state.
        /// </summary>
        public void Update()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            CurrentState?.Update();
        }
        
        /// <summary>
        /// Updates the current state at fixed intervals for physics.
        /// </summary>
        public void FixedUpdate()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            CurrentState?.FixedUpdate();
        }
    }
}
