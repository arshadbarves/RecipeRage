using Core.UI.Animation;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Initializes the UI system and creates all screens
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        /// <summary>
        /// Main menu screen UXML asset
        /// </summary>
        [SerializeField] private VisualTreeAsset _mainMenuScreenUXML;

        /// <summary>
        /// Character selection screen UXML asset
        /// </summary>
        [SerializeField] private VisualTreeAsset _characterSelectionScreenUXML;

        /// <summary>
        /// Game mode selection screen UXML asset
        /// </summary>
        [SerializeField] private VisualTreeAsset _gameModeSelectionScreenUXML;

        /// <summary>
        /// Settings screen UXML asset
        /// </summary>
        [SerializeField] private VisualTreeAsset _settingsScreenUXML;

        /// <summary>
        /// Common USS asset
        /// </summary>
        [SerializeField] private StyleSheet _commonUSS;

        /// <summary>
        /// Main menu screen USS asset
        /// </summary>
        [SerializeField] private StyleSheet _mainMenuScreenUSS;

        /// <summary>
        /// Character selection screen USS asset
        /// </summary>
        [SerializeField] private StyleSheet _characterSelectionScreenUSS;

        /// <summary>
        /// Game mode selection screen USS asset
        /// </summary>
        [SerializeField] private StyleSheet _gameModeSelectionScreenUSS;

        /// <summary>
        /// Settings screen USS asset
        /// </summary>
        [SerializeField] private StyleSheet _settingsScreenUSS;

        /// <summary>
        /// Initialize the UI system
        /// </summary>
        private void Awake()
        {
            // Create UI Animation System
            UIAnimationSystem.Instance.gameObject.name = "UIAnimationSystem";

            // Create UI Manager
            UIManager.Instance.gameObject.name = "UIManager";

            // Create screens
            CreateMainMenuScreen();
            CreateCharacterSelectionScreen();
            CreateGameModeSelectionScreen();
            CreateSettingsScreen();

            // Show main menu screen
            UIManager.Instance.ShowScreen<MainMenuScreen>(true);
        }

        /// <summary>
        /// Create the main menu screen
        /// </summary>
        private void CreateMainMenuScreen()
        {
            GameObject screenObject = new GameObject("MainMenuScreen");
            DontDestroyOnLoad(screenObject);

            UIDocument uiDocument = screenObject.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = _mainMenuScreenUXML;

            if (_commonUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_commonUSS);

            if (_mainMenuScreenUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_mainMenuScreenUSS);

            MainMenuScreen screen = screenObject.AddComponent<MainMenuScreen>();
            UIManager.Instance.RegisterScreen(screen);
        }

        /// <summary>
        /// Create the character selection screen
        /// </summary>
        private void CreateCharacterSelectionScreen()
        {
            GameObject screenObject = new GameObject("CharacterSelectionScreen");
            DontDestroyOnLoad(screenObject);

            UIDocument uiDocument = screenObject.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = _characterSelectionScreenUXML;

            if (_commonUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_commonUSS);

            if (_characterSelectionScreenUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_characterSelectionScreenUSS);

            CharacterSelectionScreen screen = screenObject.AddComponent<CharacterSelectionScreen>();
            UIManager.Instance.RegisterScreen(screen);
        }

        /// <summary>
        /// Create the game mode selection screen
        /// </summary>
        private void CreateGameModeSelectionScreen()
        {
            GameObject screenObject = new GameObject("GameModeSelectionScreen");
            DontDestroyOnLoad(screenObject);

            UIDocument uiDocument = screenObject.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = _gameModeSelectionScreenUXML;

            if (_commonUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_commonUSS);

            if (_gameModeSelectionScreenUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_gameModeSelectionScreenUSS);

            GameModeSelectionScreen screen = screenObject.AddComponent<GameModeSelectionScreen>();
            UIManager.Instance.RegisterScreen(screen);
        }

        /// <summary>
        /// Create the settings screen
        /// </summary>
        private void CreateSettingsScreen()
        {
            GameObject screenObject = new GameObject("SettingsScreen");
            DontDestroyOnLoad(screenObject);

            UIDocument uiDocument = screenObject.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = _settingsScreenUXML;

            if (_commonUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_commonUSS);

            if (_settingsScreenUSS != null)
                uiDocument.rootVisualElement.styleSheets.Add(_settingsScreenUSS);

            SettingsScreen screen = screenObject.AddComponent<SettingsScreen>();
            UIManager.Instance.RegisterScreen(screen);
        }
    }
}
