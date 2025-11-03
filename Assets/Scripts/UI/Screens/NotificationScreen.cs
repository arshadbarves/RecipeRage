using System;
using System.Collections.Generic;
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
    public enum NotificationType
    {
        Success,
        Error,
        Info,
        Warning
    }

    /// <summary>
    /// Notification data for queue
    /// </summary>
    public class NotificationData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public float Duration { get; set; }
    }

    /// <summary>
    /// Notification screen - displays auto-closing, non-interactable messages
    /// Uses BaseUIScreen animation system with custom slide animation
    /// Supports queuing multiple notifications
    /// </summary>
    [UIScreen(UIScreenType.Notification, UIScreenCategory.Popup, "Popups/NotificationTemplate")]
    public class NotificationScreen : BaseUIScreen
    {
        private Label _titleLabel;
        private Label _messageLabel;
        private VisualElement _notificationContent;
        private VisualElement _notificationIcon;
        private bool _isShowing;
        private Tween _currentTween;
        private readonly Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();
        private bool _isProcessingQueue;

        protected override void OnInitialize()
        {
            _notificationContent = GetElement<VisualElement>("notification-content");
            _notificationIcon = GetElement<VisualElement>("notification-icon");
            _titleLabel = GetElement<Label>("notification-title");
            _messageLabel = GetElement<Label>("notification-message");

            // Setup notification content - non-interactable
            if (_notificationContent != null)
            {
                _notificationContent.pickingMode = PickingMode.Ignore;
            }

            // Setup main Container - non-interactable overlay
            if (Container != null)
            {
                Container.pickingMode = PickingMode.Ignore;
            }

            GameLogger.Log("NotificationScreen initialized");
        }

        public async UniTask Show(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            await Show(GetDefaultTitle(type), message, type, duration);
        }

        public async UniTask Show(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            // Add to queue
            _notificationQueue.Enqueue(new NotificationData
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = duration
            });

            // Start processing if not already processing
            if (!_isProcessingQueue)
            {
                ProcessQueue().Forget();
            }
        }

        private async UniTaskVoid ProcessQueue()
        {
            _isProcessingQueue = true;

            while (_notificationQueue.Count > 0)
            {
                var notification = _notificationQueue.Dequeue();
                await ShowNotification(notification);
            }

            _isProcessingQueue = false;
        }

        private async UniTask ShowNotification(NotificationData notification)
        {
            GameLogger.Log($"Show notification - Title: '{notification.Title}', Message: '{notification.Message}', Type: {notification.Type}, Duration: {notification.Duration}s");

            _isShowing = true;

            // Update content
            UpdateContent(notification.Title, notification.Message, notification.Type);

            // Use BaseUIScreen's Show method with animation
            base.Show(animate: true, addToHistory: false);

            // Wait for show animation to complete
            await UniTask.WaitUntil(() => !IsAnimating);

            // Wait for duration
            await UniTask.Delay((int)(notification.Duration * 1000));

            // Hide using BaseUIScreen's Hide method
            Hide(animate: true);

            // Wait for hide animation to complete
            await UniTask.WaitUntil(() => !IsAnimating);

            _isShowing = false;
        }

        private void UpdateContent(string title, string message, NotificationType type)
        {
            // Update title
            if (_titleLabel != null)
            {
                _titleLabel.text = title;
            }

            // Update message
            if (_messageLabel != null)
            {
                _messageLabel.text = message;
            }

            // Update icon and styling based on type
            if (_notificationIcon != null && _notificationContent != null)
            {
                // Remove previous type classes
                _notificationContent.RemoveFromClassList("notification-success");
                _notificationContent.RemoveFromClassList("notification-error");
                _notificationContent.RemoveFromClassList("notification-info");
                _notificationContent.RemoveFromClassList("notification-warning");

                // Add type-specific class and update icon color
                switch (type)
                {
                    case NotificationType.Success:
                        _notificationContent.AddToClassList("notification-success");
                        _notificationIcon.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                        break;
                    case NotificationType.Error:
                        _notificationContent.AddToClassList("notification-error");
                        _notificationIcon.style.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
                        break;
                    case NotificationType.Info:
                        _notificationContent.AddToClassList("notification-info");
                        _notificationIcon.style.backgroundColor = new Color(0.4f, 0.6f, 1f);
                        break;
                    case NotificationType.Warning:
                        _notificationContent.AddToClassList("notification-warning");
                        _notificationIcon.style.backgroundColor = new Color(1f, 0.7f, 0.2f);
                        break;
                }
            }
        }

        private string GetDefaultTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "Success",
                NotificationType.Error => "Error",
                NotificationType.Info => "Info",
                NotificationType.Warning => "Warning",
                _ => "Notification"
            };
        }

        protected override void OnShow()
        {
            // Called when screen is shown via BaseUIScreen
        }

        protected override void OnHide()
        {
            // Called when screen is hidden via BaseUIScreen
            _isShowing = false;
            
            // Kill any ongoing tweens
            _currentTween?.Kill();
        }

        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            if (_notificationContent == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Reset position off-screen
            _notificationContent.style.translate = new Translate(0, new Length(-200, LengthUnit.Pixel));

            // Slide in from top with bounce
            _currentTween = DOTween.To(
                () => -200f,
                y => _notificationContent.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)),
                20f, // Slide to 20px from top
                0.5f
            ).SetEase(Ease.OutBack)
            .OnComplete(() => onComplete?.Invoke());
        }

        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            if (_notificationContent == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Slide out to top
            _currentTween = DOTween.To(
                () => 20f,
                y => _notificationContent.style.translate = new Translate(0, new Length(y, LengthUnit.Pixel)),
                -200f,
                0.4f
            ).SetEase(Ease.InCubic)
            .OnComplete(() => onComplete?.Invoke());
        }

        protected override void OnDispose()
        {
            _currentTween?.Kill();
        }
    }
}
