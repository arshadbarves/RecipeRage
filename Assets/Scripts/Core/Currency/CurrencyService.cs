using Core.Events;
using Core.Logging;
using Core.SaveSystem;
using UI;
using UI.Screens;
using Core.Bootstrap;

namespace Core.Currency
{
    /// <summary>
    /// Currency service implementation
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private const string SAVE_KEY = "player_currency";
        private readonly ISaveService _saveService;
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;

        private int _coins;
        private int _gems;

        public int Coins => _coins;
        public int Gems => _gems;

        public CurrencyService(ISaveService saveService, IEventBus eventBus, IUIService uiService)
        {
            _saveService = saveService;
            _eventBus = eventBus;
            _uiService = uiService;
            LoadFromSave();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _coins += amount;
            SaveToDisk();
            PublishCurrencyChanged();
            _uiService?.ShowNotification($"+{amount} coins!", NotificationType.Success, 2f);
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            _gems += amount;
            SaveToDisk();
            PublishCurrencyChanged();
            _uiService?.ShowNotification($"+{amount} gems!", NotificationType.Success, 2f);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return false;
            if (_coins >= amount)
            {
                _coins -= amount;
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
            if (_gems >= amount)
            {
                _gems -= amount;
                SaveToDisk();
                PublishCurrencyChanged();
                return true;
            }
            _uiService?.ShowNotification("Not enough gems", NotificationType.Error);
            return false;
        }

        public void LoadFromSave()
        {
            CurrencyData data = _saveService.LoadData<CurrencyData>(SAVE_KEY);
            if (data != null)
            {
                _coins = data.Coins;
                _gems = data.Gems;
            }
            else
            {
                _coins = 1250;
                _gems = 85;
                SaveToDisk();
            }
            PublishCurrencyChanged();
        }

        public void SaveToDisk()
        {
            CurrencyData data = new CurrencyData(_coins, _gems);
            _saveService.SaveData(SAVE_KEY, data);
        }

        public string FormatCurrency(int amount) => amount.ToString();

        private void PublishCurrencyChanged()
        {
            _eventBus.Publish(new CurrencyChangedEvent { Coins = _coins, Gems = _gems });
        }
    }
}