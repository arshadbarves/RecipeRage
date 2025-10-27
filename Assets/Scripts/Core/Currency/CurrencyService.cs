using Core.Bootstrap;
using Core.Events;
using Core.Interfaces;
using Core.SaveSystem;
using UI.Screens;
using UnityEngine;

namespace Core.Currency
{
    /// <summary>
    /// Currency service implementation
    /// Manages coins and gems with event-driven updates
    /// Uses SaveService for persistence (not PlayerPrefs)
    /// </summary>
    public class CurrencyService : ICurrencyService, IResettableService
    {
        private const string SAVE_KEY = "player_currency";
        private const int DEFAULT_COINS = 1250;
        private const int DEFAULT_GEMS = 85;

        private readonly ISaveService _saveService;
        private readonly IEventBus _eventBus;

        private int _coins;
        private int _gems;

        public int Coins => _coins;
        public int Gems => _gems;

        public CurrencyService(ISaveService saveService, IEventBus eventBus)
        {
            _saveService = saveService;
            _eventBus = eventBus;

            LoadFromSave();
            Debug.Log($"[CurrencyService] Initialized with {_coins} coins, {_gems} gems");
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyService] Attempted to add invalid coin amount: {amount}");
                return;
            }

            _coins += amount;
            SaveToDisk();
            PublishCurrencyChanged();

            Debug.Log($"[CurrencyService] Added {amount} coins. Total: {_coins}");

            // Show reward toast
            _ = GameBootstrap.Services?.UIService?.ShowToast($"+{amount} coins earned!", ToastType.Success, 2f);
        }

        public void AddGems(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyService] Attempted to add invalid gem amount: {amount}");
                return;
            }

            _gems += amount;
            SaveToDisk();
            PublishCurrencyChanged();

            Debug.Log($"[CurrencyService] Added {amount} gems. Total: {_gems}");

            // Show reward toast
            _ = GameBootstrap.Services?.UIService?.ShowToast($"+{amount} gems earned!", ToastType.Success, 2f);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyService] Attempted to spend invalid coin amount: {amount}");
                return false;
            }

            if (_coins >= amount)
            {
                _coins -= amount;
                SaveToDisk();
                PublishCurrencyChanged();

                Debug.Log($"[CurrencyService] Spent {amount} coins. Remaining: {_coins}");

                // Show success toast
                _ = GameBootstrap.Services?.UIService?.ShowToast($"Purchased for {amount} coins", ToastType.Success, 2f);

                return true;
            }

            Debug.LogWarning($"[CurrencyService] Not enough coins. Need {amount}, have {_coins}");

            // Show error toast
            _ = GameBootstrap.Services?.UIService?.ShowToast("Not enough coins", ToastType.Error, 2.5f);

            return false;
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyService] Attempted to spend invalid gem amount: {amount}");
                return false;
            }

            if (_gems >= amount)
            {
                _gems -= amount;
                SaveToDisk();
                PublishCurrencyChanged();

                Debug.Log($"[CurrencyService] Spent {amount} gems. Remaining: {_gems}");

                // Show success toast
                _ = GameBootstrap.Services?.UIService?.ShowToast($"Purchased for {amount} gems", ToastType.Success, 2f);

                return true;
            }

            Debug.LogWarning($"[CurrencyService] Not enough gems. Need {amount}, have {_gems}");

            // Show error toast
            _ = GameBootstrap.Services?.UIService?.ShowToast("Not enough gems", ToastType.Error, 2.5f);

            return false;
        }

        public void LoadFromSave()
        {
            CurrencyData data = _saveService.LoadData<CurrencyData>(SAVE_KEY);

            if (data != null)
            {
                _coins = data.Coins;
                _gems = data.Gems;
                Debug.Log($"[CurrencyService] Loaded from save: {_coins} coins, {_gems} gems");
            }
            else
            {
                // First time - set defaults
                _coins = DEFAULT_COINS;
                _gems = DEFAULT_GEMS;
                SaveToDisk();
                Debug.Log($"[CurrencyService] No save data found, using defaults: {_coins} coins, {_gems} gems");
            }

            PublishCurrencyChanged();
        }

        public void SaveToDisk()
        {
            CurrencyData data = new CurrencyData(_coins, _gems);
            _saveService.SaveData(SAVE_KEY, data);
        }

        public string FormatCurrency(int amount)
        {
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:F1}M";
            }
            else if (amount >= 1000)
            {
                return $"{amount / 1000f:F1}K";
            }
            return amount.ToString();
        }

        public void Reset()
        {
            Debug.Log("[CurrencyService] Resetting currency data");

            _coins = 0;
            _gems = 0;

            // Publish reset event
            _eventBus.Publish(new CurrencyResetEvent());

            // Also publish changed event with zero values
            PublishCurrencyChanged();
        }

        private void PublishCurrencyChanged()
        {
            _eventBus.Publish(new CurrencyChangedEvent
            {
                Coins = _coins,
                Gems = _gems
            });
        }
    }
}
