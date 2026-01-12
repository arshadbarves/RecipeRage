using Gameplay.Economy;
using Core.Logging;
using Core.Shared.Events;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Components
{
    /// <summary>
    /// Currency display component - updates UI labels automatically
    /// No MonoBehaviour - pure C# class that subscribes to events
    /// </summary>
    public class CurrencyDisplay
    {
        [Inject]
        private IEventBus _eventBus;

        private readonly VisualElement _root;
        private readonly EconomyService _economyService;

        private Label _coinsLabel;
        private Label _gemsLabel;
        private Button _addCoinsButton;
        private Button _addGemsButton;

        public CurrencyDisplay(VisualElement root, EconomyService economyService)
        {
            _root = root;
            _economyService = economyService;
        }

        public void Initialize()
        {
            _coinsLabel = _root.Q<Label>("coins-amount");
            _gemsLabel = _root.Q<Label>("gems-amount");
            _addCoinsButton = _root.Q<Button>("add-coins-button");
            _addGemsButton = _root.Q<Button>("add-gems-button");

            if (_coinsLabel == null) GameLogger.LogWarning("coins-amount label not found");
            if (_gemsLabel == null) GameLogger.LogWarning("gems-amount label not found");

            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<CurrencyResetEvent>(OnCurrencyReset);

            SetupButtons();
            UpdateUI(
                (int)_economyService.GetBalance(EconomyKeys.CurrencyCoins),
                (int)_economyService.GetBalance(EconomyKeys.CurrencyGems)
            );
        }

        private void SetupButtons()
        {
            if (_addCoinsButton != null) _addCoinsButton.clicked += OnAddCoinsClicked;
            if (_addGemsButton != null) _addGemsButton.clicked += OnAddGemsClicked;
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt) => UpdateUI(evt.Coins, evt.Gems);
        private void OnCurrencyReset(CurrencyResetEvent evt) => UpdateUI(0, 0);

        private void UpdateUI(int coins, int gems)
        {
            if (_coinsLabel != null) _coinsLabel.text = coins.ToString();
            if (_gemsLabel != null) _gemsLabel.text = gems.ToString();
        }

        private void OnAddCoinsClicked()
        {
            GameLogger.Log("Add coins clicked");
#if UNITY_EDITOR
            _economyService.AddCurrency(EconomyKeys.CurrencyCoins, 500);
#endif
        }

        private void OnAddGemsClicked()
        {
            GameLogger.Log("Add gems clicked");
#if UNITY_EDITOR
            _economyService.AddCurrency(EconomyKeys.CurrencyGems, 50);
#endif
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Unsubscribe<CurrencyResetEvent>(OnCurrencyReset);
            if (_addCoinsButton != null) _addCoinsButton.clicked -= OnAddCoinsClicked;
            if (_addGemsButton != null) _addGemsButton.clicked -= OnAddGemsClicked;
        }
    }
}
