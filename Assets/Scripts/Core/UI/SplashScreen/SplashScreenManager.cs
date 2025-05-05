using System;
using System.Collections;
using System.Threading.Tasks;
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
        [Header("Splash Screen Settings")]
        [SerializeField] private float _companySplashDuration = 2.5f;
        [SerializeField] private float _gameLogoSplashDuration = 3.0f;
        [SerializeField] private float _fadeTransitionDuration = 0.5f;
        [SerializeField] private bool _allowSkipping = true;

        [Header("UI Documents")]
        [SerializeField] private UIDocument _companySplashDocument;
        [SerializeField] private UIDocument _gameLogoSplashDocument;

        /// <summary>
        /// Set the company splash document.
        /// </summary>
        /// <param name="document">The UI document for the company splash screen</param>
        public void SetCompanySplashDocument(UIDocument document)
        {
            _companySplashDocument = document;
        }

        /// <summary>
        /// Set the game logo splash document.
        /// </summary>
        /// <param name="document">The UI document for the game logo splash screen</param>
        public void SetGameLogoSplashDocument(UIDocument document)
        {
            _gameLogoSplashDocument = document;
        }



        private VisualElement _companySplashRoot;
        private VisualElement _gameLogoSplashRoot;

        private bool _isSkipRequested = false;

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
        }

        /// <summary>
        /// Show the company splash screen.
        /// </summary>
        public Task ShowCompanySplash()
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (_companySplashDocument == null || _companySplashRoot == null)
                {
                    Debug.LogWarning("[SplashScreenManager] Company splash screen not set up");
                    tcs.SetResult(false);
                    return tcs.Task;
                }

                _isSkipRequested = false;
                _companySplashDocument.enabled = true;

                // Wait a frame to ensure UI is initialized
                StartCoroutine(InitializeCompanySplash(tcs));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SplashScreenManager] Error showing company splash: {e.Message}");

                // Ensure the document is disabled in case of error
                if (_companySplashDocument != null)
                {
                    _companySplashDocument.enabled = false;
                }

                tcs.SetException(e);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Initialize the company splash screen with animations.
        /// </summary>
        private IEnumerator InitializeCompanySplash(TaskCompletionSource<bool> tcs)
        {
            // Wait a frame to ensure UI is initialized
            yield return null;

            try
            {
                // Register skip input handler
                if (_allowSkipping)
                {
                    RegisterSkipHandler();
                }

                // Get company logo element
                var companyLogo = _companySplashRoot.Q("company-logo");
                var companyName = _companySplashRoot.Q("company-name");

                // Reset initial state
                _companySplashRoot.style.opacity = 1;
                if (companyLogo != null)
                {
                    companyLogo.style.scale = new StyleScale(new Vector3(0.8f, 0.8f, 1f));
                }
                if (companyName != null) companyName.style.opacity = 0;

                // Create animation sequence
                var sequence = new UIAnimationSequence();

                // Add animations
                if (companyLogo != null)
                {
                    sequence.Scale(companyLogo, new Vector2(0.8f, 0.8f), new Vector2(1f, 1f), _fadeTransitionDuration, 0, UIEasing.EaseOutBack);
                }

                if (companyName != null)
                {
                    sequence.Delay(_fadeTransitionDuration * 0.5f)
                            .Fade(companyName, 0, 1, _fadeTransitionDuration, 0, UIEasing.EaseOutCubic);
                }

                // Add wait
                sequence.Delay(_companySplashDuration - _fadeTransitionDuration);

                // Add fade out
                sequence.Fade(_companySplashRoot, 1, 0, _fadeTransitionDuration, 0, UIEasing.EaseInCubic);

                // Add completion callback
                sequence.OnComplete(() =>
                {
                    try
                    {
                        _companySplashDocument.enabled = false;

                        // Unregister skip input handler
                        if (_allowSkipping)
                        {
                            UnregisterSkipHandler();
                        }

                        Debug.Log("[SplashScreenManager] Company splash screen completed");
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SplashScreenManager] Error in company splash completion: {e.Message}");
                        tcs.SetException(e);
                    }
                });

                // Play the sequence
                sequence.Play();

                // Handle skip request
                StartCoroutine(CheckForSkipCompanySplash(sequence, tcs));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SplashScreenManager] Error initializing company splash: {e.Message}");

                // Ensure the document is disabled in case of error
                if (_companySplashDocument != null)
                {
                    _companySplashDocument.enabled = false;
                }

                tcs.SetException(e);
            }
        }

        /// <summary>
        /// Show the game logo splash screen.
        /// </summary>
        public Task ShowGameLogoSplash()
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (_gameLogoSplashDocument == null || _gameLogoSplashRoot == null)
                {
                    Debug.LogWarning("[SplashScreenManager] Game logo splash screen not set up");
                    tcs.SetResult(false);
                    return tcs.Task;
                }

                _isSkipRequested = false;
                _gameLogoSplashDocument.enabled = true;

                // Wait a frame to ensure UI is initialized
                StartCoroutine(InitializeGameLogoSplash(tcs));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SplashScreenManager] Error showing game logo splash: {e.Message}");

                // Ensure the document is disabled in case of error
                if (_gameLogoSplashDocument != null)
                {
                    _gameLogoSplashDocument.enabled = false;
                }

                tcs.SetException(e);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Initialize the game logo splash screen with animations.
        /// </summary>
        private IEnumerator InitializeGameLogoSplash(TaskCompletionSource<bool> tcs)
        {
            // Wait a frame to ensure UI is initialized
            yield return null;

            try
            {
                // Register skip input handler
                if (_allowSkipping)
                {
                    RegisterSkipHandler();
                }

                // Get game logo elements
                var gameLogo = _gameLogoSplashRoot.Q("game-logo");
                var gameTagline = _gameLogoSplashRoot.Q("game-tagline");

                // Reset initial state
                _gameLogoSplashRoot.style.opacity = 1;
                if (gameLogo != null)
                {
                    gameLogo.style.scale = new StyleScale(new Vector3(0.7f, 0.7f, 1f));
                    gameLogo.style.opacity = 0;
                }
                if (gameTagline != null)
                {
                    gameTagline.style.opacity = 0;
                    gameTagline.style.translate = new StyleTranslate(new Translate(0, 30f, 0));
                }

                // Create animation sequence
                var sequence = new UIAnimationSequence();

                // Add animations
                if (gameLogo != null)
                {
                    sequence.Fade(gameLogo, 0, 1, _fadeTransitionDuration, 0, UIEasing.EaseOutCubic)
                            .Scale(gameLogo, new Vector2(0.7f, 0.7f), new Vector2(1f, 1f), _fadeTransitionDuration * 1.2f, 0, UIEasing.EaseOutBack);
                }

                if (gameTagline != null)
                {
                    sequence.Delay(_fadeTransitionDuration * 0.8f)
                            .Fade(gameTagline, 0, 1, _fadeTransitionDuration, 0, UIEasing.EaseOutCubic)
                            .Move(gameTagline,
                                  new Vector2(gameTagline.style.left.value.value, gameTagline.style.top.value.value + 30),
                                  new Vector2(gameTagline.style.left.value.value, gameTagline.style.top.value.value),
                                  _fadeTransitionDuration, 0, UIEasing.EaseOutCubic);
                }

                // Add wait
                sequence.Delay(_gameLogoSplashDuration - _fadeTransitionDuration * 2);

                // Add fade out
                sequence.Fade(_gameLogoSplashRoot, 1, 0, _fadeTransitionDuration, 0, UIEasing.EaseInCubic);

                // Add completion callback
                sequence.OnComplete(() =>
                {
                    try
                    {
                        _gameLogoSplashDocument.enabled = false;

                        // Unregister skip input handler
                        if (_allowSkipping)
                        {
                            UnregisterSkipHandler();
                        }

                        Debug.Log("[SplashScreenManager] Game logo splash screen completed");
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SplashScreenManager] Error in game logo splash completion: {e.Message}");
                        tcs.SetException(e);
                    }
                });

                // Play the sequence
                sequence.Play();

                // Handle skip request
                StartCoroutine(CheckForSkipGameLogoSplash(sequence, tcs));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SplashScreenManager] Error initializing game logo splash: {e.Message}");

                // Ensure the document is disabled in case of error
                if (_gameLogoSplashDocument != null)
                {
                    _gameLogoSplashDocument.enabled = false;
                }

                tcs.SetException(e);
            }
        }

        /// <summary>
        /// Check for skip request during game logo splash animation.
        /// </summary>
        private IEnumerator CheckForSkipGameLogoSplash(UIAnimationSequence sequence, TaskCompletionSource<bool> tcs)
        {
            while (!tcs.Task.IsCompleted)
            {
                if (_isSkipRequested)
                {
                    // Stop current animation sequence
                    sequence.Stop();

                    // Fade out immediately
                    _gameLogoSplashRoot.style.opacity = 0;
                    _gameLogoSplashDocument.enabled = false;

                    // Unregister skip input handler
                    UnregisterSkipHandler();

                    // Complete the task
                    tcs.TrySetResult(true);
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Check for skip request during company splash animation.
        /// </summary>
        private IEnumerator CheckForSkipCompanySplash(UIAnimationSequence sequence, TaskCompletionSource<bool> tcs)
        {
            while (!tcs.Task.IsCompleted)
            {
                if (_isSkipRequested)
                {
                    // Stop current animation sequence
                    sequence.Stop();

                    // Fade out immediately
                    _companySplashRoot.style.opacity = 0;
                    _companySplashDocument.enabled = false;

                    // Unregister skip input handler
                    UnregisterSkipHandler();

                    // Complete the task
                    tcs.TrySetResult(true);
                    yield break;
                }

                yield return null;
            }
        }



        /// <summary>
        /// Register the skip input handler.
        /// </summary>
        private void RegisterSkipHandler()
        {
            try
            {
                // Add mouse click handler
                if (_companySplashDocument != null && _companySplashDocument.enabled && _companySplashDocument.rootVisualElement != null)
                {
                    _companySplashDocument.rootVisualElement.RegisterCallback<ClickEvent>(HandleClickEvent);
                }

                if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.enabled && _gameLogoSplashDocument.rootVisualElement != null)
                {
                    _gameLogoSplashDocument.rootVisualElement.RegisterCallback<ClickEvent>(HandleClickEvent);
                }

                // Add keyboard handler
                if (_companySplashDocument != null && _companySplashDocument.enabled && _companySplashDocument.rootVisualElement != null)
                {
                    _companySplashDocument.rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyEvent);
                }

                if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.enabled && _gameLogoSplashDocument.rootVisualElement != null)
                {
                    _gameLogoSplashDocument.rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyEvent);
                }
            }
            catch (Exception e)
            {
                // Log the error but don't let it crash the application
                Debug.LogWarning($"[SplashScreenManager] Error registering callbacks: {e.Message}");
            }
        }

        /// <summary>
        /// Unregister the skip input handler.
        /// </summary>
        private void UnregisterSkipHandler()
        {
            try
            {
                // Remove mouse click handler
                if (_companySplashDocument != null && _companySplashDocument.rootVisualElement != null)
                {
                    _companySplashDocument.rootVisualElement.UnregisterCallback<ClickEvent>(HandleClickEvent);
                }

                if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.rootVisualElement != null)
                {
                    _gameLogoSplashDocument.rootVisualElement.UnregisterCallback<ClickEvent>(HandleClickEvent);
                }

                // Remove keyboard handler
                if (_companySplashDocument != null && _companySplashDocument.rootVisualElement != null)
                {
                    _companySplashDocument.rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyEvent);
                }

                if (_gameLogoSplashDocument != null && _gameLogoSplashDocument.rootVisualElement != null)
                {
                    _gameLogoSplashDocument.rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyEvent);
                }
            }
            catch (Exception e)
            {
                // Log the error but don't let it crash the application
                Debug.LogWarning($"[SplashScreenManager] Error unregistering callbacks: {e.Message}");
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
        protected override void OnDestroy()
        {
            // Call base class OnDestroy first
            base.OnDestroy();

            // Ensure we unregister any callbacks
            UnregisterSkipHandler();
        }
    }
}