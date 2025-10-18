using System;
using Core.Utilities;
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
        private Label _versionInfo;

        #endregion

        #region Loading State

        private enum LoadingState
        {
            Idle,
            Loading,
            WaitingForMinDuration
        }

        private float _currentProgress;
        private LoadingState _loadingState;
        private float _loadingStartTime;
        private float _showStartTime;
        private Coroutine _hideDelayCoroutine;

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

            // Set version info when shown (not during initialization to avoid threading issues)
            if (_versionInfo != null)
            {
                _versionInfo.text = $"v{Application.version} (Build {Application.buildGUID.Substring(0, 8)})";
            }

            UpdateUI();
        }

        protected override void OnHide()
        {
            _loadingState = LoadingState.Idle;
            if (_hideDelayCoroutine != null)
            {
                CoroutineRunner.Stop(_hideDelayCoroutine);
                _hideDelayCoroutine = null;
            }
            OnLoadingComplete?.Invoke();
        }

        public override void Update(float deltaTime)
        {
            if (IsVisible)
            {
                if (_loadingState == LoadingState.Loading)
                {
                    CheckMinimumDuration();
                }
                else if (_loadingState == LoadingState.WaitingForMinDuration)
                {
                    CheckMinimumShowDuration();
                }
            }
        }

        protected override void OnDispose()
        {
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _progressBar = GetElement<ProgressBar>("loading-progress");
            _progressText = GetElement<Label>("progress-text");
            _versionInfo = GetElement<Label>("version-info");

            // Log missing elements for debugging
            if (_progressBar == null)
            {
                Debug.LogWarning("[LoadingScreen] loading-progress not found in template");
            }
            if (_progressText == null)
            {
                Debug.LogWarning("[LoadingScreen] progress-text not found in template");
            }
            if (_versionInfo == null)
            {
                Debug.LogWarning("[LoadingScreen] version-info not found in template");
            }
        }

        #endregion

        #region Public API

        public void StartLoading()
        {
            ResetLoadingState();
            _loadingState = LoadingState.Loading;
            _loadingStartTime = Time.time;
            UpdateProgress("Initializing...", 0f);
        }

        public void UpdateProgress(string status = null, float? progress = null)
        {
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

            if (status != null && _progressText != null)
            {
                _progressText.text = status;
                OnStatusChanged?.Invoke(status);
            }

            if (_currentProgress >= 1f && _loadingState == LoadingState.Loading)
            {
                CompleteLoading();
            }
        }

        public void UpdateStatus(string status)
        {
            UpdateProgress(status: status);
        }

        public void UpdateProgressValue(float progress)
        {
            UpdateProgress(progress: progress);
        }

        public LoadingScreen SetMinimumDuration(float duration)
        {
            MinLoadingDuration = duration;
            return this;
        }

        public LoadingScreen SetMinimumShowDuration(float duration)
        {
            MinShowDuration = duration;
            return this;
        }

        public LoadingScreen SetHideDelay(float delay)
        {
            HideDelay = delay;
            return this;
        }

        public void ForceComplete()
        {
            if (_loadingState == LoadingState.Loading)
            {
                UpdateProgress("Loading complete!", 1f);
                CompleteLoading();
            }
        }

        public float GetProgress() => _currentProgress;
        public bool IsLoading() => _loadingState == LoadingState.Loading;

        #endregion

        #region Internal Methods

        private void ResetLoadingState()
        {
            _currentProgress = 0f;
            _loadingState = LoadingState.Idle;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_progressBar != null) _progressBar.value = _currentProgress * 100;
            if (_progressText != null) _progressText.text = $"{Mathf.RoundToInt(_currentProgress * 100)}%";
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
                _loadingState = LoadingState.Idle;
                _hideDelayCoroutine = CoroutineRunner.Run(HideAfterDelay(HideDelay));
            }
        }

        private void CompleteLoading()
        {
            if (_loadingState != LoadingState.Loading) return;

            _loadingState = LoadingState.Idle;
            UpdateProgress("Loading complete!", 1f);

            float showElapsedTime = Time.time - _showStartTime;
            if (showElapsedTime >= MinShowDuration)
            {
                _hideDelayCoroutine = CoroutineRunner.Run(HideAfterDelay(HideDelay));
            }
            else
            {
                _loadingState = LoadingState.WaitingForMinDuration;
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