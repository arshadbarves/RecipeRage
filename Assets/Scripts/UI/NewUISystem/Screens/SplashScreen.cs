using System;
using Core;
using Core.Animation;
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
            if (_splashCoroutine != null && UIServiceAccessor.Instance != null)
            {
                UIServiceAccessor.StopCoroutine(_splashCoroutine);
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

            AnimateSplashSequence();
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

        private void AnimateSplashSequence()
        {
            // Fade in splash content
            if (_splashContent != null)
            {
                var animator = new DOTweenUIAnimator();
                animator.FadeIn(_splashContent, FadeInDuration, () =>
                {
                    float holdTime = SplashDuration - FadeInDuration - FadeOutDuration;

                    // Wait for hold time, then fade out
                    if (_splashContent != null)
                    {
                        // Use DOTween's delay
                        DG.Tweening.DOVirtual.DelayedCall(holdTime, () =>
                        {
                            animator.FadeOut(_splashContent, FadeOutDuration, CompleteSplash);
                        });
                    }
                });
            }
        }

        private void StopSplashSequence()
        {
            _isPlayingSplash = false;
            if (_splashCoroutine == null || UIServiceAccessor.Instance == null)
            {
                return;
            }
            UIServiceAccessor.StopCoroutine(_splashCoroutine);
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