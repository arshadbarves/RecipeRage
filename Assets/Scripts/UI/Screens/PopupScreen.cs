using System;
using Core.Animation;
using Core.Bootstrap;
using UI.Core;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Generic popup screen for confirmations, alerts, and dialogs
    /// Pure C# implementation with fluent API
    /// </summary>
    [UIScreen(UIScreenType.Popup, UIScreenPriority.Popup, "Popups/PopupTemplate")]
    public class PopupScreen : BaseUIScreen
    {
        #region Configuration Properties

        public string Title { get; set; } = "Popup";
        public string Message { get; set; } = "This is a popup message.";
        public string ConfirmButtonText { get; set; } = "OK";
        public string CancelButtonText { get; set; } = "Cancel";
        public bool ShowCancelButton { get; set; } = true;
        public bool ShowCloseButton { get; set; } = true;

        #endregion

        #region UI Elements

        private Label _titleLabel;
        private Label _messageLabel;
        private Button _confirmButton;
        private Button _cancelButton;
        private Button _closeButton;

        #endregion

        #region Actions

        public Action OnConfirmAction { get; set; }
        public Action OnCancelAction { get; set; }
        public Action OnCloseAction { get; set; }

        #endregion

        #region Events

        public event Action<PopupScreen> OnConfirmed;
        public event Action<PopupScreen> OnCancelled;
        public event Action<PopupScreen> OnClosed;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
            UpdateUI();
        }

        protected override void OnShow()
        {
            UpdateUI();
        }

        protected override void OnDispose()
        {
            UnregisterEventHandlers();
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _titleLabel = GetElement<Label>("popup-title");
            _messageLabel = GetElement<Label>("popup-message");
            _confirmButton = GetElement<Button>("confirm-button");
            _cancelButton = GetElement<Button>("cancel-button");
            _closeButton = GetElement<Button>("close-button");
        }

        private void SetupEventHandlers()
        {
            _confirmButton?.RegisterCallback<ClickEvent>(_ => HandleConfirmClicked());
            _cancelButton?.RegisterCallback<ClickEvent>(_ => HandleCancelClicked());
            _closeButton?.RegisterCallback<ClickEvent>(_ => HandleCloseClicked());
        }

        private void UnregisterEventHandlers()
        {
            _confirmButton?.UnregisterCallback<ClickEvent>(_ => HandleConfirmClicked());
            _cancelButton?.UnregisterCallback<ClickEvent>(_ => HandleCancelClicked());
            _closeButton?.UnregisterCallback<ClickEvent>(_ => HandleCloseClicked());
        }

        #endregion

        #region Fluent API

        /// <summary>
        /// Set the popup title
        /// </summary>
        public PopupScreen SetTitle(string title)
        {
            Title = title;
            if (_titleLabel != null)
            {
                _titleLabel.text = title;
            }
            return this;
        }

        /// <summary>
        /// Set the popup message
        /// </summary>
        public PopupScreen SetMessage(string message)
        {
            Message = message;
            if (_messageLabel != null)
            {
                _messageLabel.text = message;
            }
            return this;
        }

        /// <summary>
        /// Set the confirm button text and action
        /// </summary>
        public PopupScreen SetConfirmButton(string text, Action action = null)
        {
            ConfirmButtonText = text;
            OnConfirmAction = action;
            
            if (_confirmButton != null)
            {
                _confirmButton.text = text;
                _confirmButton.style.display = DisplayStyle.Flex;
            }
            
            return this;
        }

        /// <summary>
        /// Set the cancel button text and action
        /// </summary>
        public PopupScreen SetCancelButton(string text, Action action = null)
        {
            CancelButtonText = text;
            OnCancelAction = action;
            ShowCancelButton = true;
            
            if (_cancelButton != null)
            {
                _cancelButton.text = text;
                _cancelButton.style.display = DisplayStyle.Flex;
            }
            
            return this;
        }

        /// <summary>
        /// Set the confirm action
        /// </summary>
        public PopupScreen SetConfirmAction(Action action)
        {
            OnConfirmAction = action;
            return this;
        }

        /// <summary>
        /// Set the cancel action
        /// </summary>
        public PopupScreen SetCancelAction(Action action)
        {
            OnCancelAction = action;
            return this;
        }

        /// <summary>
        /// Hide the cancel button
        /// </summary>
        public PopupScreen HideCancelButton()
        {
            ShowCancelButton = false;
            if (_cancelButton != null)
            {
                _cancelButton.style.display = DisplayStyle.None;
            }
            return this;
        }

        /// <summary>
        /// Hide the close button
        /// </summary>
        public PopupScreen HideCloseButton()
        {
            ShowCloseButton = false;
            if (_closeButton != null)
            {
                _closeButton.style.display = DisplayStyle.None;
            }
            return this;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Create a simple alert popup
        /// </summary>
        public static PopupScreen CreateAlert(string title, string message, Action onConfirm = null)
        {
            PopupScreen popup = GameBootstrap.Services?.UIService.GetScreen<PopupScreen>();
            return popup?
                .SetTitle(title)
                .SetMessage(message)
                .SetConfirmButton("OK", onConfirm)
                .HideCancelButton();
        }

        /// <summary>
        /// Create a confirmation popup
        /// </summary>
        public static PopupScreen CreateConfirmation(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            PopupScreen popup = GameBootstrap.Services?.UIService.GetScreen<PopupScreen>();
            return popup?
                .SetTitle(title)
                .SetMessage(message)
                .SetConfirmButton("Confirm", onConfirm)
                .SetCancelButton("Cancel", onCancel);
        }

        /// <summary>
        /// Create a yes/no popup
        /// </summary>
        public static PopupScreen CreateYesNo(string title, string message, Action onYes = null, Action onNo = null)
        {
            PopupScreen popup = GameBootstrap.Services?.UIService.GetScreen<PopupScreen>();
            return popup?
                .SetTitle(title)
                .SetMessage(message)
                .SetConfirmButton("Yes", onYes)
                .SetCancelButton("No", onNo);
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = Title;
            }
            if (_messageLabel != null)
            {
                _messageLabel.text = Message;
            }
            if (_confirmButton != null)
            {
                _confirmButton.text = ConfirmButtonText;
            }
            if (_cancelButton != null) 
            {
                _cancelButton.text = CancelButtonText;
                _cancelButton.style.display = ShowCancelButton ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (_closeButton != null)
            {
                _closeButton.style.display = ShowCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region Animation Customization

        /// <summary>
        /// Popups use a bouncy scale-in animation for more impact
        /// </summary>
        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.ScaleIn(element, duration, onComplete);
        }

        /// <summary>
        /// Popups scale out quickly when hiding
        /// </summary>
        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.ScaleOut(element, duration, onComplete);
        }

        /// <summary>
        /// Popups animate faster for snappy interactions
        /// </summary>
        public override float GetAnimationDuration()
        {
            return 0.25f; // Slightly faster than default
        }

        /// <summary>
        /// Add a subtle fade to the background before showing
        /// </summary>
        public override void OnBeforeShowAnimation()
        {
            // Could add background dimming effect here
            // Or play a sound effect
            // Or trigger haptic feedback on mobile
        }

        /// <summary>
        /// Focus the confirm button after the popup is fully shown
        /// </summary>
        public override void OnAfterShowAnimation()
        {
            // Auto-focus the confirm button for keyboard navigation
            _confirmButton?.Focus();
        }

        #endregion

        #region Event Handlers

        private void HandleConfirmClicked()
        {
            OnConfirmAction?.Invoke();
            OnConfirmed?.Invoke(this);
            Hide();
        }

        private void HandleCancelClicked()
        {
            OnCancelAction?.Invoke();
            OnCancelled?.Invoke(this);
            Hide();
        }

        private void HandleCloseClicked()
        {
            OnCloseAction?.Invoke();
            OnClosed?.Invoke(this);
            Hide();
        }

        #endregion
    }
}