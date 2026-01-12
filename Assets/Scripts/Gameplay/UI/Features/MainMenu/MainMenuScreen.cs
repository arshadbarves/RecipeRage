using Gameplay.UI.Components;
using Gameplay.UI.Components.Tabs;
using Gameplay.Persistence;
using UnityEngine.UIElements;
using VContainer;
using Core.UI;
using Core.Logging;
using Core.Shared.Events;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Session;

namespace Gameplay.UI.Features.MainMenu
{
    /// <summary>
    /// Main menu screen - Fortnite-style lobby with nav pills and sidebar
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Dependencies

        [Inject] private MainMenuViewModel _viewModel;
        [Inject] private IObjectResolver _container;
        [Inject] private IUIService _uiService;
        [Inject] private IEventBus _eventBus;
        [Inject] private SessionManager _sessionManager;

        #endregion

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private CharacterTabComponent _characterTab;
        private CurrencyDisplay _currencyDisplay;

        private Label _playerLevelLabel;
        private Label _playerNameLabel;
        private Label _goldAmountLabel;
        private Label _gemsAmountLabel;

        private TabView _mainTabs;

        protected override void OnInitialize()
        {
            QueryElements();
            _eventBus?.Subscribe<LogoutEvent>(OnLogout);
        }

        private void QueryElements()
        {
            _goldAmountLabel = GetElement<Label>("gold-amount");
            _gemsAmountLabel = GetElement<Label>("gems-amount");
            _playerNameLabel = GetElement<Label>("player-name-main");
            _playerLevelLabel = GetElement<Label>("player-sub-main");
            _mainTabs = GetElement<TabView>("main-tabs");
        }

        private void OnLogout(LogoutEvent evt) => ClearSessionComponents();

        private void ClearSessionComponents()
        {
            _lobbyTab?.Dispose();
            _currencyDisplay?.Dispose();
            _lobbyTab = null;
            _currencyDisplay = null;
        }

        protected override void OnDispose()
        {
            _eventBus?.Unsubscribe<LogoutEvent>(OnLogout);
            ClearSessionComponents();
        }

        protected override void OnShow()
        {
            UpdatePlayerInfo();
            InitializeSessionComponents();
            _lobbyTab?.PlayIntroAnimations(null);
        }

        private void InitializeSessionComponents()
        {
            if (_sessionManager?.IsSessionActive == false) return;
            if (_lobbyTab != null) return;

            InitializeCurrencyDisplay();
            InitializeAllTabs();
        }

        public override void Update(float deltaTime) => _lobbyTab?.Update(deltaTime);

        private void InitializeCurrencyDisplay()
        {
            if (_sessionManager?.IsSessionActive == true)
            {
                // Currency display is handled separately
            }
        }

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
                _playerLevelLabel.text = $"LVL. {progress?.HighestLevel ?? 0} // VANGUARD";

            if (_playerNameLabel != null)
                _playerNameLabel.text = string.IsNullOrEmpty(stats?.PlayerName) ? "STRYKER" : stats.PlayerName.ToUpper();
        }
    }
}