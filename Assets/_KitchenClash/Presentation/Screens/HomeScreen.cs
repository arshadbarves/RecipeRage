using KitchenClash.Presentation.Components;
using KitchenClash.Application.Services;
using KitchenClash.Application;
using UnityEngine.UIElements;
using VContainer;
using KitchenClash.Presentation;
using KitchenClash.Domain;
using Playcenter.UI.Toolkit;
using KitchenClash.Presentation.Extensions;
using KitchenClash.Presentation.ViewModels;
using Playcenter.Shell;
using Playcenter.Services;
using Playcenter.UI;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/MainMenuViewTemplate")]
    public class HomeScreen : BaseUIScreen
    {
        [Inject] private MainMenuViewModel _viewModel;
        [Inject] private IEventBus _eventBus;
        [Inject] private ISessionContext _sessionContext;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private IDailyStreakService _dailyStreakService;

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private CharacterTabComponent _characterTab;
        private IEconomyService _economyService;
        private bool _dailyStreakChecked;

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
                _eventBus?.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                _economyService = null;
            }

            _lobbyTab?.Dispose();
            _lobbyTab = null;

            _characterTab?.Dispose();
            _characterTab = null;

            _shopTab?.Dispose();
            _shopTab = null;
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
                _localizationManager.Bind(tabs.Q<Tab>("tab-lobby"),   LocKeys.MainTabPlay,    this);
                _localizationManager.Bind(tabs.Q<Tab>("tab-compete"), LocKeys.MainTabCompete, this);
                _localizationManager.Bind(tabs.Q<Tab>("tab-shop"),    LocKeys.MainTabShop,    this);
                // tab-season uses a hard-coded label in UXML; no localization key needed yet
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
            _lobbyTab?.PlayIntroAnimations();
            CheckDailyStreak();
        }

        private void CheckDailyStreak()
        {
            if (_dailyStreakChecked) return;
            _dailyStreakChecked = true;

            try
            {
                if (_dailyStreakService != null && _dailyStreakService.CanClaim(System.DateTime.UtcNow))
                {
                    UIService?.PushPopup<DailyStreakScreen>();
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[HomeScreen] Daily streak check failed: {ex.Message}");
            }
        }

        private void SubscribeToCurrencyUpdates()
        {
            if (!_sessionContext.IsSessionActive) return;

            _economyService = _sessionContext.EconomyService;
            if (_economyService == null) return;

            // Subscribe to balance changes
            _eventBus?.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);

            // Initial update
            UpdateCurrencyLabel(EconomyKeys.CurrencyCoins, _economyService.GetBalance(EconomyKeys.CurrencyCoins));
            UpdateCurrencyLabel(EconomyKeys.CurrencyGems, _economyService.GetBalance(EconomyKeys.CurrencyGems));
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            UpdateCurrencyLabel(EconomyKeys.CurrencyCoins, evt.Coins);
            UpdateCurrencyLabel(EconomyKeys.CurrencyGems, evt.Gems);
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
            if (!_sessionContext.IsSessionActive) return;
            if (_lobbyTab != null) return;
            InitializeAllTabs();
        }

        public override void Update(float deltaTime)
        {
            _lobbyTab?.Update(deltaTime);
            _characterTab?.Update(deltaTime);
        }

        private void InitializeAllTabs()
        {
            if (!_sessionContext.IsSessionActive || _viewModel == null) return;

            _viewModel.Initialize();
            
            // Tabs need session-scoped services; inject via ISessionContext (not SessionManager).
            var lobbyRoot = GetElement<VisualElement>("lobby-root");
            if (lobbyRoot != null)
            {
                _lobbyTab = new LobbyTabComponent(_viewModel.LobbyVM);
                _sessionContext.Inject(_lobbyTab);
                _lobbyTab.Initialize(lobbyRoot);
            }

            var characterRoot = GetElement<VisualElement>("character-root");
            if (characterRoot != null)
            {
                _characterTab = new CharacterTabComponent(_sessionContext.CharacterService);
                _sessionContext.Inject(_characterTab);
                _characterTab.Initialize(characterRoot);
            }

            var shopRoot = GetElement<VisualElement>("shop-root");
            if (shopRoot != null)
            {
                _shopTab = new ShopTabComponent(_viewModel.ShopVM);
                _sessionContext.Inject(_shopTab);
                _shopTab.Initialize(shopRoot);
            }
        }

        private void UpdatePlayerInfo()
        {
            if (!_sessionContext.IsSessionActive) return;

            var playerDataService = _sessionContext.PlayerDataService;
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
