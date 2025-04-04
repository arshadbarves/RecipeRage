using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Base class for all game states
    /// </summary>
    public abstract class GameState : IState
    {
        /// <summary>
        /// Event triggered when the state is entered
        /// </summary>
        public event Action OnStateEntered;
        
        /// <summary>
        /// Event triggered when the state is exited
        /// </summary>
        public event Action OnStateExited;
        
        /// <summary>
        /// List of state types that this state can transition to
        /// </summary>
        protected List<Type> _allowedTransitions = new List<Type>();
        
        /// <summary>
        /// Whether the state is currently active
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Constructor to initialize the state
        /// </summary>
        protected GameState()
        {
            // Initialize allowed transitions in derived classes
            InitializeAllowedTransitions();
        }
        
        /// <summary>
        /// Initialize the list of allowed state transitions
        /// </summary>
        protected abstract void InitializeAllowedTransitions();
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public virtual void Enter()
        {
            if (IsActive)
            {
                Debug.LogWarning($"State {GetType().Name} is already active.");
                return;
            }
            
            IsActive = true;
            Debug.Log($"Entering state: {GetType().Name}");
            OnStateEntered?.Invoke();
        }
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        public virtual void Update()
        {
            if (!IsActive)
            {
                return;
            }
            
            // Update logic implemented in derived classes
        }
        
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        public virtual void Exit()
        {
            if (!IsActive)
            {
                Debug.LogWarning($"State {GetType().Name} is not active.");
                return;
            }
            
            IsActive = false;
            Debug.Log($"Exiting state: {GetType().Name}");
            OnStateExited?.Invoke();
        }
        
        /// <summary>
        /// Whether the state can transition to the specified state
        /// </summary>
        /// <param name="nextState">The state to transition to</param>
        /// <returns>True if the transition is allowed</returns>
        public virtual bool CanTransitionTo(IState nextState)
        {
            if (nextState == null)
            {
                return false;
            }
            
            Type nextStateType = nextState.GetType();
            return _allowedTransitions.Contains(nextStateType);
        }
        
        /// <summary>
        /// Add a state type to the list of allowed transitions
        /// </summary>
        /// <typeparam name="T">The state type to allow</typeparam>
        protected void AllowTransitionTo<T>() where T : IState
        {
            Type stateType = typeof(T);
            if (!_allowedTransitions.Contains(stateType))
            {
                _allowedTransitions.Add(stateType);
            }
        }
    }
}
