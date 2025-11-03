using System;
using Core.Bootstrap;
using Core.Logging;
using Cysharp.Threading.Tasks;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Splash screen - quick brand display on startup
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenCategory.System, "Screens/SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {
        #region UI Elements

        private VisualElement _splashContent;
        private Label _companyName;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            GameLogger.Log("Initialized");
        }

        protected override void OnShow()
        {
            // Fade in splash content
            if (_splashContent != null)
            {
                var animationService = GameBootstrap.Services?.AnimationService;
                if (animationService != null)
                {
                    animationService.UI.FadeIn(_splashContent, 0.5f);
                }
            }
        }

        protected override void OnHide()
        {
            // Fade out
            if (Container != null)
            {
                var animationService = GameBootstrap.Services?.AnimationService;
                if (animationService != null)
                {
                    animationService.UI.FadeOut(Container, 0.3f);
                }
            }
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _splashContent = GetElement<VisualElement>("splash-content");
            _companyName = GetElement<Label>("company-name");

            if (_splashContent == null)
            {
                GameLogger.LogWarning("splash-content not found");
            }
            if (_companyName == null)
            {
                GameLogger.LogWarning("company-name not found");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show splash for specified duration then hide
        /// </summary>
        public async UniTask ShowForDurationAsync(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            Hide(true);
            await UniTask.Delay(300); // Wait for fade out
        }

        #endregion
    }
}
