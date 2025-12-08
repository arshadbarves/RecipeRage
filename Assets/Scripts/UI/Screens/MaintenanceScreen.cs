using System;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Cysharp.Threading.Tasks;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Maintenance screen - displays maintenance information with optional countdown
    /// Two modes:
    /// 1. Scheduled Maintenance - Shows estimated end time with countdown
    /// 2. Server Down - Shows generic message with retry button
    /// </summary>
    [UIScreen(UIScreenType.Maintenance, UIScreenCategory.System, "Screens/MaintenanceTemplate")]
    public class MaintenanceScreen : BaseUIScreen
    {
        #region UI Elements

        private VisualElement _maintenanceCard;
        private Label _titleLabel;
        private Label _messageLabel;
        private VisualElement _countdownContainer;
        private Label _countdownLabel;
        private Button _retryButton;
        private VisualElement _maintenanceIcon;

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

            GameLogger.Log("Initialized");
        }

        protected override void OnShow()
        {
            _isRetrying = false;
            UpdateUI();

            GameLogger.Log("Showing maintenance screen");
        }

        protected override void OnHide()
        {
            _isRetrying = false;

            GameLogger.Log("Hiding maintenance screen");
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

        #region UI Setup

        private void CacheUIElements()
        {
            _titleLabel = GetElement<Label>("maintenance-title");
            _messageLabel = GetElement<Label>("news-title");
            _countdownContainer = GetElement<VisualElement>("countdown-container");
            _countdownLabel = GetElement<Label>("countdown-label");
            _retryButton = GetElement<Button>("retry-button");
            _maintenanceIcon = GetElement<VisualElement>("maintenance-icon");

            if (_titleLabel == null)
                GameLogger.LogWarning("maintenance-title not found");
            if (_countdownContainer == null)
                GameLogger.LogWarning("countdown-container not found");
            if (_countdownLabel == null)
                GameLogger.LogWarning("countdown-label not found");
            if (_retryButton == null)
                GameLogger.LogWarning("retry-button not found");
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
            var eventBus = GameBootstrap.Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Subscribe<MaintenanceModeEvent>(HandleMaintenanceModeEvent);
                eventBus.Subscribe<MaintenanceCheckFailedEvent>(HandleMaintenanceCheckFailed);
            }
        }

        private void UnsubscribeFromEvents()
        {
            var eventBus = GameBootstrap.Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<MaintenanceModeEvent>(HandleMaintenanceModeEvent);
                eventBus.Unsubscribe<MaintenanceCheckFailedEvent>(HandleMaintenanceCheckFailed);
            }
        }

        private void UpdateUI()
        {
            // Update title
            if (_titleLabel != null)
            {
                _titleLabel.text = _hasEstimatedTime ? "Scheduled Maintenance" : "Service Unavailable";
            }

            // Update message
            if (_messageLabel != null)
            {
                _messageLabel.text = _maintenanceMessage;
            }

            // Update countdown visibility
            if (_countdownContainer != null)
            {
                _countdownContainer.style.display = _hasEstimatedTime ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Update retry button
            if (_retryButton != null)
            {
                _retryButton.style.display = _allowRetry ? DisplayStyle.Flex : DisplayStyle.None;
                _retryButton.SetEnabled(!_isRetrying);
                _retryButton.text = _isRetrying ? "Retrying..." : "Retry Connection";
            }

            // Update countdown if available
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
                // Format: "Estimated time remaining: 2h 15m"
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

        #endregion

        #region Event Handlers

        private void HandleMaintenanceModeEvent(MaintenanceModeEvent evt)
        {
            GameLogger.Log($"Maintenance mode event received - IsMaintenanceMode: {evt.IsMaintenanceMode}");

            if (!evt.IsMaintenanceMode)
            {
                // No maintenance, hide screen if visible
                if (IsVisible)
                {
                    Hide(true);
                }
                return;
            }

            // Set maintenance data
            _maintenanceMessage = evt.Message;
            _allowRetry = evt.AllowRetry;

            // Parse estimated end time if available
            _hasEstimatedTime = !string.IsNullOrEmpty(evt.EstimatedEndTime);
            if (_hasEstimatedTime)
            {
                try
                {
                    _estimatedEndTime = DateTime.Parse(evt.EstimatedEndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    GameLogger.Log($"Estimated end time: {_estimatedEndTime}");
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"Failed to parse estimated end time: {ex.Message}");
                    _hasEstimatedTime = false;
                }
            }

            // Show maintenance screen
            UpdateUI();

            if (!IsVisible)
            {
                Show(true);
            }
        }

        private void HandleMaintenanceCheckFailed(MaintenanceCheckFailedEvent evt)
        {
            GameLogger.LogWarning($"Maintenance check failed: {evt.Error}");
            // This could indicate server is down - but we don't show maintenance screen here
            // AuthenticationService will handle showing maintenance for login failures
        }

        private void OnRetryClicked()
        {
            if (_isRetrying) return;

            GameLogger.Log("Retry button clicked");
            _isRetrying = true;
            UpdateUI();

            // Attempt to reconnect
            AttemptReconnectAsync().Forget();
        }

        private async UniTaskVoid AttemptReconnectAsync()
        {
            try
            {
                // Wait a bit before retrying
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                var authService = GameBootstrap.Services?.AuthenticationService;
                if (authService == null)
                {
                    GameLogger.LogError("AuthenticationService not available");
                    _isRetrying = false;
                    UpdateUI();
                    return;
                }

                // Try to log in again
                bool success = await authService.AttemptAutoLoginAsync();

                if (success)
                {
                    GameLogger.Log("Reconnect successful");

                    // Check maintenance status again
                    var maintenanceService = GameBootstrap.Services?.MaintenanceService;
                    if (maintenanceService != null)
                    {
                        await maintenanceService.CheckMaintenanceStatusAsync();
                    }
                }
                else
                {
                    GameLogger.LogWarning("Reconnect failed");
                    _isRetrying = false;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Reconnect error: {ex.Message}");
                _isRetrying = false;
                UpdateUI();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set maintenance data and show screen
        /// </summary>
        public void ShowMaintenance(bool hasEstimate, DateTime? estimatedEndTime, string message, bool allowRetry)
        {
            _hasEstimatedTime = hasEstimate && estimatedEndTime.HasValue;
            _estimatedEndTime = estimatedEndTime ?? DateTime.UtcNow;
            _maintenanceMessage = message;
            _allowRetry = allowRetry;

            UpdateUI();
            Show(true);
        }

        #endregion
    }
}
