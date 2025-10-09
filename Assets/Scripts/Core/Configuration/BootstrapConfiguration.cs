using System;
using UnityEngine;

namespace Core.Configuration
{
    [Serializable]
    public class BootstrapConfiguration
    {
        [Header("Splash Screen")]
        [Tooltip("Whether to show company splash screens on startup")]
        public bool showSplashScreens = true;
        
        [Tooltip("How long to display the company splash screen")]
        [Range(1f, 10f)]
        public float companySplashDuration = 2.5f;
        
        [Tooltip("Duration for splash screen fade in animation")]
        [Range(0.1f, 2f)]
        public float splashFadeInDuration = 0.8f;
        
        [Tooltip("Duration for splash screen fade out animation")]
        [Range(0.1f, 2f)]
        public float splashFadeOutDuration = 0.5f;

        [Header("Loading Screen")]
        [Tooltip("Minimum time to show loading screen (prevents flashing)")]
        [Range(1f, 10f)]
        public float minLoadingScreenDuration = 3.0f;
        
        [Tooltip("Duration for loading screen fade transitions")]
        [Range(0.1f, 2f)]
        public float loadingFadeTransitionDuration = 0.5f;
        
        [Tooltip("How often to change loading tips")]
        [Range(2f, 10f)]
        public float tipChangeDuration = 5.0f;
        
        [Header("System Initialization")]
        [Tooltip("Maximum time to wait for all systems to initialize")]
        [Range(10f, 60f)]
        public float initializationTimeout = 30f;

        [Tooltip("Timeout for individual system initialization")]
        [Range(1f, 15f)]
        public float systemInitTimeout = 5f;

        [Tooltip("Which game state to start in after initialization")]
        public GameBootstrap.GameStateType initialState = GameBootstrap.GameStateType.MainMenu;

        [Header("Loading Tips")]
        [Tooltip("Tips to cycle through during loading")]
        [TextArea(2, 5)]
        public string[] loadingTips = {
            "Tip: Use WASD to move around the kitchen",
            "Tip: Press Space to interact with cooking stations",
            "Tip: Complete orders quickly for bonus points",
            "Tip: Keep your kitchen organized for better efficiency",
            "Tip: Watch the timer - don't let orders expire!",
            "Tip: Upgrade your equipment to cook faster"
        };

        [Header("Debug Settings")]
        [Tooltip("Enable verbose logging during initialization")]
        public bool enableDebugLogging = false;

        /// <summary>
        /// Validates the configuration settings
        /// </summary>
        private void OnValidate()
        {
            // Ensure minimum durations
            companySplashDuration = Mathf.Max(0.5f, companySplashDuration);
            minLoadingScreenDuration = Mathf.Max(1f, minLoadingScreenDuration);
            initializationTimeout = Mathf.Max(5f, initializationTimeout);
            
            // Ensure we have at least one loading tip
            if (loadingTips == null || loadingTips.Length == 0)
            {
                loadingTips = new[] { "Loading..." };
            }
        }

        /// <summary>
        /// Should splash screens be shown based on current build settings
        /// </summary>
        public bool ShouldShowSplashScreens
        {
            get
            {
                return showSplashScreens;
            }
        }
    }
}