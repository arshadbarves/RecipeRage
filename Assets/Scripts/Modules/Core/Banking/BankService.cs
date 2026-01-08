using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using Core.UI;
using Modules.Core.Banking.Data;
using Modules.Core.Banking.Interfaces;

namespace Modules.Core.Banking
{
    public class BankService : IBankService
    {
        private readonly IBankBackend _backend;
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private BankData _data;

        public event Action<string, long> OnBalanceChanged;
        public event Action<string> OnItemUnlocked;

        public BankService(IBankBackend backend, IEventBus eventBus, IUIService uiService)
        {
            _backend = backend;
            _eventBus = eventBus;
            _uiService = uiService;
        }

        public async Task InitializeAsync()
        {
            _data = await _backend.LoadDataAsync();
            // Trigger initial events for all balances
            foreach (var kvp in _data.Balances)
            {
                OnBalanceChanged?.Invoke(kvp.Key, kvp.Value);
            }
        }

        public long GetBalance(string currencyId)
        {
            if (_data == null) return 0;
            return _data.Balances.TryGetValue(currencyId, out long balance) ? balance : 0;
        }

        public void ModifyBalance(string currencyId, long amount)
        {
            if (_data == null || amount == 0) return;

            long current = GetBalance(currencyId);
            long next = current + amount;
            
            _data.Balances[currencyId] = next;
            SaveToDisk();

            OnBalanceChanged?.Invoke(currencyId, next);
            
            // Legacy/Compat: Notify UI for standard currencies
            if (amount > 0 && (currencyId == BankKeys.CurrencyCoins || currencyId == BankKeys.CurrencyGems))
            {
                _uiService?.ShowNotification($"+{amount} {currencyId}!", NotificationType.Success, 2f);
            }

            // Publish legacy event for backwards compatibility with existing UI components
            PublishLegacyCurrencyEvent();
        }

        public bool HasItem(string itemId)
        {
            return _data?.Inventory.Contains(itemId) ?? false;
        }

        public void AddItem(string itemId)
        {
            if (_data == null || string.IsNullOrEmpty(itemId) || HasItem(itemId)) return;

            _data.Inventory.Add(itemId);
            SaveToDisk();
            OnItemUnlocked?.Invoke(itemId);
        }

        public string GetData(string key)
        {
            if (_data == null) return null;
            return _data.Data.TryGetValue(key, out string value) ? value : null;
        }

        public void SetData(string key, string value)
        {
            if (_data == null) return;
            _data.Data[key] = value;
            SaveToDisk();
        }

        public bool Purchase(string itemId, long cost, string currencyId)
        {
            if (_data == null) return false;

            // Check if already owned
            if (HasItem(itemId)) return false;

            // Check funds
            long balance = GetBalance(currencyId);
            if (balance < cost)
            {
                _uiService?.ShowNotification($"Not enough {currencyId}", NotificationType.Error);
                return false;
            }

            // Atomic Deduct & Grant
            ModifyBalance(currencyId, -cost);
            AddItem(itemId);

            _uiService?.ShowNotification($"Purchased {itemId}!", NotificationType.Success);
            return true;
        }

        private void SaveToDisk()
        {
            _backend.SaveDataAsync(_data).Wait();
        }

        private void PublishLegacyCurrencyEvent()
        {
            // Compatibility with UI components listening to CurrencyChangedEvent
            _eventBus?.Publish(new CurrencyChangedEvent 
            { 
                Coins = (int)GetBalance(BankKeys.CurrencyCoins), 
                Gems = (int)GetBalance(BankKeys.CurrencyGems) 
            });
        }
    }
}