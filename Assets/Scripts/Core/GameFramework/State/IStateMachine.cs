using System;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Interface for state machines.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Event triggered when a state transition occurs.
        /// </summary>
        event Action<IState, IState> OnStateChanged;
        
        /// <summary>
        /// The current active state.
        /// </summary>
        IState CurrentState { get; }
        
        /// <summary>
        /// The previous state before the current one.
        /// </summary>
        IState PreviousState { get; }
        
        /// <summary>
        /// Initialize the state machine with an initial state.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        void Initialize(IState initialState);
        
        /// <summary>
        /// Change to a new state.
        /// </summary>
        /// <param name="newState">The new state to change to</param>
        void ChangeState(IState newState);
        
        /// <summary>
        /// Update the current state.
        /// </summary>
        void Update();
        
        /// <summary>
        /// Update the current state at fixed intervals for physics.
        /// </summary>
        void FixedUpdate();
    }
}
