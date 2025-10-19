using Core.Bootstrap;
using Core.Currency;
using Core.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    /// <summary>
    /// Currency display component - updates UI labels automatically
    /// No MonoBehaviour - pure C# class that subscribes to events
    /// Follows Single Responsibility Principle - only handles UI display
    /// </summary>
    public class CurrencyDisplay
    {
        private readonly VisualElement _root;
        private readonly IEventBus _eventBus;
        private readonly ICurrencyService _currencyService;

        private Label _coinsLabel;
        private Label _gemsLabel;
        private Button _addCoinsButton;
        private Button _addGemsButton;

        public CurrencyDisplay(VisualElement root, IEventBus eventBus, ICurrencyService currencyService)
        {
            _root = root;
            _eventBus = eventBus;
            _currencyService = currencyService;

            Initialize();
        }

        private void Initialize()
        {
            Debug.Log("[CurrencyDisplay] Initializing");

            // Cache UI elements
            _coinsLabel = _root.Q<Label>("coins-amount");
            _gemsLabel = _root.Q<Label>("gems-amount");
            _addCoinsButton = _root.Q<Button>("add-coins-button");
            _addGemsButton = _root.Q<Button>("add-gems-button");

            // Log missing elements
            if (_coinsLabel == null)
                Debug.LogWarning("[CurrencyDisplay] coins-amount label not found");
            if (_gemsLabel == null)
                Debug.LogWarning("[CurrencyDisplay] gems-amount label not found");

            // Subscribe to currency events
            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<CurrencyResetEvent>(OnCurrencyReset);

            // Setup buttons (editor only)
            SetupButtons();

            // Initial update
            UpdateUI(_currencyService.Coins, _currencyService.Gems);

            Debug.Log("[CurrencyDisplay] Initialized successfully");
        }

        private void SetupButtons()
        {
            if (_addCoinsButton != null)
            {
                _addCoinsButton.clicked += OnAddCoinsClicked;
            }

            if (_addGemsButton != null)
            {
                _addGemsButton.clicked += OnAddGemsClicked;
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            UpdateUI(evt.Coins, evt.Gems);
        }

        private void OnCurrencyReset(CurrencyResetEvent evt)
        {
            UpdateUI(0, 0);
        }

        private void UpdateUI(int coins, int gems)
        {
            if (_coinsLabel != null)
            {
                _coinsLabel.text = _currencyService.FormatCurrency(coins);
                Debug.Log($"[CurrencyDisplay] Updated coins label to: {_coinsLabel.text}");
            }

            if (_gemsLabel != null)
            {
                _gemsLabel.text = _currencyService.FormatCurrency(gems);
                Debug.Log($"[CurrencyDisplay] Updated gems label to: {_gemsLabel.text}");
            }
        }

        private void OnAddCoinsClicked()
        {
            Debug.Log("[CurrencyDisplay] Add coins clicked");

#if UNITY_EDITOR
            // Editor only - add test currency
            _currencyService.AddCoins(500);
#else
            // Production - open store
            Debug.Log("[CurrencyDisplay] Opening store for coins purchase");
            // TODO: Open in-app purchase dialog
#endif
        }

        private void OnAddGemsClicked()
        {
            Debug.Log("[CurrencyDisplay] Add gems clicked");

#if UNITY_EDITOR
            // Editor only - add test currency
            _currencyService.AddGems(50);
#else
            // Production - open store
            Debug.Log("[CurrencyDisplay] Opening store for gems purchase");
            // TODO: Open in-app purchase dialog
#endif
        }

        /// <summary>
        /// Cleanup - unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            _eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Unsubscribe<CurrencyResetEvent>(OnCurrencyReset);

            if (_addCoinsButton != null)
            {
                _addCoinsButton.clicked -= OnAddCoinsClicked;
            }

            if (_addGemsButton != null)
            {
                _addGemsButton.clicked -= OnAddGemsClicked;
            }

            Debug.Log("[CurrencyDisplay] Disposed");
        }
    }
}
