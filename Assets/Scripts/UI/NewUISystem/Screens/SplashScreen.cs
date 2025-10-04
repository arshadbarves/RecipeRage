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

        private Label _companyNameLabel;

        #endregion

        #region Splash State

        private bool _isPlayingSplash;
        private float _splashTimer;
        private SplashPhase _currentPhase;

        private enum SplashPhase
        {
            FadeIn,
            Hold,
            FadeOut,
            Complete
        }

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

        public override void Update(float deltaTime)
        {
            if (IsVisible && _isPlayingSplash)
            {
                UpdateSplashSequence(deltaTime);
            }
        }

        protected override void OnDispose()
        {
            // Clean up resources
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _companyNameLabel = GetElement<Label>("company-name");

            // Log missing elements for debugging
            if (_companyNameLabel == null) Debug.LogWarning("[SplashScreen] company-name not found in template");
        }

        private void SetInitialValues()
        {
            if (_companyNameLabel != null)
                _companyNameLabel.text = CompanyName;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the company name
        /// </summary>
        public SplashScreen SetCompanyName(string companyName)
        {
            CompanyName = companyName;
            if (_companyNameLabel != null)
                _companyNameLabel.text = companyName;
            return this;
        }

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
        public SplashScreen SetFadeDurations(float fadeIn, float fadeOut)
        {
            FadeInDuration = Mathf.Max(0.1f, fadeIn);
            FadeOutDuration = Mathf.Max(0.1f, fadeOut);
            return this;
        }



        /// <summary>
        /// Play the splash sequence
        /// </summary>
        public void PlaySplashSequence()
        {
            if (_isPlayingSplash) return;

            _isPlayingSplash = true;
            _splashTimer = 0f;
            _currentPhase = SplashPhase.FadeIn;

            PrepareForSplash();
        }



        /// <summary>
        /// Check if splash is currently playing
        /// </summary>
        public bool IsPlayingSplash() => _isPlayingSplash;

        #endregion

        #region Internal Methods

        private void UpdateUI()
        {
            if (_companyNameLabel != null) _companyNameLabel.text = CompanyName;
        }

        private void PrepareForSplash()
        {
            // Reset company name label for animation
            if (_companyNameLabel != null)
                _companyNameLabel.style.opacity = 0;
        }

        private void UpdateSplashSequence(float deltaTime)
        {
            _splashTimer += deltaTime;

            switch (_currentPhase)
            {
                case SplashPhase.FadeIn:
                    UpdateFadeInPhase();
                    break;
                case SplashPhase.Hold:
                    UpdateHoldPhase();
                    break;
                case SplashPhase.FadeOut:
                    UpdateFadeOutPhase();
                    break;
                case SplashPhase.Complete:
                    CompleteSplash();
                    break;
            }
        }

        private void UpdateFadeInPhase()
        {
            float fadeProgress = _splashTimer / FadeInDuration;
            
            if (fadeProgress >= 1f)
            {
                // Fade in complete
                SetElementsOpacity(1f);
                _currentPhase = SplashPhase.Hold;
                _splashTimer = 0f;
            }
            else
            {
                // Animate fade in
                SetElementsOpacity(fadeProgress);
            }
        }

        private void UpdateHoldPhase()
        {
            float holdDuration = SplashDuration - FadeInDuration - FadeOutDuration;
            
            if (_splashTimer >= holdDuration)
            {
                _currentPhase = SplashPhase.FadeOut;
                _splashTimer = 0f;
            }
        }

        private void UpdateFadeOutPhase()
        {
            float fadeProgress = _splashTimer / FadeOutDuration;
            
            if (fadeProgress >= 1f)
            {
                // Fade out complete
                SetElementsOpacity(0f);
                _currentPhase = SplashPhase.Complete;
            }
            else
            {
                // Animate fade out
                SetElementsOpacity(1f - fadeProgress);
            }
        }

        private void SetElementsOpacity(float opacity)
        {
            if (_companyNameLabel != null)
                _companyNameLabel.style.opacity = opacity;
        }



        private void StopSplashSequence()
        {
            _isPlayingSplash = false;
            _currentPhase = SplashPhase.Complete;
        }

        private void CompleteSplash()
        {
            _isPlayingSplash = false;
            OnSplashComplete?.Invoke();

            // Auto-hide after completion
            Hide(true);
        }

        #endregion

        #region Animation Methods (Alternative to manual updates)

        /// <summary>
        /// Play splash using Unity's animation system instead of manual updates
        /// </summary>
        public SplashScreen PlaySplashWithAnimations()
        {
            if (_isPlayingSplash) return this;

            _isPlayingSplash = true;
            PrepareForSplash();

            // Fade in company name
            if (_companyNameLabel != null)
            {
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _companyNameLabel,
                    0f, 1f,
                    FadeInDuration, 0f,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseOut,
                    () => {
                        // Hold for duration, then fade out
                        float holdTime = SplashDuration - FadeInDuration - FadeOutDuration;
                        UIManager.Instance.StartCoroutine(FadeOutAfterDelay(holdTime));
                    }
                );
            }

            return this;
        }

        private System.Collections.IEnumerator FadeOutAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Fade out company name
            if (_companyNameLabel != null)
            {
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _companyNameLabel, 1f, 0f, FadeOutDuration, 0f,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseIn,
                    () => CompleteSplash()
                );
            }
        }

        #endregion
    }
}