using System;
using Core.Patterns;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.SplashScreen
{
    /// <summary>
    /// Manages the splash screens for the game.
    /// </summary>
    public class SplashScreenManager : MonoBehaviourSingleton<SplashScreenManager>
    {
        [Header("Splash Screen Settings")] [SerializeField]
        private float _companySplashDuration = 2.5f;

        [SerializeField] private float _fadeInDuration = 0.8f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        [Header("UI Documents")] [SerializeField]
        private UIDocument _companySplashDocument;

        /// <summary>
        /// Set the company splash document.
        /// </summary>
        /// <param name="document">The UI document for the company splash screen</param>
        public void SetCompanySplashDocument(UIDocument document)
        {
            _companySplashDocument = document;
        }

        private VisualElement _companySplashRoot;

        /// <summary>
        /// Initialize the splash screen manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize UI elements
            InitializeUIElements();

            // Hide splash screen initially
            HideSplashScreen();

            Debug.Log("[SplashScreenManager] Initialized");
        }

        /// <summary>
        /// Initialize UI elements from the UI Documents.
        /// </summary>
        private void InitializeUIElements()
        {
            // Company splash elements
            if (_companySplashDocument != null)
            {
                _companySplashRoot = _companySplashDocument.rootVisualElement.Q("company-splash-root");
                if (_companySplashRoot == null)
                {
                    Debug.LogError("[SplashScreenManager] Could not find company-splash-root element");
                }
            }
        }

        /// <summary>
        /// Hide the splash screen.
        /// </summary>
        private void HideSplashScreen()
        {
            if (_companySplashDocument != null)
            {
                _companySplashDocument.enabled = false;
            }
        }

        /// <summary>
        /// Show the company splash screen.
        /// </summary>
        public void ShowCompanySplash()
        {
            if (_companySplashDocument == null || _companySplashRoot == null)
            {
                Debug.LogWarning("[SplashScreenManager] Company splash screen not set up");
                return;
            }

            _companySplashDocument.enabled = true;
            
            // Set initial state - completely transparent
            _companySplashRoot.style.opacity = 0;

            // Calculate total duration for the sequence
            int totalDuration = Mathf.RoundToInt((_fadeInDuration + _companySplashDuration + _fadeOutDuration) * 1000);

            // Use the animation system's FadeIn animation with custom duration and completion callback
            UnityNativeUIAnimationSystem.AnimateOpacity(_companySplashRoot, 0, 1,
                Mathf.RoundToInt(_fadeInDuration * 1000), 0, UnityNativeUIAnimationSystem.EasingCurve.EaseOut, () =>
                {
                    // After fade in, wait for hold duration, then fade out
                    UnityNativeUIAnimationSystem.AnimateOpacity(_companySplashRoot, 1, 0,
                        Mathf.RoundToInt(_fadeOutDuration * 1000), 
                        Mathf.RoundToInt(_companySplashDuration * 1000), // Use delay for hold duration
                        UnityNativeUIAnimationSystem.EasingCurve.EaseOut, () =>
                        {
                            _companySplashDocument.enabled = false;
                            OnSplashComplete?.Invoke();
                        });
                });
        }

        /// <summary>
        /// Event triggered when splash screen completes
        /// </summary>
        public event Action OnSplashComplete;
    }
}