using System;
using Core.Patterns;
using UnityEngine;

namespace Core.GameFramework.State
{
    /// <summary>
    /// Manages game states and transitions between them.
    /// </summary>
    public class GameStateManager : MonoBehaviourSingleton<GameStateManager>
    {
        /// <summary>
        /// Event triggered when a state transition occurs.
        /// </summary>
        public event Action<IState, IState> OnStateChanged;

        /// <summary>
        /// The state machine that manages game states.
        /// </summary>
        private IStateMachine _stateMachine;

        /// <summary>
        /// The current active state.
        /// </summary>
        public IState CurrentState => _stateMachine?.CurrentState;

        /// <summary>
        /// The previous state before the current one.
        /// </summary>
        public IState PreviousState => _stateMachine?.PreviousState;

        /// <summary>
        /// Initialize the game state manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Create the state machine
            _stateMachine = new StateMachine();

            // Subscribe to state machine events
            _stateMachine.OnStateChanged += HandleStateChanged;

            Debug.Log("[GameStateManager] Initialized");
        }

        /// <summary>
        /// Update the current state.
        /// </summary>
        private void Update()
        {
            // Update the state machine
            _stateMachine?.Update();
        }

        /// <summary>
        /// Update the current state at fixed intervals for physics.
        /// </summary>
        private void FixedUpdate()
        {
            // Update the state machine for physics
            _stateMachine?.FixedUpdate();
        }

        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from state machine events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
        }

        /// <summary>
        /// Initialize the state machine with an initial state.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        public void Initialize(IState initialState)
        {
            if (_stateMachine != null)
            {
                _stateMachine.Initialize(initialState);
            }
        }

        /// <summary>
        /// Change to a new state.
        /// </summary>
        /// <param name="newState">The new state to change to</param>
        public void ChangeState(IState newState)
        {
            if (_stateMachine != null)
            {
                _stateMachine.ChangeState(newState);
            }
        }

        /// <summary>
        /// Change to a new state of type T.
        /// </summary>
        /// <typeparam name="T">The type of state to change to</typeparam>
        public void ChangeState<T>() where T : IState, new()
        {
            ChangeState(new T());
        }

        /// <summary>
        /// Handle state machine state changed event.
        /// </summary>
        /// <param name="previousState">The previous state</param>
        /// <param name="currentState">The current state</param>
        private void HandleStateChanged(IState previousState, IState currentState)
        {
            Debug.Log($"[GameStateManager] State changed from {(previousState != null ? previousState.GetType().Name : "null")} to {currentState.GetType().Name}");

            // Forward the event
            OnStateChanged?.Invoke(previousState, currentState);
        }
    }
}
