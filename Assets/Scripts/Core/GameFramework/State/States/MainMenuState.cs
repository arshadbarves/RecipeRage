using RecipeRage.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the main menu of the game.
    /// </summary>
    public class MainMenuState : GameState
    {
        // No need to store references to UI elements anymore as they're managed by the UIManager

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Show main menu UI
            Debug.Log("[MainMenuState] Showing main menu UI");

            // Show the main menu screen using the UI Manager
            UI.UIManager.Instance.ShowScreen<UI.Screens.MainMenuScreen>(true);
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide main menu UI
            Debug.Log("[MainMenuState] Hiding main menu UI");

            // Hide the main menu screen
            UI.UIManager.Instance.GetScreen<UI.Screens.MainMenuScreen>()?.Hide(true);
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Handle main menu input and logic is now handled by the MainMenuUI component
        }
    }
}
