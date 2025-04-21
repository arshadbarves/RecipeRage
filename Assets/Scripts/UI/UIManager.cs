using System;
using System.Collections.Generic;
using Core.Patterns;
using UI.Screens;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages all UI screens in the game
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {

        /// <summary>
        /// Currently active screen
        /// </summary>
        private UIScreen _currentScreen;

        /// <summary>
        /// Reference to the gameplay UI manager
        /// </summary>
        private GameplayUIManager _gameplayUIManager;

        /// <summary>
        /// Stack of previous screens for navigation history
        /// </summary>
        private readonly Stack<UIScreen> _screenHistory = new Stack<UIScreen>();
        /// <summary>
        /// Dictionary of all registered screens
        /// </summary>
        private readonly Dictionary<Type, UIScreen> _screens = new Dictionary<Type, UIScreen>();

        /// <summary>
        /// Register a screen with the manager
        /// </summary>
        /// <typeparam name="T"> Type of screen to register </typeparam>
        /// <param name="screen"> Screen instance </param>
        public void RegisterScreen<T>(T screen) where T : UIScreen
        {
            Type screenType = typeof(T);

            if (_screens.ContainsKey(screenType))
            {
                Debug.LogWarning($"[UIManager] Screen of type {screenType.Name} already registered");
                return;
            }

            _screens[screenType] = screen;
        }

        /// <summary>
        /// Unregister a screen from the manager
        /// </summary>
        /// <typeparam name="T"> Type of screen to unregister </typeparam>
        public void UnregisterScreen<T>() where T : UIScreen
        {
            Type screenType = typeof(T);

            if (!_screens.ContainsKey(screenType))
            {
                Debug.LogWarning($"[UIManager] Screen of type {screenType.Name} not registered");
                return;
            }

            _screens.Remove(screenType);
        }

        /// <summary>
        /// Show a specific screen
        /// </summary>
        /// <typeparam name="T"> Type of screen to show </typeparam>
        /// <param name="animate"> Whether to animate the transition </param>
        /// <param name="addToHistory"> Whether to add the current screen to history </param>
        public void ShowScreen<T>(bool animate = true, bool addToHistory = true) where T : UIScreen
        {
            Type screenType = typeof(T);

            if (!_screens.ContainsKey(screenType))
            {
                Debug.LogError($"[UIManager] Screen of type {screenType.Name} not registered");
                return;
            }

            UIScreen targetScreen = _screens[screenType];

            // Hide current screen if there is one
            if (_currentScreen != null && _currentScreen != targetScreen)
            {
                if (addToHistory)
                {
                    _screenHistory.Push(_currentScreen);
                }

                _currentScreen.Hide(animate);
            }

            // Show target screen
            targetScreen.Show(animate);
            _currentScreen = targetScreen;
        }

        /// <summary>
        /// Get a registered screen
        /// </summary>
        /// <typeparam name="T"> Type of screen to get </typeparam>
        /// <returns> The screen instance or null if not found </returns>
        public T GetScreen<T>() where T : UIScreen
        {
            Type screenType = typeof(T);

            if (!_screens.ContainsKey(screenType))
            {
                Debug.LogWarning($"[UIManager] Screen of type {screenType.Name} not registered");
                return null;
            }

            return (T)_screens[screenType];
        }

        /// <summary>
        /// Go back to the previous screen
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        /// <returns> True if successfully navigated back, false if history is empty </returns>
        public bool GoBack(bool animate = true)
        {
            if (_screenHistory.Count == 0)
            {
                return false;
            }

            UIScreen previousScreen = _screenHistory.Pop();

            // Hide current screen
            if (_currentScreen != null)
            {
                _currentScreen.Hide(animate);
            }

            // Show previous screen
            previousScreen.Show(animate);
            _currentScreen = previousScreen;

            return true;
        }

        /// <summary>
        /// Clear the screen history
        /// </summary>
        public void ClearHistory()
        {
            _screenHistory.Clear();
        }
        /// <summary>
        /// Show the gameplay UI
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void ShowGameplay(bool animate = true)
        {
            Debug.Log("[UIManager] Showing gameplay UI");

            // Hide all UI screens
            if (_currentScreen != null)
            {
                _currentScreen.Hide(animate);
                _currentScreen = null;
            }

            // Find the gameplay UI manager if not already cached
            if (_gameplayUIManager == null)
            {
                _gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();

                // If still not found, it might be created later in the Game scene
                if (_gameplayUIManager == null)
                {
                    Debug.LogWarning("[UIManager] GameplayUIManager not found. It might be created later in the Game scene.");
                    return;
                }
            }

            // Enable the gameplay UI
            if (_gameplayUIManager != null)
            {
                _gameplayUIManager.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the gameplay UI
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void HideGameplay(bool animate = true)
        {
            Debug.Log("[UIManager] Hiding gameplay UI");

            // Find the gameplay UI manager if not already cached
            if (_gameplayUIManager == null)
            {
                _gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();

                if (_gameplayUIManager == null)
                {
                    Debug.LogWarning("[UIManager] GameplayUIManager not found");
                    return;
                }
            }

            // Disable the gameplay UI
            if (_gameplayUIManager != null)
            {
                _gameplayUIManager.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show the main menu
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void ShowMainMenu(bool animate = true)
        {
            ShowScreen<MainMenuScreen>(animate, false);
        }

        /// <summary>
        /// Hide the main menu
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void HideMainMenu(bool animate = true)
        {
            MainMenuScreen mainMenuScreen = GetScreen<MainMenuScreen>();
            if (mainMenuScreen != null && mainMenuScreen.IsVisible)
            {
                mainMenuScreen.Hide(animate);
            }
        }

        /// <summary>
        /// Show the game over screen
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void ShowGameOver(bool animate = true)
        {
            // Hide gameplay UI first
            HideGameplay(false);

            // If there's a GameOverScreen, show it
            // Otherwise, this would need to be implemented
            // For now, we'll just show a placeholder or return to main menu
            Debug.Log("[UIManager] Game Over - Returning to main menu");
            ShowMainMenu(animate);
        }

        /// <summary>
        /// Hide the game over screen
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void HideGameOver(bool animate = true)
        {
            // If there's a GameOverScreen, hide it
            // For now, we'll just ensure we're not showing it
            Debug.Log("[UIManager] Hiding game over screen");
        }

        /// <summary>
        /// Show the lobby UI
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void ShowLobby(bool animate = true)
        {
            Debug.Log("[UIManager] Showing lobby UI");

            // Find the LobbyUI component if it exists
            LobbyUI lobbyUI = FindFirstObjectByType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.gameObject.SetActive(true);
                return;
            }

            // If no dedicated LobbyUI exists, show the main menu screen as a fallback
            // This assumes the main menu has lobby functionality or can transition to it
            ShowMainMenu(animate);
        }

        /// <summary>
        /// Hide the lobby UI
        /// </summary>
        /// <param name="animate"> Whether to animate the transition </param>
        public void HideLobby(bool animate = true)
        {
            Debug.Log("[UIManager] Hiding lobby UI");

            // Find the LobbyUI component if it exists
            LobbyUI lobbyUI = FindFirstObjectByType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.gameObject.SetActive(false);
                return;
            }

            // If no dedicated LobbyUI exists, we don't need to do anything special
            // as the state transition will handle showing the next UI
        }
    }
}