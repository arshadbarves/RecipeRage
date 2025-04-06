using System;
using System.Collections.Generic;
using RecipeRage.UI.Screens;
using UnityEngine;

namespace RecipeRage.UI
{
    /// <summary>
    /// Manages all UI screens in the game
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    _instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Dictionary of all registered screens
        /// </summary>
        private Dictionary<Type, UIScreen> _screens = new Dictionary<Type, UIScreen>();
        
        /// <summary>
        /// Currently active screen
        /// </summary>
        private UIScreen _currentScreen;
        
        /// <summary>
        /// Stack of previous screens for navigation history
        /// </summary>
        private Stack<UIScreen> _screenHistory = new Stack<UIScreen>();
        
        /// <summary>
        /// Initialize the UI manager
        /// </summary>
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Register a screen with the manager
        /// </summary>
        /// <typeparam name="T">Type of screen to register</typeparam>
        /// <param name="screen">Screen instance</param>
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
        /// <typeparam name="T">Type of screen to unregister</typeparam>
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
        /// <typeparam name="T">Type of screen to show</typeparam>
        /// <param name="animate">Whether to animate the transition</param>
        /// <param name="addToHistory">Whether to add the current screen to history</param>
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
        /// <typeparam name="T">Type of screen to get</typeparam>
        /// <returns>The screen instance or null if not found</returns>
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
        /// <param name="animate">Whether to animate the transition</param>
        /// <returns>True if successfully navigated back, false if history is empty</returns>
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
    }
}
