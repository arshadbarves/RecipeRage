using System;
using Core.Banking;
using Core.Banking.Interfaces;
using Core.Logging;
using Core.Persistence;
using Core.Shared;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Session;
using VContainer;

namespace Gameplay.UI.Features.User
{
    public class UsernameViewModel : BaseViewModel
    {
        private readonly ISaveService _saveService;
        private readonly SessionManager _sessionManager;
        private readonly IUIService _uiService;

        public BindableProperty<string> Username { get; } = new BindableProperty<string>("");
        public BindableProperty<string> StatusMessage { get; } = new BindableProperty<string>("");
        public BindableProperty<bool> IsStatusError { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsLoading { get; } = new BindableProperty<bool>(false);
        public BindableProperty<string> CostText { get; } = new BindableProperty<string>("0");
        public BindableProperty<bool> IsCostVisible { get; } = new BindableProperty<bool>(false);
        public BindableProperty<bool> IsCancelVisible { get; } = new BindableProperty<bool>(true);
        public BindableProperty<bool> IsConfirmEnabled { get; } = new BindableProperty<bool>(false);

        private int _changeCost = 0;
        private bool _isFirstTime = false;
        private Action<string> _onConfirmCallback;

        private IBankService BankService
        {
            get
            {
                var sessionContainer = _sessionManager?.SessionContainer;
                if (sessionContainer != null)
                {
                    return sessionContainer.Resolve<IBankService>();
                }
                return null;
            }
        }

        [Inject]
        public UsernameViewModel(ISaveService saveService, SessionManager sessionManager, IUIService uiService)
        {
            _saveService = saveService;
            _sessionManager = sessionManager;
            _uiService = uiService;
        }

        public override void Initialize()
        {
            base.Initialize();
            Username.OnValueChanged += OnUsernameChanged;
        }

        public void Setup(bool isFirstTime, Action<string> onConfirm)
        {
            _isFirstTime = isFirstTime;
            _onConfirmCallback = onConfirm;
            _changeCost = isFirstTime ? 0 : 50;

            IsCancelVisible.Value = !isFirstTime;
            IsCostVisible.Value = _changeCost > 0;
            CostText.Value = _changeCost.ToString();

            // Load current name if not first time
            if (!isFirstTime)
            {
                var stats = _saveService.GetPlayerStats();
                Username.Value = stats.PlayerName ?? "";
            }
            else
            {
                Username.Value = "";
            }
            
            ValidateInput(Username.Value);
        }

        private void OnUsernameChanged(string newValue)
        {
            ValidateInput(newValue);
        }

        private void ValidateInput(string name)
        {
            if (IsLoading.Value) return;

            string trimmedName = name?.Trim() ?? "";
            bool isValid = ValidateUsernameLogic(trimmedName, out string error);

            if (!string.IsNullOrEmpty(trimmedName))
            {
                StatusMessage.Value = error;
                IsStatusError.Value = !isValid;
            }
            else
            {
                StatusMessage.Value = "";
                IsStatusError.Value = false;
            }

            IsConfirmEnabled.Value = isValid;
        }

        private bool ValidateUsernameLogic(string name, out string error)
        {
            if (string.IsNullOrEmpty(name))
            {
                error = "Username cannot be empty";
                return false;
            }

            if (name.Length < 3)
            {
                error = "Minimum 3 characters required";
                return false;
            }

            if (name.Length > 16)
            {
                error = "Maximum 16 characters allowed";
                return false;
            }

            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ')
                {
                    error = "Only letters, numbers, and underscores allowed";
                    return false;
                }
            }

            error = "Username is available";
            return true;
        }

        public void SaveUsername()
        {
            if (IsLoading.Value || !IsConfirmEnabled.Value) return;

            string newName = Username.Value?.Trim();
            
            // Re-validate just in case
            if (!ValidateUsernameLogic(newName, out string error))
            {
                StatusMessage.Value = error;
                IsStatusError.Value = true;
                return;
            }

            // Check cost
            if (_changeCost > 0)
            {
                if (BankService == null)
                {
                    StatusMessage.Value = "Session Error. Try restarting.";
                    IsStatusError.Value = true;
                    return;
                }

                if (BankService.GetBalance(BankKeys.CurrencyGems) < _changeCost)
                {
                    StatusMessage.Value = "Not enough gems!";
                    IsStatusError.Value = true;
                    return;
                }
            }

            IsLoading.Value = true;
            IsConfirmEnabled.Value = false;
            StatusMessage.Value = "Saving...";
            IsStatusError.Value = false;

            try
            {
                if (_changeCost > 0 && BankService != null)
                {
                    BankService.ModifyBalance(BankKeys.CurrencyGems, -_changeCost);
                }

                _saveService.UpdatePlayerStats(stats =>
                {
                    stats.PlayerName = newName;
                });

                _onConfirmCallback?.Invoke(newName);
                _uiService.ShowNotification("Username updated successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to save username: {ex.Message}");
                StatusMessage.Value = "Failed to save. Try again.";
                IsStatusError.Value = true;
                IsConfirmEnabled.Value = true; // Re-enable so they can try again
            }
            finally
            {
                IsLoading.Value = false;
            }
        }

        public override void Dispose()
        {
            Username.OnValueChanged -= OnUsernameChanged;
            base.Dispose();
        }
    }
}