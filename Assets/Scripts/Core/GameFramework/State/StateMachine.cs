using System;
using UnityEngine;

namespace Core.GameFramework.State
{
    /// <summary>
    /// Implementation of a state machine.
    /// </summary>
    public class StateMachine : IStateMachine
    {
        /// <summary>
        /// Event triggered when a state transition occurs.
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
        /// Initialize the state machine with an initial state.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        public void Initialize(IState initialState)
        {
            CurrentState = initialState;
            CurrentState.Enter();

            Debug.Log($"[StateMachine] Initialized with state: {initialState.GetType().Name}");
        }

        /// <summary>
        /// Change to a new state.
        /// </summary>
        /// <param name="newState">The new state to change to</param>
        public void ChangeState(IState newState)
        {
            if (newState == null)
            {
                Debug.LogError("[StateMachine] Cannot change to a null state");
                return;
            }

            if (CurrentState == newState)
            {
                Debug.LogWarning($"[StateMachine] Already in state: {newState.GetType().Name}");
                return;
            }

            // Exit the current state
            if (CurrentState != null)
            {
                CurrentState.Exit();
            }

            // Store the previous state
            PreviousState = CurrentState;

            // Enter the new state
            CurrentState = newState;
            CurrentState.Enter();

            // Trigger the state changed event
            OnStateChanged?.Invoke(PreviousState, CurrentState);

            Debug.Log($"[StateMachine] Changed state from {(PreviousState != null ? PreviousState.GetType().Name : "null")} to {CurrentState.GetType().Name}");
        }

        /// <summary>
        /// Update the current state.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// Update the current state at fixed intervals for physics.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}
