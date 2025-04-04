using UnityEngine;

namespace RecipeRage.Core.GameFramework.State
{
    /// <summary>
    /// Base class for game states with common functionality.
    /// </summary>
    public abstract class GameState : IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        public string StateName => GetType().Name;
        
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public virtual void Enter()
        {
            Debug.Log($"[GameState] Entered state: {StateName}");
        }
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            Debug.Log($"[GameState] Exited state: {StateName}");
        }
        
        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public virtual void Update()
        {
            // Base implementation does nothing
        }
        
        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public virtual void FixedUpdate()
        {
            // Base implementation does nothing
        }
    }
}
