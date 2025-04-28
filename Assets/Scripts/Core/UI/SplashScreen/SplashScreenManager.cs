using System;
using System.Collections;
using System.Threading.Tasks;
using Core.Patterns;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.SplashScreen
{
    /// <summary>
    /// Manages the splash screens and loading screen for the game.
    /// </summary>
    public class SplashScreenManager : MonoBehaviourSingleton<SplashScreenManager>
    {
        [Header("Splash Screen Settings")]
        [SerializeField] public float _companySplashDuration = 2.5f;
        [SerializeField] public float _gameLogoSplashDuration = 3.0f;
        [SerializeField] public float _fadeTransitionDuration = 0.5f;
        [SerializeField] public float _minLoadingScreenDuration = 3.0f;
        [SerializeField] public bool _allowSkipping = true;
        
        [Header("UI Documents")]
        [SerializeField] public UIDocument _companySplashDocument;
        [SerializeField] public UIDocument _gameLogoSplashDocument;
        [SerializeField] public UIDocument _loadingScreenDocument;
        
        [Header("Loading Tips")]
        [SerializeField] public string[] _loadingTips;
        [SerializeField] public float _tipChangeDuration = 5.0f;
        
        private VisualElement _companySplashRoot;
        private VisualElement _gameLogoSplashRoot;
        private VisualElement _loadingScreenRoot;
        private ProgressBar _loadingProgressBar;
        private Label _loadingStatusLabel;
        private Label _loadingTipLabel;
        
        private bool _isSkipRequested = false;
        private float _loadingStartTime;
        private Coroutine _tipsCoroutine;
        
        /// <summary>
        /// Initialize the splash screen manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize UI elements
            InitializeUIElements();
            
            // Hide all screens initially
            HideAllScreens();
            
            Debug.Log("[SplashScreenManager] Initialized");
        }
        
        /// <summary>
        /// Initialize UI elements from the UI Documents.
        /// </summary>
        private void InitializeUIElements()
        {
            try
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
                
                // Game logo splash elements
                if (_gameLogoSplashDocument != null)
                {
                    _gameLogoSplashRoot = _gameLogoSplashDocument.rootVisualElement.Q("game-logo-splash-root");
                    if (_gameLogoSplashRoot == null)
                    {
                        Debug.LogError("[SplashScreenManager] Could not find game-logo-splash-root element");
                    }
                }
                
                // Loading screen elements
                if (_loadingScreenDocument != null)
                {
                    _loadingScreenRoot = _loadingScreenDocument.rootVisualElement.Q("loading-screen-root");
                    if (_loadingScreenRoot == null)
                    {
                        Debug.LogError("[SplashScreenManager] Could not find loading-screen-root element");
                    }
                    else
                    {
                        _loadingProgressBar = _loadingScreenRoot.Q<ProgressBar>("loading-progress-bar");
                        _loadingStatusLabel = _loadingScreenRoot.Q<Label>("loading-status-label");
                        _loadingTipLabel = _loadingScreenRoot.Q<Label>("loading-tip-label");
                        
                        if (_loadingProgressBar == null)
                        {
                            Debug.LogError("[SplashScreenManager] Could not find loading-progress-bar element");
                        }
                        
                        if (_loadingStatusLabel == null)
                        {
                            Debug.LogError("[SplashScreenManager] Could not find loading-status-label element");
                        }
                        
                        if (_loadingTipLabel == null)
                        {
                            Debug.LogError("[SplashScreenManager] Could not find loading-tip-label element");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SplashScreenManager] Error initializing UI elements: {e.Message}");
            }
        }
        
        /// <summary>
        /// Hide all splash screens.
        /// </summary>
        private void HideAllScreens()
        {
            if (_companySplashDocument != null)
            {
                _companySplashDocument.enabled = false;
            }
            
            if (_gameLogoSplashDocument != null)
            {
                _gameLogoSplashDocument.enabled = false;
            }
            
            if (_loadingScreenDocument != null)
            {
                _loadingScreenDocument.enabled = false;
            }
        }
        
        /// <summary>
        /// Show the company splash screen.
        /// </summary>
        public async Task ShowCompanySplash()
        {
            if (_companySplashDocument == null || _companySplashRoot == null)
            {
                Debug.LogWarning("[SplashScreenManager] Company splash screen not set up");
                return;
            }
            
            _isSkipRequested = false;
            _companySplashDocument.enabled = true;
            
            // Register skip input handler
            if (_allowSkipping)
            {
                RegisterSkipHandler();
            }
            
            // Fade in
            await FadeIn(_companySplashRoot);
            
            // Wait for duration or skip
            await WaitForDurationOrSkip(_companySplashDuration);
            
            // Fade out
            await FadeOut(_companySplashRoot);
            
            _companySplashDocument.enabled = false;
            
            // Unregister skip input handler
            if (_allowSkipping)
            {
                UnregisterSkipHandler();
            }
            
            Debug.Log("[SplashScreenManager] Company splash screen completed");
        }
        
        /// <summary>
        /// Show the game logo splash screen.
        /// </summary>
        public async Task ShowGameLogoSplash()
        {
            if (_gameLogoSplashDocument == null || _gameLogoSplashRoot == null)
            {
                Debug.LogWarning("[SplashScreenManager] Game logo splash screen not set up");
                return;
            }
            
            _isSkipRequested = false;
            _gameLogoSplashDocument.enabled = true;
            
            // Register skip input handler
            if (_allowSkipping)
            {
                RegisterSkipHandler();
            }
            
            // Fade in
            await FadeIn(_gameLogoSplashRoot);
            
            // Wait for duration or skip
            await WaitForDurationOrSkip(_gameLogoSplashDuration);
            
            // Fade out
            await FadeOut(_gameLogoSplashRoot);
            
            _gameLogoSplashDocument.enabled = false;
            
            // Unregister skip input handler
            if (_allowSkipping)
            {
                UnregisterSkipHandler();
            }
            
            Debug.Log("[SplashScreenManager] Game logo splash screen completed");
        }
        
        /// <summary>
        /// Show the loading screen.
        /// </summary>
        public void ShowLoadingScreen()
        {
            if (_loadingScreenDocument == null || _loadingScreenRoot == null)
            {
                Debug.LogWarning("[SplashScreenManager] Loading screen not set up");
                return;
            }
            
            _loadingStartTime = Time.time;
            _loadingScreenDocument.enabled = true;
            
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = 0;
            }
            
            if (_loadingStatusLabel != null)
            {
                _loadingStatusLabel.text = "Initializing...";
            }
            
            // Start cycling loading tips
            if (_loadingTipLabel != null && _loadingTips != null && _loadingTips.Length > 0)
            {
                _tipsCoroutine = StartCoroutine(CycleLoadingTips());
            }
            
            // Fade in
            if (_loadingScreenRoot != null)
            {
                _loadingScreenRoot.style.opacity = 1;
            }
            
            Debug.Log("[SplashScreenManager] Loading screen shown");
        }
        
        /// <summary>
        /// Update the loading progress.
        /// </summary>
        /// <param name="status">The current loading status</param>
        /// <param name="progress">The loading progress (0-1)</param>
        public void UpdateLoadingProgress(string status, float progress)
        {
            if (_loadingStatusLabel != null)
            {
                _loadingStatusLabel.text = status;
            }
            
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = progress;
            }
        }
        
        /// <summary>
        /// Hide the loading screen.
        /// </summary>
        public async Task HideLoadingScreen()
        {
            if (_loadingScreenDocument == null || _loadingScreenRoot == null)
            {
                return;
            }
            
            // Ensure minimum display time
            float elapsedTime = Time.time - _loadingStartTime;
            if (elapsedTime < _minLoadingScreenDuration)
            {
                await Task.Delay(Mathf.RoundToInt((_minLoadingScreenDuration - elapsedTime) * 1000));
            }
            
            // Set progress to 100% for visual satisfaction
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = 1.0f;
            }
            
            if (_loadingStatusLabel != null)
            {
                _loadingStatusLabel.text = "Ready!";
            }
            
            // Wait a moment at 100%
            await Task.Delay(500);
            
            // Fade out
            await FadeOut(_loadingScreenRoot);
            
            // Stop cycling loading tips
            if (_tipsCoroutine != null)
            {
                StopCoroutine(_tipsCoroutine);
                _tipsCoroutine = null;
            }
            
            _loadingScreenDocument.enabled = false;
            
            Debug.Log("[SplashScreenManager] Loading screen hidden");
        }
        
        /// <summary>
        /// Fade in a visual element.
        /// </summary>
        /// <param name="element">The element to fade in</param>
        private async Task FadeIn(VisualElement element)
        {
            if (element == null)
            {
                return;
            }
            
            element.style.opacity = 0;
            
            float startTime = Time.time;
            while (Time.time - startTime < _fadeTransitionDuration && !_isSkipRequested)
            {
                float progress = (Time.time - startTime) / _fadeTransitionDuration;
                element.style.opacity = progress;
                await Task.Yield();
            }
            
            element.style.opacity = 1;
        }
        
        /// <summary>
        /// Fade out a visual element.
        /// </summary>
        /// <param name="element">The element to fade out</param>
        private async Task FadeOut(VisualElement element)
        {
            if (element == null)
            {
                return;
            }
            
            element.style.opacity = 1;
            
            float startTime = Time.time;
            while (Time.time - startTime < _fadeTransitionDuration && !_isSkipRequested)
            {
                float progress = 1 - ((Time.time - startTime) / _fadeTransitionDuration);
                element.style.opacity = progress;
                await Task.Yield();
            }
            
            element.style.opacity = 0;
        }
        
        /// <summary>
        /// Wait for a duration or until skip is requested.
        /// </summary>
        /// <param name="duration">The duration to wait</param>
        private async Task WaitForDurationOrSkip(float duration)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration && !_isSkipRequested)
            {
                await Task.Yield();
            }
            
            _isSkipRequested = false;
        }
        
        /// <summary>
        /// Cycle through loading tips.
        /// </summary>
        private IEnumerator CycleLoadingTips()
        {
            if (_loadingTips == null || _loadingTips.Length == 0 || _loadingTipLabel == null)
            {
                yield break;
            }
            
            int currentTipIndex = 0;
            
            while (_loadingScreenDocument.enabled)
            {
                _loadingTipLabel.text = _loadingTips[currentTipIndex];
                
                yield return new WaitForSeconds(_tipChangeDuration);
                
                currentTipIndex = (currentTipIndex + 1) % _loadingTips.Length;
            }
        }
        
        /// <summary>
        /// Register the skip input handler.
        /// </summary>
        private void RegisterSkipHandler()
        {
            // Add mouse click handler
            if (_companySplashDocument != null && _companySplashDocument.enabled)
            {
                _companySplashDocument.rootVisualElement.RegisterCallback<ClickEvent>(HandleClickEvent);
            }
            
            if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.enabled)
            {
                _gameLogoSplashDocument.rootVisualElement.RegisterCallback<ClickEvent>(HandleClickEvent);
            }
            
            // Add keyboard handler
            if (_companySplashDocument != null && _companySplashDocument.enabled)
            {
                _companySplashDocument.rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyEvent);
            }
            
            if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.enabled)
            {
                _gameLogoSplashDocument.rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyEvent);
            }
        }
        
        /// <summary>
        /// Unregister the skip input handler.
        /// </summary>
        private void UnregisterSkipHandler()
        {
            // Remove mouse click handler
            if (_companySplashDocument != null)
            {
                _companySplashDocument.rootVisualElement.UnregisterCallback<ClickEvent>(HandleClickEvent);
            }
            
            if (_gameLogoSplashDocument != null)
            {
                _gameLogoSplashDocument.rootVisualElement.UnregisterCallback<ClickEvent>(HandleClickEvent);
            }
            
            // Remove keyboard handler
            if (_companySplashDocument != null)
            {
                _companySplashDocument.rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyEvent);
            }
            
            if (_gameLogoSplashDocument != null)
            {
                _gameLogoSplashDocument.rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyEvent);
            }
        }
        
        /// <summary>
        /// Handle click events for skipping.
        /// </summary>
        private void HandleClickEvent(ClickEvent evt)
        {
            _isSkipRequested = true;
        }
        
        /// <summary>
        /// Handle key events for skipping.
        /// </summary>
        private void HandleKeyEvent(KeyDownEvent evt)
        {
            // Skip on space, enter, or escape
            if (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Escape)
            {
                _isSkipRequested = true;
            }
        }
        
        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Ensure we unregister any callbacks
            UnregisterSkipHandler();
            
            // Stop any running coroutines
            if (_tipsCoroutine != null)
            {
                StopCoroutine(_tipsCoroutine);
                _tipsCoroutine = null;
            }
        }
    }
}
