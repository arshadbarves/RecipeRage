using System;
using System.Collections;
using System.Threading.Tasks;
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

        /// <summary>
        /// Initialize the loading screen manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize UI elements
            InitializeUIElements();

            // Hide loading screen initially
            HideLoadingScreen();

            Debug.Log("[LoadingScreenManager] Initialized");
        }

        /// <summary>
        /// Initialize UI elements from the UI Documents.
        /// </summary>
        private void InitializeUIElements()
        {
            try
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
            catch (Exception e)
            {
                Debug.LogError($"[LoadingScreenManager] Error initializing UI elements: {e.Message}");
            }
        }

        /// <summary>
        /// Show the loading screen.
        /// </summary>
        public void ShowLoadingScreen()
        {
            try
            {
                if (_loadingScreenDocument == null || _loadingScreenRoot == null)
                {
                    Debug.LogWarning("[LoadingScreenManager] Loading screen not set up");
                    return;
                }

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

                // Fade in the loading screen
                _loadingScreenRoot.style.opacity = 0;
                UIAnimationSystem.Instance.Fade(_loadingScreenRoot, 0, 1, _fadeTransitionDuration, 0, UIEasing.EaseOutCubic);

                Debug.Log("[LoadingScreenManager] Loading screen shown");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadingScreenManager] Error showing loading screen: {e.Message}");
            }
        }

        /// <summary>
        /// Hide the loading screen.
        /// </summary>
        public Task HideLoadingScreen()
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (_loadingScreenDocument == null || _loadingScreenRoot == null)
                {
                    Debug.LogWarning("[LoadingScreenManager] Loading screen not set up");
                    tcs.SetResult(false);
                    return tcs.Task;
                }

                // Stop cycling tips
                StopCyclingTips();

                // Check if we need to wait for minimum duration
                float elapsedTime = Time.time - _loadingStartTime;
                float remainingTime = Mathf.Max(0, _minLoadingScreenDuration - elapsedTime);

                // Wait for minimum duration and then hide
                StartCoroutine(HideLoadingScreenAfterDelay(remainingTime, tcs));
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadingScreenManager] Error hiding loading screen: {e.Message}");
                tcs.SetException(e);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Hide the loading screen after a delay.
        /// </summary>
        private IEnumerator HideLoadingScreenAfterDelay(float delay, TaskCompletionSource<bool> tcs)
        {
            // Wait for the delay
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // Set progress to 100% for visual satisfaction using the enhanced animation system
            if (_loadingProgressBar != null)
            {
                // Get the current progress value
                float startValue = _loadingProgressBar.value;
                
                // Create a task completion source to wait for the animation to complete
                TaskCompletionSource<bool> animationTask = new TaskCompletionSource<bool>();
                
                // Start the animation in a separate coroutine
                StartCoroutine(AnimateProgressBarValue(startValue, 1.0f, 0.5f, Core.UI.Animation.UIEasing.EaseOutCubic, animationTask));
                
                // Wait for the animation to complete outside the try-catch
                while (!animationTask.Task.IsCompleted)
                {
                    yield return null;
                }
            }

            // Fade out the loading screen
            UIAnimationSystem.Instance.Fade(_loadingScreenRoot, 1, 0, _fadeTransitionDuration, 0, UIEasing.EaseInCubic, () =>
            {
                try
                {
                    // Disable the document
                    _loadingScreenDocument.enabled = false;

                    Debug.Log("[LoadingScreenManager] Loading screen hidden");
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LoadingScreenManager] Error in loading screen fade completion: {e.Message}");
                    tcs.SetException(e);
                }
            });
        }

        /// <summary>
        /// Update the loading progress.
        /// </summary>
        /// <param name="status">The status text to display</param>
        /// <param name="progress">The progress value (0-1)</param>
        public void UpdateLoadingProgress(string status, float progress)
        {
            try
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
            catch (Exception e)
            {
                Debug.LogError($"[LoadingScreenManager] Error updating loading progress: {e.Message}");
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
                    try
                    {
                        _loadingTipLabel.text = _loadingTips[tipIndex];
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[LoadingScreenManager] Error setting tip: {e.Message}");
                    }
                }

                // Wait for the duration
                yield return new WaitForSeconds(_tipChangeDuration);

                // Move to the next tip
                tipIndex = (tipIndex + 1) % _loadingTips.Length;
            }
        }
        
        /// <summary>
        /// Animate the progress bar value from start to end
        /// </summary>
        private IEnumerator AnimateProgressBarValue(float startValue, float endValue, float duration, 
            Core.UI.Animation.EasingFunction easing, TaskCompletionSource<bool> completionSource)
        {
            float startTime = Time.time;
            float endTime = startTime + duration;
            bool isRunning = true;
            
            while (isRunning && Time.time < endTime)
            {
                try
                {
                    float elapsed = Time.time - startTime;
                    float normalizedTime = Mathf.Clamp01(elapsed / duration);

                    // Apply easing if provided
                    if (easing != null)
                    {
                        normalizedTime = easing(normalizedTime);
                    }

                    // Interpolate value
                    float currentValue = Mathf.Lerp(startValue, endValue, normalizedTime);

                    // Update progress bar
                    if (_loadingProgressBar != null)
                    {
                        _loadingProgressBar.value = currentValue;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LoadingScreenManager] Error in progress bar animation: {e.Message}");
                    isRunning = false;
                    
                    // Try to set the final value even if there was an error
                    try
                    {
                        if (_loadingProgressBar != null)
                        {
                            _loadingProgressBar.value = endValue;
                        }
                    }
                    catch { }
                    
                    // Signal completion with error
                    completionSource.SetException(e);
                    yield break;
                }
                
                yield return null;
            }
            
            try
            {
                // Ensure final value is set
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.value = endValue;
                }
                
                // Signal completion
                completionSource.SetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadingScreenManager] Error setting final progress value: {e.Message}");
                completionSource.SetException(e);
            }
        }
    }
}
