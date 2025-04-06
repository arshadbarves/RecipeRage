using RecipeRage.UI;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the main menu of the game.
    /// </summary>
    public class MainMenuState : GameState
    {
        /// <summary>
        /// Reference to the main menu UI prefab.
        /// </summary>
        private GameObject _mainMenuUIPrefab;

        /// <summary>
        /// Reference to the instantiated main menu UI.
        /// </summary>
        private GameObject _mainMenuUIInstance;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Show main menu UI
            Debug.Log("[MainMenuState] Showing main menu UI");

            // Load the main menu UI prefab if not already loaded
            if (_mainMenuUIPrefab == null)
            {
                _mainMenuUIPrefab = Resources.Load<GameObject>("UI/MainMenuUI");

                if (_mainMenuUIPrefab == null)
                {
                    Debug.LogError("[MainMenuState] Failed to load MainMenuUI prefab from Resources/UI/MainMenuUI");
                    return;
                }
            }

            // Instantiate the main menu UI
            _mainMenuUIInstance = GameObject.Instantiate(_mainMenuUIPrefab);

            // Make sure it persists across scene loads
            GameObject.DontDestroyOnLoad(_mainMenuUIInstance);
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide main menu UI
            Debug.Log("[MainMenuState] Hiding main menu UI");

            // Destroy the main menu UI instance
            if (_mainMenuUIInstance != null)
            {
                GameObject.Destroy(_mainMenuUIInstance);
                _mainMenuUIInstance = null;
            }
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
