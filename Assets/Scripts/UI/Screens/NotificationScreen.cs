using System;
using System.Collections;
using Core.Animation;
using Core.Bootstrap;
using Core.Utilities;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Notification screen for temporary messages and alerts
    /// Demonstrates custom animation behavior with auto-hide functionality
    /// </summary>
    [UIScreen(UIScreenType.Notification, UIScreenPriority.Notification, "NotificationTemplate")]
    public class NotificationScreen : BaseUIScreen
    {
        #region Configuration Properties

        public string Message { get; set; } = "Notification message";
        public NotificationType Type { get; set; } = NotificationType.Info;
        public float AutoHideDelay { get; set; } = 3.0f;
        public bool AutoHide { get; set; } = true;

        #endregion

        #region UI Elements

        private Label _messageLabel;
        private VisualElement _iconElement;
        private Button _closeButton;
        private VisualElement _progressBar;

        #endregion

        #region Events

        public event Action<NotificationScreen> OnNotificationClosed;
        public event Action<NotificationScreen> OnNotificationExpired;

        #endregion

        #region Private Fields

        private Coroutine _autoHideCoroutine;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
        }

        protected override void OnShow()
        {
            UpdateUI();
            StartAutoHideTimer();
        }

        protected override void OnHide()
        {
            StopAutoHideTimer();
        }

        protected override void OnDispose()
        {
            StopAutoHideTimer();
            UnregisterEventHandlers();
        }

        #endregion

        #region Animation Customization

        /// <summary>
        /// Notifications slide in from the top for immediate attention
        /// </summary>
        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideIn(element, SlideDirection.Top, duration, onComplete);
        }

        /// <summary>
        /// Notifications slide out to the top when dismissed
        /// </summary>
        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideOut(element, SlideDirection.Top, duration, onComplete);
        }

        /// <summary>
        /// Notifications use fast animations for snappy feedback
        /// </summary>
        public override float GetAnimationDuration()
        {
            return 0.2f; // Very fast for immediate feedback
        }

        /// <summary>
        /// Prepare notification styling based on type before showing
        /// </summary>
        public override void OnBeforeShowAnimation()
        {
            ApplyNotificationStyling();
        }

        /// <summary>
        /// Start auto-hide timer after animation completes
        /// </summary>
        public override void OnAfterShowAnimation()
        {
            if (AutoHide)
            {
                StartAutoHideTimer();
            }
        }

        /// <summary>
        /// Stop any running timers before hiding
        /// </summary>
        public override void OnBeforeHideAnimation()
        {
            StopAutoHideTimer();
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _messageLabel = GetElement<Label>("notification-message");
            _iconElement = GetElement<VisualElement>("notification-icon");
            _closeButton = GetElement<Button>("close-button");
            _progressBar = GetElement<VisualElement>("progress-bar");
        }

        private void SetupEventHandlers()
        {
            _closeButton?.RegisterCallback<ClickEvent>(_ => HandleCloseClicked());
        }

        private void UnregisterEventHandlers()
        {
            _closeButton?.UnregisterCallback<ClickEvent>(_ => HandleCloseClicked());
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the notification message
        /// </summary>
        public NotificationScreen SetMessage(string message)
        {
            Message = message;
            if (_messageLabel != null)
            {
                _messageLabel.text = message;
            }
            return this;
        }

        /// <summary>
        /// Set the notification type (affects styling and icon)
        /// </summary>
        public NotificationScreen SetType(NotificationType type)
        {
            Type = type;
            ApplyNotificationStyling();
            return this;
        }

        /// <summary>
        /// Set auto-hide behavior
        /// </summary>
        public NotificationScreen SetAutoHide(bool autoHide, float delay = 3.0f)
        {
            AutoHide = autoHide;
            AutoHideDelay = delay;
            return this;
        }

        /// <summary>
        /// Disable auto-hide (notification stays until manually closed)
        /// </summary>
        public NotificationScreen DisableAutoHide()
        {
            AutoHide = false;
            StopAutoHideTimer();
            return this;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Show a quick info notification
        /// </summary>
        public static NotificationScreen ShowInfo(string message, float duration = 3.0f)
        {
            NotificationScreen notification = GameBootstrap.Services?.UIService.GetScreen<NotificationScreen>();
            return notification?
                .SetMessage(message)
                .SetType(NotificationType.Info)
                .SetAutoHide(true, duration);
        }

        /// <summary>
        /// Show a success notification
        /// </summary>
        public static NotificationScreen ShowSuccess(string message, float duration = 2.5f)
        {
            NotificationScreen notification = GameBootstrap.Services?.UIService.GetScreen<NotificationScreen>();
            return notification?
                .SetMessage(message)
                .SetType(NotificationType.Success)
                .SetAutoHide(true, duration);
        }

        /// <summary>
        /// Show a warning notification
        /// </summary>
        public static NotificationScreen ShowWarning(string message, float duration = 4.0f)
        {
            NotificationScreen notification = GameBootstrap.Services?.UIService.GetScreen<NotificationScreen>();
            return notification?
                .SetMessage(message)
                .SetType(NotificationType.Warning)
                .SetAutoHide(true, duration);
        }

        /// <summary>
        /// Show an error notification (no auto-hide by default)
        /// </summary>
        public static NotificationScreen ShowError(string message, bool autoHide = false)
        {
            NotificationScreen notification = GameBootstrap.Services?.UIService.GetScreen<NotificationScreen>();
            return notification?
                .SetMessage(message)
                .SetType(NotificationType.Error)
                .SetAutoHide(autoHide, 5.0f);
        }

        #endregion

        #region Internal Methods

        private void UpdateUI()
        {
            if (_messageLabel != null)
            {
                _messageLabel.text = Message;
            }
            ApplyNotificationStyling();
        }

        private void ApplyNotificationStyling()
        {
            if (Container == null)
            {
                return;
            }

            // Remove existing type classes
            Container.RemoveFromClassList("notification--info");
            Container.RemoveFromClassList("notification--success");
            Container.RemoveFromClassList("notification--warning");
            Container.RemoveFromClassList("notification--error");

            // Add appropriate class based on type
            string typeClass = Type switch
            {
                NotificationType.Info => "notification--info",
                NotificationType.Success => "notification--success",
                NotificationType.Warning => "notification--warning",
                NotificationType.Error => "notification--error",
                _ => "notification--info"
            };

            Container.AddToClassList(typeClass);

            // Update icon if present
            if (_iconElement != null)
            {
                _iconElement.RemoveFromClassList("icon--info");
                _iconElement.RemoveFromClassList("icon--success");
                _iconElement.RemoveFromClassList("icon--warning");
                _iconElement.RemoveFromClassList("icon--error");

                string iconClass = Type switch
                {
                    NotificationType.Info => "icon--info",
                    NotificationType.Success => "icon--success",
                    NotificationType.Warning => "icon--warning",
                    NotificationType.Error => "icon--error",
                    _ => "icon--info"
                };

                _iconElement.AddToClassList(iconClass);
            }
        }

        private void StartAutoHideTimer()
        {
            if (!AutoHide)
            {
                return;
            }

            StopAutoHideTimer();
            
            // Start coroutine through CoroutineRunner (since this is not a MonoBehaviour)
            _autoHideCoroutine = CoroutineRunner.Run(AutoHideCoroutine());
        }

        private void StopAutoHideTimer()
        {
            if (_autoHideCoroutine != null)
            {
                CoroutineRunner.Stop(_autoHideCoroutine);
                _autoHideCoroutine = null;
            }
        }

        private IEnumerator AutoHideCoroutine()
        {
            float elapsed = 0f;
            
            while (elapsed < AutoHideDelay)
            {
                elapsed += Time.deltaTime;
                
                // Update progress bar if present
                if (_progressBar != null)
                {
                    float progress = 1f - (elapsed / AutoHideDelay);
                    _progressBar.style.width = Length.Percent(progress * 100f);
                }
                
                yield return null;
            }

            // Auto-hide the notification
            OnNotificationExpired?.Invoke(this);
            Hide();
        }

        #endregion

        #region Event Handlers

        private void HandleCloseClicked()
        {
            OnNotificationClosed?.Invoke(this);
            Hide();
        }

        #endregion
    }

    /// <summary>
    /// Types of notifications for different styling and behavior
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}