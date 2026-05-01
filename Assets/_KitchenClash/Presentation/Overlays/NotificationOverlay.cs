using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using System.Collections.Generic;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Overlays
{
    /// <summary>
    /// Toast host for queued notifications and transient system messages
    /// </summary>
    [UIScreen(UIScreenCategory.Toast, "Popups/NotificationTemplate")]
    public class NotificationOverlay : BaseUIScreen, Presentation.Common.INotificationScreen
    {
        private VisualElement _hostRoot;
        private VisualElement _notificationContainer;
        private VisualTreeAsset _toastTemplate;
        private readonly Queue<NotificationRequest> _queue = new();
        private bool _isShowing;

        protected override void OnInitialize()
        {
            _hostRoot = GetElement<VisualElement>("screen-container");
            _notificationContainer = GetElement<VisualElement>("notification-container");
            _toastTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/NotificationToast");

            if (Container != null) Container.pickingMode = PickingMode.Ignore;
            if (TemplateContainer != null) TemplateContainer.pickingMode = PickingMode.Ignore;
            if (_hostRoot != null) _hostRoot.pickingMode = PickingMode.Ignore;
            if (_notificationContainer != null) _notificationContainer.pickingMode = PickingMode.Ignore;
        }

        public async UniTask Show(string message, NotificationType type, float duration)
        {
            _queue.Enqueue(new NotificationRequest { Message = message, Type = type, Duration = duration });
            if (!_isShowing) await ProcessQueue();
        }

        public async UniTask Show(string title, string message, NotificationType type, float duration)
        {
            _queue.Enqueue(new NotificationRequest { Title = title, Message = message, Type = type, Duration = duration });
            if (!_isShowing) await ProcessQueue();
        }

        private async UniTask ProcessQueue()
        {
            _isShowing = true;
            while (_queue.Count > 0)
            {
                var request = _queue.Dequeue();
                await ShowNotification(request);
            }
            _isShowing = false;

            if (IsVisible)
            {
                Hide(false);
            }
        }

        private async UniTask ShowNotification(NotificationRequest request)
        {
            if (_notificationContainer == null || _toastTemplate == null) return;

            var toast = _toastTemplate.CloneTree();
            var root = toast.Q<VisualElement>("notification-toast");
            var messageLabel = toast.Q<Label>("notification-message");
            var closeButton = toast.Q<Button>("close-button");

            if (root == null || messageLabel == null)
            {
                return;
            }

            if (messageLabel != null)
            {
                messageLabel.text = string.IsNullOrWhiteSpace(request.Title)
                    ? request.Message
                    : $"{request.Title}\n{request.Message}";
            }

            if (closeButton != null)
            {
                closeButton.style.display = DisplayStyle.None;
            }

            root.pickingMode = PickingMode.Ignore;
            root.AddToClassList(request.Type.ToString().ToLowerInvariant());
            _notificationContainer.Add(root);

            // Simple animation
            root.style.opacity = 0;
            await UniTask.Delay(10);
            root.style.opacity = 1;

            await UniTask.Delay(TimeSpan.FromSeconds(request.Duration));

            root.style.opacity = 0;
            await UniTask.Delay(300);
            _notificationContainer.Remove(root);
        }

        private class NotificationRequest
        {
            public string Title;
            public string Message;
            public NotificationType Type;
            public float Duration;
        }
    }
}
