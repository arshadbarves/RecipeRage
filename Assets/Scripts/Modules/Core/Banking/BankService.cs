using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Currency;
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

        public int Coins => _data?.Coins ?? 0;
        public int Gems => _data?.Gems ?? 0;

        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;

        public BankService(IBankBackend backend, IEventBus eventBus, IUIService uiService)
        {
            _backend = backend;
            _eventBus = eventBus;
            _uiService = uiService;
        }

        public async Task InitializeAsync()
        {
            _data = await _backend.LoadDataAsync();
            PublishCurrencyChanged();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _data.Coins += amount;
            SaveToDisk();
            PublishCurrencyChanged();
            _uiService?.ShowNotification($"+{amount} coins!", NotificationType.Success, 2f);
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            _data.Gems += amount;
            SaveToDisk();
            PublishCurrencyChanged();
            _uiService?.ShowNotification($"+{amount} gems!", NotificationType.Success, 2f);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return false;
            if (_data.Coins >= amount)
            {
                _data.Coins -= amount;
                SaveToDisk();
                PublishCurrencyChanged();
                return true;
            }
            _uiService?.ShowNotification("Not enough coins", NotificationType.Error);
            return false;
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0) return false;
            if (_data.Gems >= amount)
            {
                _data.Gems -= amount;
                SaveToDisk();
                PublishCurrencyChanged();
                return true;
            }
            _uiService?.ShowNotification("Not enough gems", NotificationType.Error);
            return false;
        }

        public List<string> GetUnlockedSkins() => new List<string>(_data.UnlockedSkinIds);

        public bool IsSkinUnlocked(string skinId) => _data.UnlockedSkinIds.Contains(skinId);

        public bool UnlockSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || IsSkinUnlocked(skinId)) return false;
            _data.UnlockedSkinIds.Add(skinId);
            SaveToDisk();
            OnSkinUnlocked?.Invoke(skinId);
            return true;
        }

        public string GetEquippedSkinId(int characterId)
        {
            return _data.EquippedSkinIds.TryGetValue(characterId, out var skinId) ? skinId : null;
        }

        public bool EquipSkin(int characterId, string skinId)
        {
            if (!IsSkinUnlocked(skinId)) return false;
            _data.EquippedSkinIds[characterId] = skinId;
            SaveToDisk();
            OnSkinEquipped?.Invoke(characterId, skinId);
            return true;
        }

        public void LoadFromSave()
        {
            // Legacy support - using sync version if needed, but we prefer async Initialize
            _data = _backend.LoadDataAsync().GetAwaiter().GetResult();
            PublishCurrencyChanged();
        }

        public void SaveToDisk()
        {
            _backend.SaveDataAsync(_data).Wait();
        }

        public string FormatCurrency(int amount) => amount.ToString();

        private void PublishCurrencyChanged()
        {
            _eventBus?.Publish(new CurrencyChangedEvent { Coins = Coins, Gems = Gems });
        }
    }
}
