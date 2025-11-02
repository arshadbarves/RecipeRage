using System;
using Core.Animation;
using Core.Bootstrap;
using Core.Logging;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }

    /// <summary>
    /// Toast notification screen - displays temporary messages
    /// Uses BaseUIScreen architecture for consistency
    /// </summary>
    [UIScreen(UIScreenType.Toast, UIScreenPriority.Notification, "Popups/ToastTemplate")]
    public class ToastScreen : BaseUIScreen
    {
        private Label _messageLabel;
        private VisualElement _toastContent;
        private VisualElement _toastContainer;
        private bool _isShowing;

        protected override void OnInitialize()
        {
            _toastContainer = GetElement<VisualElement>("toast-container");
            _toastContent = GetElement<VisualElement>("toast-content");
            _messageLabel = GetElement<Label>("toast-message");

            // Setup toast container - position it off-screen initially for slide animation
            if (_toastContainer != null)
            {
                _toastContainer.pickingMode = PickingMode.Ignore;
                // Start position off-screen (above)
                _toastContainer.style.translate = new Translate(0, new Length(-150, LengthUnit.Pixel));
            }

            // Setup main Container
            if (Container != null)
            {
                // Make container full screen but non-blocking
                Container.style.position = Position.Absolute;
                Container.style.left = 0;
                Container.style.top = 0;
                Container.style.right = 0;
                Container.style.bottom = 0;
                Container.style.width = Length.Percent(100);
                Container.style.height = Length.Percent(100);
                Container.pickingMode = PickingMode.Ignore;

                // Initially hidden
                Container.style.opacity = 1; // Keep container visible
                Container.style.display = DisplayStyle.None;
            }

            GameLogger.Log("Initialized");
        }

        public async UniTask Show(string message, ToastType type = ToastType.Info, float duration = 3f)
        {
            GameLogger.Log($"Show called - Message: '{message}', Type: {type}, Duration: {duration}s");

            if (_isShowing)
            {
                GameLogger.Log("Already showing, hiding previous toast");
                await HideInternal();
            }

            _isShowing = true;

            if (_messageLabel != null)
            {
                _messageLabel.text = message;
                GameLogger.Log($"Message label updated: '{message}'");
            }
            else
            {
                GameLogger.LogWarning("Message label is null!");
            }

            if (_toastContent != null)
            {
                // Remove previous type classes
                _toastContent.RemoveFromClassList("toast-success");
                _toastContent.RemoveFromClassList("toast-error");
                _toastContent.RemoveFromClassList("toast-info");
                _toastContent.RemoveFromClassList("toast-warning");

                // Add type-specific class
                switch (type)
                {
                    case ToastType.Success:
                        _toastContent.AddToClassList("toast-success");
                        break;
                    case ToastType.Error:
                        _toastContent.AddToClassList("toast-error");
                        break;
                    case ToastType.Info:
                        _toastContent.AddToClassList("toast-info");
                        break;
                    case ToastType.Warning:
                        _toastContent.AddToClassList("toast-warning");
                        break;
                }
                GameLogger.Log($"Toast type class added: toast-{type.ToString().ToLower()}");
            }
            else
            {
                GameLogger.LogWarning("Toast content is null!");
            }

            // Reset position before showing
            if (_toastContainer != null)
            {
                _toastContainer.style.translate = new Translate(0, new Length(-150, LengthUnit.Pixel));
                GameLogger.Log("Reset toast position to off-screen");
            }

            // Show the screen (triggers OnShow and animations)
            if (Container != null)
            {
                Container.style.display = DisplayStyle.Flex;
                GameLogger.Log("Container display set to Flex");
            }
            else
            {
                GameLogger.LogError("Container is null!");
            }

            // Small delay to ensure display is set before animation
            await UniTask.Delay(50);

            // Slide in from top
            await SlideInAnimation();

            // Wait for duration
            GameLogger.Log($"Waiting {duration}s before hiding");
            await UniTask.Delay((int)(duration * 1000));

            // Hide
            GameLogger.Log("Hiding toast");
            await HideInternal();
        }

        private async UniTask SlideInAnimation()
        {
            if (_toastContainer == null)
            {
                GameLogger.LogError("Toast container is null!");
                return;
            }

            GameLogger.Log("Starting slide in animation with DOTween");

            // Use DOTween directly for more control
            var tween = DOTween.To(
                () => -150f,
                y => _toastContainer.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)),
                0f,
                0.4f
            ).SetEase(Ease.OutCubic);

            await tween.AsyncWaitForCompletion();
            GameLogger.Log("Slide in animation complete");
        }

        private async UniTask HideInternal()
        {
            if (!_isShowing) return;

            if (_toastContainer == null) return;

            GameLogger.Log("Starting slide out animation with DOTween");

            // Use DOTween directly for slide out
            var tween = DOTween.To(
                () => 0f,
                y => _toastContainer.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)),
                -150f,
                0.3f
            ).SetEase(Ease.InCubic);

            await tween.AsyncWaitForCompletion();
            GameLogger.Log("Slide out animation complete");

            if (Container != null)
            {
                Container.style.display = DisplayStyle.None;
            }

            _isShowing = false;
        }

        protected override void OnShow()
        {
            // Called when screen is shown
        }

        protected override void OnHide()
        {
            // Called when screen is hidden
            _isShowing = false;
        }

        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Custom animation handled in Show() method
            onComplete?.Invoke();
        }

        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Custom animation handled in HideInternal() method
            onComplete?.Invoke();
        }
    }
}
