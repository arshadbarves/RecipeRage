using Gameplay.UI.Components.Tabs;
using Gameplay.Persistence;
using Gameplay.Economy;
using UnityEngine.UIElements;
using VContainer;
using Core.UI;
using Core.Shared.Events;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Session;
using Gameplay.UI.Extensions;
using Gameplay.UI.Localization;
using UnityEngine;

namespace Gameplay.UI.Features.MainMenu
{
    [UIScreen(UIScreenCategory.Screen, "Screens/MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        [Inject] private MainMenuViewModel _viewModel;
        [Inject] private IObjectResolver _container;
        [Inject] private IUIService _uiService;
        [Inject] private IEventBus _eventBus;
        [Inject] private SessionManager _sessionManager;
        [Inject] private Core.Localization.ILocalizationManager _localizationManager;

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private CharacterTabComponent _characterTab;
        private EconomyService _economyService;

        private Label _playerLevelLabel;
        private Label _playerNameLabel;
        private Label _playerTagLabel;
        private Label _goldAmountLabel;
        private Label _gemsAmountLabel;

        protected override void OnInitialize()
        {
            QueryElements();
            _eventBus?.Subscribe<LogoutEvent>(OnLogout);
            BindLocalization();
        }

        private void QueryElements()
        {
            _goldAmountLabel = GetElement<Label>("gold-amount");
            _gemsAmountLabel = GetElement<Label>("gems-amount");
            _playerNameLabel = GetElement<Label>("player-name-main");
            _playerLevelLabel = GetElement<Label>("player-sub-main");
            _playerTagLabel = GetElement<Label>("player-nick");
        }

        private void OnLogout(LogoutEvent evt) => ClearSessionComponents();

        private void ClearSessionComponents()
        {
            // Unsubscribe from economy events
            if (_economyService != null)
            {
                _economyService.OnBalanceChanged -= OnBalanceChanged;
                _economyService = null;
            }

            _lobbyTab?.Dispose();
            _lobbyTab = null;
        }

        protected override void OnDispose()
        {
            _eventBus?.Unsubscribe<LogoutEvent>(OnLogout);
            ClearSessionComponents();

            _localizationManager?.UnregisterAll(this);
        }

        private void BindLocalization()
        {
            if (_localizationManager == null) return;

            // Main Menu Tabs
            var tabs = GetElement<TabView>("main-tabs");
            if (tabs != null)
            {
                _localizationManager.Bind(tabs.Q<Tab>("tab-lobby"), LocKeys.MainTabPlay, this);
                _localizationManager.Bind(tabs.Q<Tab>("tab-compete"), LocKeys.MainTabCompete, this);
                _localizationManager.Bind(tabs.Q<Tab>("tab-shop"), LocKeys.MainTabShop, this);
            }

            // Cascade refresh to tabs
            _lobbyTab?.RefreshLocalization();

            // Static Player Labels with localized parts
            _localizationManager.RegisterBinding(this, LocKeys.MainMenuLvlPrefix, _ => UpdatePlayerInfo());
        }

        private void RefreshLocalization()
        {
            BindLocalization();
        }

        protected override void OnShow()
        {
            UpdatePlayerInfo();
            SubscribeToCurrencyUpdates();
            InitializeSessionComponents();
            _lobbyTab?.PlayIntroAnimations(null);
        }

        private void SubscribeToCurrencyUpdates()
        {
            if (_sessionManager?.IsSessionActive != true) return;

            _economyService = _sessionManager.SessionContainer?.Resolve<EconomyService>();
            if (_economyService == null) return;

            // Subscribe to balance changes
            _economyService.OnBalanceChanged += OnBalanceChanged;

            // Initial update
            UpdateCurrencyLabel(EconomyKeys.CurrencyCoins, _economyService.GetBalance(EconomyKeys.CurrencyCoins));
            UpdateCurrencyLabel(EconomyKeys.CurrencyGems, _economyService.GetBalance(EconomyKeys.CurrencyGems));
        }

        private void OnBalanceChanged(string currencyId, long newBalance)
        {
            UpdateCurrencyLabel(currencyId, newBalance);
        }

        private void UpdateCurrencyLabel(string currencyId, long balance)
        {
            if (currencyId == EconomyKeys.CurrencyCoins && _goldAmountLabel != null)
                _goldAmountLabel.text = balance.ToString();
            else if (currencyId == EconomyKeys.CurrencyGems && _gemsAmountLabel != null)
                _gemsAmountLabel.text = balance.ToString();
        }

        private void InitializeSessionComponents()
        {
            if (_sessionManager?.IsSessionActive == false) return;
            if (_lobbyTab != null) return;
            InitializeAllTabs();
        }

        public override void Update(float deltaTime) => _lobbyTab?.Update(deltaTime);

        private void InitializeAllTabs()
        {
            if (_sessionManager?.IsSessionActive == false || _viewModel == null) return;

            _viewModel.Initialize();
            var sessionContainer = _sessionManager.SessionContainer;

            var lobbyRoot = GetElement<VisualElement>("lobby-root");
            if (lobbyRoot != null)
            {
                _lobbyTab = new LobbyTabComponent(_viewModel.LobbyVM);
                sessionContainer.Inject(_lobbyTab);
                _lobbyTab.Initialize(lobbyRoot);
            }

            var shopRoot = GetElement<VisualElement>("shop-root");
            if (shopRoot != null)
            {
                _shopTab = new ShopTabComponent(_viewModel.ShopVM);
                sessionContainer.Inject(_shopTab);
                _shopTab.Initialize(shopRoot);
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_sessionManager?.IsSessionActive != true) return;

            var playerDataService = _sessionManager.SessionContainer?.Resolve<PlayerDataService>();
            if (playerDataService == null) return;

            var stats = playerDataService.GetStats();
            var progress = playerDataService.GetProgress();

            if (_playerLevelLabel != null)
            {
                string lvlPrefix = _localizationManager?.GetText(LocKeys.MainMenuLvlPrefix) ?? "LVL.";
                string rankSuffix = _localizationManager?.GetText(LocKeys.MainMenuRankVanguard) ?? "VANGUARD";
                _playerLevelLabel.text = $"{lvlPrefix} {progress?.HighestLevel ?? 0} // {rankSuffix}";
            }

            var playerName = string.IsNullOrEmpty(stats?.PlayerName) ? "STRYKER" : stats.PlayerName.ToUpper();

            if (_playerNameLabel != null)
                _playerNameLabel.text = playerName;

            if (_playerTagLabel != null)
                _playerTagLabel.text = playerName;
        }
    }
}