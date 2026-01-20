using System;
using Gameplay.Economy.Data;
using Core.Persistence;
using Core.UI.Interfaces;

namespace Gameplay.Economy
{
    public class EconomyService
    {
        private readonly ISaveService _saveService;
        private readonly IUIService _uiService;

        private EconomyData _data;

        public event Action<string, long> OnBalanceChanged;
        public event Action<string> OnItemUnlocked;

        public EconomyService(ISaveService saveService, IUIService uiService)
        {
            _saveService = saveService;
            _uiService = uiService;
        }

        public void Initialize()
        {
            // Load Data from Generic SaveService
            _data = _saveService.LoadData<EconomyData>(EconomyKeys.SaveKey);

            // Initialize Defaults if New User (empty inventory/balances logic handled here or in data)
            if (_data == null || (_data.Balances.Count == 0 && _data.Inventory.Count == 0))
            {
                if(_data == null) _data = new EconomyData();

                // Set Defaults
                _data.Balances[EconomyKeys.CurrencyCoins] = 1000;
                _data.Balances[EconomyKeys.CurrencyGems] = 50;
                _data.Inventory.Add(EconomyKeys.ItemStarterPack);

                Save();
            }

            // Fire initial events
            foreach (var kvp in _data.Balances)
            {
                OnBalanceChanged?.Invoke(kvp.Key, kvp.Value);
            }
        }

        public long GetBalance(string currencyId)
        {
            if (_data == null) return 0;
            return _data.Balances.TryGetValue(currencyId, out long val) ? val : 0;
        }

        public void AddCurrency(string currencyId, long amount)
        {
            if (_data == null) return;

            long current = GetBalance(currencyId);
            long next = current + amount;
            _data.Balances[currencyId] = next;

            Save();
            OnBalanceChanged?.Invoke(currencyId, next);
        }

        public bool Purchase(string itemId, long cost, string currencyId)
        {
            if (_data == null) return false;
            if (_data.Inventory.Contains(itemId)) return false; // Already owned

            long balance = GetBalance(currencyId);
            if (balance < cost)
            {
                _uiService?.ShowNotification($"Not enough {currencyId}!", NotificationType.Error);
                return false;
            }

            // Atomic Transaction
            _data.Balances[currencyId] = balance - cost;
            _data.Inventory.Add(itemId);

            Save();

            OnBalanceChanged?.Invoke(currencyId, _data.Balances[currencyId]);
            OnItemUnlocked?.Invoke(itemId);

            _uiService?.ShowNotification($"Purchased {itemId}!", NotificationType.Success);
            return true;
        }

        public bool HasItem(string itemId)
        {
            return _data != null && _data.Inventory.Contains(itemId);
        }

        private void Save()
        {
            _saveService.SaveData(EconomyKeys.SaveKey, _data);
        }
    }
}
