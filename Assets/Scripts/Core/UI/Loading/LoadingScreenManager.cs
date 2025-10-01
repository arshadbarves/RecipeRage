using System;
using System.Collections;
using Core.Patterns;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Loading
{
    /// <summary>
    /// Manages the loading screen for the game.
    /// </summary>
    public class LoadingScreenManager : MonoBehaviourSingleton<LoadingScreenManager>
    {
        [Header("Loading Screen Settings")]
        [SerializeField] private float _minLoadingScreenDuration = 3.0f;
        [SerializeField] private float _fadeTransitionDuration = 0.5f;

        [Header("UI Documents")]
        [SerializeField] private UIDocument _loadingScreenDocument;

        [Header("Loading Tips")]
        [SerializeField] private string[] _loadingTips;
        [SerializeField] private float _tipChangeDuration = 5.0f;

        /// <summary>
        /// Set the loading screen document.
        /// </summary>
        /// <param name="document">The UI document for the loading screen</param>
        public void SetLoadingScreenDocument(UIDocument document)
        {
            _loadingScreenDocument = document;
        }

        /// <summary>
        /// Set the loading tips.
        /// </summary>
        /// <param name="tips">The loading tips to display</param>
        public void SetLoadingTips(string[] tips)
        {
            _loadingTips = tips;
        }

        private VisualElement _loadingScreenRoot;
        private ProgressBar _loadingProgressBar;
        private Label _loadingStatusLabel;
        private Label _loadingTipLabel;

        private float _loadingStartTime;
        private Coroutine _tipsCoroutine;
        private bool _isVisible;

        /// <summary>
        /// Initialize the loading screen manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize UI elements
            InitializeUIElements();

            // Hide loading screen initially (without showing it first)
            _isVisible = false;
            if (_loadingScreenDocument != null)
            {
                _loadingScreenDocument.enabled = false;
            }

            Debug.Log("[LoadingScreenManager] Initialized");
        }

        /// <summary>
        /// Initialize UI elements from the UI Documents.
        /// </summary>
        private void InitializeUIElements()
        {
            // Loading screen elements
            if (_loadingScreenDocument != null)
            {
                _loadingScreenRoot = _loadingScreenDocument.rootVisualElement.Q("loading-screen-root");
                if (_loadingScreenRoot == null)
                {
                    Debug.LogError("[LoadingScreenManager] Could not find loading-screen-root element");
                }
                else
                {
                    _loadingProgressBar = _loadingScreenRoot.Q<ProgressBar>("loading-progress-bar");
                    _loadingStatusLabel = _loadingScreenRoot.Q<Label>("loading-status-label");
                    _loadingTipLabel = _loadingScreenRoot.Q<Label>("loading-tip-label");

                    if (_loadingProgressBar == null)
                    {
                        Debug.LogError("[LoadingScreenManager] Could not find loading-progress-bar element");
                    }

                    if (_loadingStatusLabel == null)
                    {
                        Debug.LogError("[LoadingScreenManager] Could not find loading-status-label element");
                    }

                    if (_loadingTipLabel == null)
                    {
                        Debug.LogError("[LoadingScreenManager] Could not find loading-tip-label element");
                    }
                }
            }
        }

        /// <summary>
        /// Show the loading screen.
        /// </summary>
        public void ShowLoadingScreen()
        {
            if (_loadingScreenDocument == null || _loadingScreenRoot == null)
            {
                Debug.LogWarning("[LoadingScreenManager] Loading screen not set up");
                return;
            }

            // Mark as visible
            _isVisible = true;

            // Enable the document
            _loadingScreenDocument.enabled = true;

            // Reset progress bar
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = 0f;
            }

            // Set initial status
            if (_loadingStatusLabel != null)
            {
                _loadingStatusLabel.text = "Initializing...";
            }

            // Start cycling tips
            StartCyclingTips();

            // Record start time for minimum duration
            _loadingStartTime = Time.time;

            // Fade in the loading screen using Unity's native system
            _loadingScreenRoot.style.opacity = 0;
            UnityNativeUIAnimationSystem.AnimateOpacity(_loadingScreenRoot, 0, 1, 
                Mathf.RoundToInt(_fadeTransitionDuration * 1000), 0);

            Debug.Log("[LoadingScreenManager] Loading screen shown");
        }

        /// <summary>
        /// Hide the loading screen.
        /// </summary>
        public void HideLoadingScreen()
        {
            if (_loadingScreenDocument == null || _loadingScreenRoot == null || !_isVisible)
            {
                Debug.LogWarning("[LoadingScreenManager] Cannot hide loading screen - not visible or not set up");
                return;
            }

            // Stop cycling tips
            StopCyclingTips();

            // Calculate minimum duration delay
            float elapsedTime = Time.time - _loadingStartTime;
            float remainingTime = Mathf.Max(0, _minLoadingScreenDuration - elapsedTime);
            int delayMs = Mathf.RoundToInt(remainingTime * 1000);

            Debug.Log($"[LoadingScreenManager] Hiding loading screen - elapsed: {elapsedTime:F2}s, remaining: {remainingTime:F2}s, delay: {delayMs}ms");

            // Set progress to 100% first, then fade out
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = 1.0f;
            }

            // Fade out with minimum duration delay
            UnityNativeUIAnimationSystem.AnimateOpacity(_loadingScreenRoot, 1, 0, 
                Mathf.RoundToInt(_fadeTransitionDuration * 1000), delayMs, 
                UnityNativeUIAnimationSystem.EasingCurve.EaseOut, () =>
                {
                    Debug.Log("[LoadingScreenManager] Animation completed - hiding loading screen");
                    _isVisible = false;
                    _loadingScreenDocument.enabled = false;
                    OnLoadingComplete?.Invoke();
                    Debug.Log("[LoadingScreenManager] Loading screen hidden and event fired");
                });
        }

        /// <summary>
        /// Update the loading progress.
        /// </summary>
        /// <param name="status">The status text to display</param>
        /// <param name="progress">The progress value (0-1)</param>
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
        /// Start cycling through loading tips.
        /// </summary>
        private void StartCyclingTips()
        {
            // Stop any existing coroutine
            StopCyclingTips();

            // Start new coroutine if we have tips
            if (_loadingTips != null && _loadingTips.Length > 0 && _loadingTipLabel != null)
            {
                _tipsCoroutine = StartCoroutine(CycleTips());
            }
        }

        /// <summary>
        /// Stop cycling through loading tips.
        /// </summary>
        private void StopCyclingTips()
        {
            if (_tipsCoroutine != null)
            {
                StopCoroutine(_tipsCoroutine);
                _tipsCoroutine = null;
            }
        }

        /// <summary>
        /// Cycle through loading tips.
        /// </summary>
        private IEnumerator CycleTips()
        {
            int tipIndex = 0;

            while (true)
            {
                // Set the tip
                if (_loadingTipLabel != null)
                {
                    _loadingTipLabel.text = _loadingTips[tipIndex];
                }

                // Wait for the duration
                yield return new WaitForSeconds(_tipChangeDuration);

                // Move to the next tip
                tipIndex = (tipIndex + 1) % _loadingTips.Length;
            }
        }
        
        /// <summary>
        /// Event triggered when loading screen completes
        /// </summary>
        public event Action OnLoadingComplete;
    }
}
