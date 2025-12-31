using System;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Cysharp.Threading.Tasks;
using UI.Core;
using UnityEngine.UIElements;
using RecipeRage.Modules.Auth.Core;
using VContainer;

namespace UI.Screens
{
    /// <summary>
    /// Maintenance screen - displays maintenance information with optional countdown
    /// </summary>
    [UIScreen(UIScreenType.Maintenance, UIScreenCategory.System, "Screens/MaintenanceTemplate")]
    public class MaintenanceScreen : BaseUIScreen
    {
        #region Dependencies

        [Inject]
        private IEventBus _eventBus;

        [Inject]
        private IAuthService _authService;

        [Inject]
        private IMaintenanceService _maintenanceService;

        [Inject]
        private ILoggingService _loggingService;

        #endregion

        #region UI Elements

        private Label _titleLabel;
        private Label _messageLabel;
        private VisualElement _countdownContainer;
        private Label _countdownLabel;
        private Button _retryButton;

        #endregion

        #region State

        private bool _hasEstimatedTime;
        private DateTime _estimatedEndTime;
        private string _maintenanceMessage;
        private bool _allowRetry;
        private bool _isRetrying;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupButtonHandlers();
            SubscribeToEvents();

            _loggingService?.LogInfo("Initialized");
        }

        protected override void OnShow()
        {
            _isRetrying = false;
            UpdateUI();

            _loggingService?.LogInfo("Showing maintenance screen");
        }

        protected override void OnHide()
        {
            _isRetrying = false;
            _loggingService?.LogInfo("Hiding maintenance screen");
        }

        public override void Update(float deltaTime)
        {
            if (_hasEstimatedTime && _countdownContainer != null)
            {
                UpdateCountdown();
            }
        }

        protected override void OnDispose()
        {
            if (_retryButton != null)
            {
                _retryButton.clicked -= OnRetryClicked;
            }

            UnsubscribeFromEvents();
        }

        #endregion

        private void CacheUIElements()
        {
            _titleLabel = GetElement<Label>("maintenance-title");
            _messageLabel = GetElement<Label>("news-title");
            _countdownContainer = GetElement<VisualElement>("countdown-container");
            _countdownLabel = GetElement<Label>("countdown-label");
            _retryButton = GetElement<Button>("retry-button");

            if (_titleLabel == null) _loggingService?.LogWarning("maintenance-title not found");
            if (_countdownContainer == null) _loggingService?.LogWarning("countdown-container not found");
            if (_countdownLabel == null) _loggingService?.LogWarning("countdown-label not found");
            if (_retryButton == null) _loggingService?.LogWarning("retry-button not found");
        }

        private void SetupButtonHandlers()
        {
            if (_retryButton != null)
            {
                _retryButton.clicked += OnRetryClicked;
            }
        }

        private void SubscribeToEvents()
        {
            if (_eventBus != null)
            {
                _eventBus.Subscribe<MaintenanceModeEvent>(HandleMaintenanceModeEvent);
                _eventBus.Subscribe<MaintenanceCheckFailedEvent>(HandleMaintenanceCheckFailed);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<MaintenanceModeEvent>(HandleMaintenanceModeEvent);
                _eventBus.Unsubscribe<MaintenanceCheckFailedEvent>(HandleMaintenanceCheckFailed);
            }
        }

        private void UpdateUI()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = _hasEstimatedTime ? "Scheduled Maintenance" : "Service Unavailable";
            }

            if (_messageLabel != null)
            {
                _messageLabel.text = _maintenanceMessage;
            }

            if (_countdownContainer != null)
            {
                _countdownContainer.style.display = _hasEstimatedTime ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_retryButton != null)
            {
                _retryButton.style.display = _allowRetry ? DisplayStyle.Flex : DisplayStyle.None;
                _retryButton.SetEnabled(!_isRetrying);
                _retryButton.text = _isRetrying ? "Retrying..." : "Retry Connection";
            }

            if (_hasEstimatedTime)
            {
                UpdateCountdown();
            }
        }

        private void UpdateCountdown()
        {
            if (_countdownLabel == null) return;

            TimeSpan remaining = _estimatedEndTime - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0)
            {
                _countdownLabel.text = "Maintenance should be complete. Please retry.";
                _allowRetry = true;

                if (_retryButton != null)
                {
                    _retryButton.style.display = DisplayStyle.Flex;
                }
            }
            else
            {
                string timeText = FormatTimeRemaining(remaining);
                _countdownLabel.text = $"Estimated time remaining: {timeText}";
            }
        }

        private string FormatTimeRemaining(TimeSpan remaining)
        {
            if (remaining.TotalHours >= 1)
            {
                return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            }
            else if (remaining.TotalMinutes >= 1)
            {
                return $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s";
            }
            else
            {
                return $"{remaining.Seconds}s";
            }
        }

        private void HandleMaintenanceModeEvent(MaintenanceModeEvent evt)
        {
            _loggingService?.LogInfo($"Maintenance mode event received - IsMaintenanceMode: {evt.IsMaintenanceMode}");

            if (!evt.IsMaintenanceMode)
            {
                if (IsVisible) Hide(true);
                return;
            }

            _maintenanceMessage = evt.Message;
            _allowRetry = evt.AllowRetry;

            _hasEstimatedTime = !string.IsNullOrEmpty(evt.EstimatedEndTime);
            if (_hasEstimatedTime)
            {
                try
                {
                    _estimatedEndTime = DateTime.Parse(evt.EstimatedEndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    _loggingService?.LogInfo($"Estimated end time: {_estimatedEndTime}");
                }
                catch (Exception ex)
                {
                    _loggingService?.LogError($"Failed to parse estimated end time: {ex.Message}");
                    _hasEstimatedTime = false;
                }
            }

            UpdateUI();
            if (!IsVisible) Show(true);
        }

        private void HandleMaintenanceCheckFailed(MaintenanceCheckFailedEvent evt)
        {
            _loggingService?.LogWarning($"Maintenance check failed: {evt.Error}");
        }

        private void OnRetryClicked()
        {
            if (_isRetrying) return;

            _loggingService?.LogInfo("Retry button clicked");
            _isRetrying = true;
            UpdateUI();

            AttemptReconnectAsync().Forget();
        }

        private async UniTaskVoid AttemptReconnectAsync()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                if (_authService == null)
                {
                    _loggingService?.LogError("AuthService not available");
                    _isRetrying = false;
                    UpdateUI();
                    return;
                }

                bool success = await _authService.LoginAsync(AuthType.DeviceID);

                if (success)
                {
                    _loggingService?.LogInfo("Reconnect successful");
                    if (_maintenanceService != null)
                    {
                        await _maintenanceService.CheckMaintenanceStatusAsync();
                    }
                }
                else
                {
                    _loggingService?.LogWarning("Reconnect failed");
                    _isRetrying = false;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError($"Reconnect error: {ex.Message}");
                _isRetrying = false;
                UpdateUI();
            }
        }
    }
}