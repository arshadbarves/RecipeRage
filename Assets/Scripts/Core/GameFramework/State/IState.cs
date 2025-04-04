using System;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Interface for all game states in the state machine
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Event triggered when the state is entered
        /// </summary>
        event Action OnStateEntered;
        
        /// <summary>
        /// Event triggered when the state is exited
        /// </summary>
        event Action OnStateExited;
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        void Enter();
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        void Update();
        
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        void Exit();
        
        /// <summary>
        /// Whether the state can transition to the specified state
        /// </summary>
        /// <param name="nextState">The state to transition to</param>
        /// <returns>True if the transition is allowed</returns>
        bool CanTransitionTo(IState nextState);
    }
}
