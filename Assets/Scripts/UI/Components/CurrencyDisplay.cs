using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Modules.Core.Banking.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Components
{
    /// <summary>
    /// Currency display component - updates UI labels automatically
    /// No MonoBehaviour - pure C# class that subscribes to events
    /// Follows Single Responsibility Principle - only handles UI display
    /// </summary>
    public class CurrencyDisplay
    {
        [Inject]
        private IEventBus _eventBus;

        private readonly VisualElement _root;
        private readonly IBankService _bankService;

        private Label _coinsLabel;
        private Label _gemsLabel;
        private Button _addCoinsButton;
        private Button _addGemsButton;

        public CurrencyDisplay(VisualElement root, IBankService bankService)
        {
            _root = root;
            _bankService = bankService;
        }

        public void Initialize()
        {
            // Cache UI elements
            _coinsLabel = _root.Q<Label>("coins-amount");
            _gemsLabel = _root.Q<Label>("gems-amount");
            _addCoinsButton = _root.Q<Button>("add-coins-button");
            _addGemsButton = _root.Q<Button>("add-gems-button");

            // Log missing elements
            if (_coinsLabel == null)
                GameLogger.LogWarning("coins-amount label not found");
            if (_gemsLabel == null)
                GameLogger.LogWarning("gems-amount label not found");

            // Subscribe to currency events
            _eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            _eventBus.Subscribe<CurrencyResetEvent>(OnCurrencyReset);

            // Setup buttons (editor only)
            SetupButtons();

            // Initial update
            UpdateUI((int)_bankService.GetBalance("coins"), (int)_bankService.GetBalance("gems"));
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
                _coinsLabel.text = coins.ToString(); // Simplified formatting
            }

            if (_gemsLabel != null)
            {
                _gemsLabel.text = gems.ToString();
            }
        }

        private void OnAddCoinsClicked()
        {
            GameLogger.Log("Add coins clicked");

#if UNITY_EDITOR
            // Editor only - add test currency
            _bankService.ModifyBalance("coins", 500);
#else
            // Production - open store
#endif
        }

        private void OnAddGemsClicked()
        {
            GameLogger.Log("Add gems clicked");

#if UNITY_EDITOR
            // Editor only - add test currency
            _bankService.ModifyBalance("gems", 50);
#else
            // Production - open store
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
        }
    }
}
