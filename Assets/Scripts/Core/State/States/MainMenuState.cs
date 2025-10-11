using UI.UISystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.State.States
{
    /// <summary>
    /// State for the main menu.
    /// </summary>
    public class MainMenuState : BaseState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Load the main menu scene if not already loaded
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }

            // Show the main menu UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowScreen(UIScreenType.Menu, true, false);
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the main menu UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.HideScreen(UIScreenType.Menu, true);
            }
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Main menu update logic
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Main menu physics update logic
        }
    }
}