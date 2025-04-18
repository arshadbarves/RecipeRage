using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the main menu.
    /// </summary>
    public class MainMenuState : IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[MainMenuState] Entered");
            
            // Load the main menu scene if not already loaded
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            
            // Show the main menu UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMainMenu();
            }
        }
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[MainMenuState] Exited");
            
            // Hide the main menu UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.HideMainMenu();
            }
        }
        
        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public void Update()
        {
            // Main menu update logic
        }
        
        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public void FixedUpdate()
        {
            // Main menu physics update logic
        }
    }
}
