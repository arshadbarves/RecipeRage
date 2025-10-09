using System;
using Core;
using Core.UI.Animation;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Splash screen for company branding and game logo
    /// Pure C# implementation with programmatic configuration
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenPriority.Splash, "SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {
        #region Configuration Properties

        public string CompanyName { get; set; } = GameConstants.COMPANY_DISPLAY;
        public float SplashDuration { get; set; } = 3f;
        public float FadeInDuration { get; set; } = 0.8f;
        public float FadeOutDuration { get; set; } = 0.5f;

        #endregion

        #region UI Elements

        private VisualElement _splashContent;
        private Label _companyNameLabel;

        #endregion

        #region Splash State

        private bool _isPlayingSplash;
        private Coroutine _splashCoroutine;

        #endregion

        #region Events

        public event Action OnSplashComplete;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetInitialValues();

            Debug.Log("[SplashScreen] Initialized with pure C# implementation");
        }

        protected override void OnShow()
        {
            UpdateUI();

            // Auto-start splash sequence
            PlaySplashSequence();
        }

        protected override void OnHide()
        {
            StopSplashSequence();
        }

        protected override void OnDispose()
        {
            // Clean up resources
            if (_splashCoroutine != null && UIManager.Instance != null)
            {
                UIManager.Instance.StopCoroutine(_splashCoroutine);
                _splashCoroutine = null;
            }
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _splashContent = GetElement<VisualElement>("splash-content");
            _companyNameLabel = GetElement<Label>("company-name");

            // Log missing elements for debugging
            if (_splashContent == null)
            {
                Debug.LogWarning("[SplashScreen] splash-content not found in template");
            }
            if (_companyNameLabel == null)
            {
                Debug.LogWarning("[SplashScreen] company-name not found in template");
            }
        }

        private void SetInitialValues()
        {
            if (_companyNameLabel != null)
            {
                _companyNameLabel.text = CompanyName;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set splash duration
        /// </summary>
        public SplashScreen SetSplashDuration(float duration)
        {
            SplashDuration = Mathf.Max(0.5f, duration);
            return this;
        }

        /// <summary>
        /// Set fade durations
        /// </summary>
        public void SetFadeDurations(float fadeIn, float fadeOut)
        {
            FadeInDuration = Mathf.Max(0.1f, fadeIn);
            FadeOutDuration = Mathf.Max(0.1f, fadeOut);
        }


        /// <summary>
        /// Play the splash sequence using automatic animations
        /// </summary>
        private void PlaySplashSequence()
        {
            if (_isPlayingSplash)
            {
                return;
            }

            _isPlayingSplash = true;
            PrepareForSplash();

            // Start automatic animation sequence
            _splashCoroutine = UIManager.Instance.StartCoroutine(AnimateSplashSequence());
        }

        #endregion

        #region Internal Methods

        private void UpdateUI()
        {
            if (_companyNameLabel != null)
            {
                _companyNameLabel.text = CompanyName;
            }
        }

        private void PrepareForSplash()
        {
            // Reset splash content for animation
            if (_splashContent != null)
            {
                _splashContent.style.opacity = 0;
            }
        }

        private System.Collections.IEnumerator AnimateSplashSequence()
        {
            // Fade in splash content
            if (_splashContent != null)
            {
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _splashContent,
                    0f, 1f,
                    FadeInDuration, 0f,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseOut,
                    null
                );
            }

            // Wait for fade in to complete
            yield return new WaitForSeconds(FadeInDuration);

            // Hold for duration
            float holdTime = SplashDuration - FadeInDuration - FadeOutDuration;
            yield return new WaitForSeconds(holdTime);

            // Fade out splash content
            if (_splashContent != null)
            {
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _splashContent,
                    1f, 0f,
                    FadeOutDuration, 0f,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseIn,
                    null
                );
            }

            // Wait for fade out to complete
            yield return new WaitForSeconds(FadeOutDuration);

            // Complete splash
            CompleteSplash();
        }

        private void StopSplashSequence()
        {
            _isPlayingSplash = false;
            if (_splashCoroutine == null || UIManager.Instance == null)
            {
                return;
            }
            UIManager.Instance.StopCoroutine(_splashCoroutine);
            _splashCoroutine = null;
        }

        private void CompleteSplash()
        {
            _isPlayingSplash = false;
            OnSplashComplete?.Invoke();

            // Auto-hide after completion
            Hide(true);
        }

        #endregion
    }
}