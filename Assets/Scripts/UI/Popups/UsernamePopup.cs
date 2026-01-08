using System;
using Core.UI;
using DG.Tweening;
using UI.Core;
using UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Popups
{
    /// <summary>
    /// Mandatory popup for setting/changing username
    /// Used for both first-time setup and subsequent changes
    /// Refactored to MVVM
    /// </summary>
    [UIScreen(UIScreenType.UsernamePopup, UIScreenCategory.Modal, "Popups/UsernamePopupTemplate")]
    public class UsernamePopup : BaseUIScreen
    {
        [Inject] private UsernameViewModel _viewModel;

        private TextField _usernameField;
        private Label _statusLabel;
        private Label _costLabel;
        private Button _confirmButton;
        private Button _cancelButton;
        private VisualElement _costContainer;
        private VisualElement _cardWrapper;
        private VisualElement _loadingOverlay;
        private VisualElement _spinner;

        private Action _onCancel;
        private Tween _spinnerTween;

        protected override void OnInitialize()
        {
            _usernameField = GetElement<TextField>("username-input");
            _statusLabel = GetElement<Label>("status-message");
            _costLabel = GetElement<Label>("cost-value");
            _confirmButton = GetElement<Button>("confirm-button");
            _cancelButton = GetElement<Button>("cancel-button");
            _costContainer = GetElement<VisualElement>("cost-container");
            _cardWrapper = GetElement<VisualElement>("popup-card-wrapper");
            _loadingOverlay = GetElement<VisualElement>("loading-overlay");
            _spinner = _loadingOverlay?.Q("spinner"); // Assuming class spinner is on a child or element itself

            // If spinner is not found via query, try explicit naming if added, or rely on .spinner class
            if (_spinner == null && _loadingOverlay != null)
                _spinner = _loadingOverlay.Q(className: "spinner");

            BindEvents();
            BindViewModel();
        }

        private void BindEvents()
        {
            if (_confirmButton != null)
                _confirmButton.clicked += OnConfirmClicked;

            if (_cancelButton != null)
                _cancelButton.clicked += OnCancelClicked;

            if (_usernameField != null)
            {
                _usernameField.RegisterValueChangedCallback(OnUsernameChanged);
            }
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.Initialize();

            // Bind Properties
            _viewModel.Username.Bind(val =>
            {
                if (_usernameField != null && _usernameField.value != val)
                    _usernameField.SetValueWithoutNotify(val);
            });

            _viewModel.StatusMessage.Bind(msg =>
            {
                if (_statusLabel != null)
                {
                    _statusLabel.text = msg;
                    _statusLabel.style.visibility = string.IsNullOrEmpty(msg) ? Visibility.Hidden : Visibility.Visible;
                }
            });

            _viewModel.IsStatusError.Bind(isError =>
            {
                if (_statusLabel != null)
                    _statusLabel.style.color = isError ? Color.red : Color.green;
            });

            _viewModel.CostText.Bind(txt =>
            {
                if (_costLabel != null) _costLabel.text = txt;
            });

            _viewModel.IsCostVisible.Bind(visible =>
            {
                if (_costContainer != null)
                    _costContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            });

            _viewModel.IsCancelVisible.Bind(visible =>
            {
                if (_cancelButton != null)
                    _cancelButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            });

            _viewModel.IsConfirmEnabled.Bind(enabled =>
            {
                if (_confirmButton != null)
                    _confirmButton.SetEnabled(enabled);
            });

            _viewModel.IsLoading.Bind(isLoading =>
            {
                if (_loadingOverlay != null)
                {
                    _loadingOverlay.RemoveFromClassList("hidden");
                    if (!isLoading) _loadingOverlay.AddToClassList("hidden");
                }
                
                if (isLoading)
                    StartSpinnerAnimation();
                else
                    StopSpinnerAnimation();
            });
        }

        private void OnUsernameChanged(ChangeEvent<string> evt)
        {
            _viewModel.Username.Value = evt.newValue;
        }

        private void OnConfirmClicked()
        {
            _viewModel.SaveUsername();
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Hide(true);
        }

        public void ShowForUsername(bool isFirstTime, Action<string> onConfirm, Action onCancel = null)
        {
            _onCancel = onCancel;
            
            // Setup ViewModel for this session
            // We wrap the confirm callback to hide the popup on success
            Action<string> wrappedConfirm = (name) =>
            {
                onConfirm?.Invoke(name);
                Hide(true);
            };

            _viewModel.Setup(isFirstTime, wrappedConfirm);

            Show(true, false);
            PlayAppearAnimation();
        }

        protected override void OnShow()
        {
            if (_usernameField != null)
                _usernameField.Focus();
        }

        private void PlayAppearAnimation()
        {
            if (_cardWrapper == null) return;

            _cardWrapper.style.scale = new Scale(Vector3.one * 0.7f);
            DOTween.To(() => 0.7f, x => _cardWrapper.style.scale = new Scale(Vector3.one * x), 1.0f, 0.5f)
                   .SetEase(Ease.OutBack);
        }

        private void StartSpinnerAnimation()
        {
            if (_spinner == null) return;
            
            _spinnerTween?.Kill();
            // Rotate the spinner visual element using DOTween on the transform rotation
            // Note: UI Toolkit rotation is in degrees
            _spinner.style.rotate = new Rotate(0);
            _spinnerTween = DOTween.To(() => 0f, x => _spinner.style.rotate = new Rotate(x), 360f, 1f)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }

        private void StopSpinnerAnimation()
        {
            _spinnerTween?.Kill();
        }

        protected override void OnDispose()
        {
            if (_confirmButton != null)
                _confirmButton.clicked -= OnConfirmClicked;

            if (_cancelButton != null)
                _cancelButton.clicked -= OnCancelClicked;

            if (_usernameField != null)
                _usernameField.UnregisterValueChangedCallback(OnUsernameChanged);
            
            _spinnerTween?.Kill();
            _viewModel?.Dispose();
        }
    }
}
