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

            // Create a UI Document GameObject
            _mainMenuUIInstance = new GameObject("MainMenuUI");

            // Add UIDocument component
            UIDocument uiDocument = _mainMenuUIInstance.AddComponent<UIDocument>();

            // Load the UXML asset
            var uxmlAsset = Resources.Load<VisualTreeAsset>("UI/MainMenuUI");
            if (uxmlAsset == null)
            {
                Debug.LogError("[MainMenuState] Failed to load MainMenuUI UXML from Resources/UI/MainMenuUI");
                return;
            }

            // Assign the UXML asset to the UIDocument
            uiDocument.visualTreeAsset = uxmlAsset;

            // Load the USS assets
            var commonStyleSheet = Resources.Load<StyleSheet>("UI/Common");
            var mainMenuStyleSheet = Resources.Load<StyleSheet>("UI/MainMenuUI");

            if (commonStyleSheet != null && mainMenuStyleSheet != null)
            {
                // Add style sheets to the UIDocument
                uiDocument.styleSheets.Add(commonStyleSheet);
                uiDocument.styleSheets.Add(mainMenuStyleSheet);
            }
            else
            {
                Debug.LogWarning("[MainMenuState] Failed to load one or more style sheets");
            }

            // Add MainMenuUI component
            _mainMenuUIInstance.AddComponent<UI.MainMenuUI>();

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
