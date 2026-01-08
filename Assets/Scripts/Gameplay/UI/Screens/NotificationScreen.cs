using System;
using System.Collections.Generic;
using Modules.UI;
using Cysharp.Threading.Tasks;
using Modules.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.UI.Screens
{
    /// <summary>
    /// Persistent notification screen for toasts and messages
    /// </summary>
    [UIScreen(UIScreenType.Notification, UIScreenCategory.Persistent, "Popups/NotificationTemplate")]
    public class NotificationScreen : BaseUIScreen
    {
        private VisualElement _notificationContainer;
        private VisualTreeAsset _toastTemplate;
        private readonly Queue<NotificationRequest> _queue = new();
        private bool _isShowing;

        protected override void OnInitialize()
        {
            _notificationContainer = GetElement<VisualElement>("notification-container");
            _toastTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/NotificationToast");
        }

        public async UniTask Show(string message, UI.NotificationType type, float duration)
        {
            _queue.Enqueue(new NotificationRequest { Message = message, Type = type, Duration = duration });
            if (!_isShowing) await ProcessQueue();
        }

        public async UniTask Show(string title, string message, UI.NotificationType type, float duration)
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
        }

        private async UniTask ShowNotification(NotificationRequest request)
        {
            if (_notificationContainer == null || _toastTemplate == null) return;

            var toast = _toastTemplate.CloneTree();
            var root = toast.Q<VisualElement>("toast-root");
            var titleLabel = toast.Q<Label>("toast-title");
            var messageLabel = toast.Q<Label>("toast-message");

            if (titleLabel != null) titleLabel.text = request.Title ?? "";
            if (messageLabel != null) messageLabel.text = request.Message;

            root.AddToClassList(request.Type.ToString().ToLower());
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
            public UI.NotificationType Type;
            public float Duration;
        }
    }
}