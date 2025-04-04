using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the main menu
    /// </summary>
    public class MainMenuState : GameState
    {
        /// <summary>
        /// Initialize the list of allowed state transitions
        /// </summary>
        protected override void InitializeAllowedTransitions()
        {
            AllowTransitionTo<LoadingState>();
            AllowTransitionTo<MatchmakingState>();
            AllowTransitionTo<GameplayState>();
        }
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Show main menu UI
            Debug.Log("Showing main menu UI");
            
            // TODO: Implement main menu UI display
        }
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            // Handle main menu updates if needed
        }
        
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            
            // Hide main menu UI
            Debug.Log("Hiding main menu UI");
            
            // TODO: Implement main menu UI hiding
        }
    }
}
