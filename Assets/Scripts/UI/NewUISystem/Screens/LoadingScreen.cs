using System;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Loading screen with progress tracking and tips
    /// Pure C# implementation with programmatic configuration
    /// </summary>
    [UIScreen(UIScreenType.Loading, UIScreenPriority.Loading, "LoadingScreenTemplate")]
    public class LoadingScreen : BaseUIScreen
    {
        #region Configuration Properties

        public float MinLoadingDuration { get; set; } = 2f;
        public float MinShowDuration { get; set; } = 1.5f;
        public float HideDelay { get; set; } = 0.5f;

        #endregion

        #region UI Elements

        private ProgressBar _progressBar;
        private Label _progressText;
        private Label _statusText;
        private Label _versionInfo;

        #endregion

        #region Loading State

        private float _currentProgress;
        private bool _isLoading;
        private float _loadingStartTime;
        private float _showStartTime;
        private bool _waitingForMinShowDuration;

        #endregion

        #region Events

        public event Action OnLoadingComplete;
        public event Action<float> OnProgressChanged;
        public event Action<string> OnStatusChanged;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            ResetLoadingState();
            
            Debug.Log("[LoadingScreen] Initialized with pure C# implementation");
        }

        protected override void OnShow()
        {
            _loadingStartTime = Time.time;
            _showStartTime = Time.time;
            _waitingForMinShowDuration = false;
            UpdateUI();
        }

        protected override void OnHide()
        {
            _isLoading = false;
            OnLoadingComplete?.Invoke();
        }

        public override void Update(float deltaTime)
        {
            if (IsVisible)
            {
                if (_isLoading)
                {
                    CheckMinimumDuration();
                }
                else if (_waitingForMinShowDuration)
                {
                    CheckMinimumShowDuration();
                }
            }
        }

        protected override void OnDispose()
        {
            // Clean up any resources
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _progressBar = GetElement<ProgressBar>("loading-progress");
            _progressText = GetElement<Label>("progress-text");
            _statusText = GetElement<Label>("progress-text"); // Using progress-text for status messages
            _versionInfo = GetElement<Label>("version-info");

            // Set version info from Application
            if (_versionInfo != null)
            {
                _versionInfo.text = $"v{Application.version} (Build {Application.buildGUID.Substring(0, 8)})";
            }

            // Log missing elements for debugging
            if (_progressBar == null) Debug.LogWarning("[LoadingScreen] loading-progress not found in template");
            if (_progressText == null) Debug.LogWarning("[LoadingScreen] progress-text not found in template");
            if (_versionInfo == null) Debug.LogWarning("[LoadingScreen] version-info not found in template");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start the loading process
        /// </summary>
        public void StartLoading()
        {
            // Reset state when explicitly starting a new loading process
            ResetLoadingState();
            _isLoading = true;
            _loadingStartTime = Time.time;
            UpdateProgress("Initializing...", 0f);
        }

        /// <summary>
        /// Update loading progress manually
        /// </summary>
        /// <param name="status">Status message to display (null to keep current)</param>
        /// <param name="progress">Progress value 0-1 (null to keep current)</param>
        public void UpdateProgress(string status = null, float? progress = null)
        {
            // Update progress if provided
            if (progress.HasValue)
            {
                float clampedProgress = Mathf.Clamp01(progress.Value);
                _currentProgress = clampedProgress;

                if (_progressBar != null)
                {
                    _progressBar.value = clampedProgress * 100;
                }

                if (_progressText != null)
                {
                    _progressText.text = $"{Mathf.RoundToInt(clampedProgress * 100)}%";
                }

                OnProgressChanged?.Invoke(clampedProgress);
            }

            // Update status if provided
            if (status != null && _statusText != null)
            {
                _statusText.text = status;
                OnStatusChanged?.Invoke(status);
            }

            // Complete loading if progress reaches 100%
            if (_currentProgress >= 1f && _isLoading)
            {
                CompleteLoading();
            }
        }

        /// <summary>
        /// Update only the status text
        /// </summary>
        public void UpdateStatus(string status)
        {
            UpdateProgress(status: status);
        }

        /// <summary>
        /// Update only the progress value
        /// </summary>
        public void UpdateProgressValue(float progress)
        {
            UpdateProgress(progress: progress);
        }

        /// <summary>
        /// Set minimum loading duration (time from start to completion)
        /// </summary>
        public LoadingScreen SetMinimumDuration(float duration)
        {
            MinLoadingDuration = duration;
            return this;
        }

        /// <summary>
        /// Set minimum show duration (minimum time screen stays visible)
        /// </summary>
        public LoadingScreen SetMinimumShowDuration(float duration)
        {
            MinShowDuration = duration;
            return this;
        }

        /// <summary>
        /// Set delay before hiding after completion
        /// </summary>
        public LoadingScreen SetHideDelay(float delay)
        {
            HideDelay = delay;
            return this;
        }

        /// <summary>
        /// Force complete loading
        /// </summary>
        public void ForceComplete()
        {
            if (_isLoading)
            {
                UpdateProgress("Loading complete!", 1f);
                CompleteLoading();
            }
        }

        /// <summary>
        /// Get current loading progress (0-1)
        /// </summary>
        public float GetProgress() => _currentProgress;

        /// <summary>
        /// Check if currently loading
        /// </summary>
        public bool IsLoading() => _isLoading;

        #endregion

        #region Internal Methods

        private void ResetLoadingState()
        {
            _currentProgress = 0f;
            _isLoading = false;

            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update progress display with current values (don't reset to 0)
            if (_progressBar != null)
            {
                _progressBar.value = _currentProgress * 100;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(_currentProgress * 100)}%";
            }
        }

        private void CheckMinimumDuration()
        {
            if (_currentProgress >= 1f)
            {
                float elapsedTime = Time.time - _loadingStartTime;
                if (elapsedTime >= MinLoadingDuration)
                {
                    CompleteLoading();
                }
            }
        }

        private void CheckMinimumShowDuration()
        {
            float showElapsedTime = Time.time - _showStartTime;
            if (showElapsedTime >= MinShowDuration)
            {
                _waitingForMinShowDuration = false;
                // Auto-hide after configured delay
                UIManager.Instance.StartCoroutine(HideAfterDelay(HideDelay));
            }
        }

        private void CompleteLoading()
        {
            if (!_isLoading) return;

            _isLoading = false;
            UpdateProgress("Loading complete!", 1f);

            // Check if minimum show duration has been met
            float showElapsedTime = Time.time - _showStartTime;
            if (showElapsedTime >= MinShowDuration)
            {
                // Minimum show duration met, hide after delay
                UIManager.Instance.StartCoroutine(HideAfterDelay(HideDelay));
            }
            else
            {
                // Wait for minimum show duration before hiding
                _waitingForMinShowDuration = true;
            }
        }

        private System.Collections.IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide(true);
        }

        #endregion
    }
}