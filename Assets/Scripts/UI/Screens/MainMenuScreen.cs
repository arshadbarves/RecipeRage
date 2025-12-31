using Core.Bootstrap;
using Core.Animation;
using Core.Events;
using Core.Logging;
using Core.Networking.Interfaces;
using UI.Components;
using UI.Components.Tabs;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Core.SaveSystem;
using Core.State;
using RecipeRage.Modules.Auth.Core;
using Core.Currency;
using System.Collections.Generic;
using Core.Networking;
using Core.Characters;
using Core.State.States;
using UI;

namespace UI.Screens
{
    /// <summary>
    /// Main menu screen - container for tab-based navigation
    /// </summary>
    [UIScreen(UIScreenType.MainMenu, UIScreenCategory.Screen, "Screens/MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Dependencies

        [Inject] private IUIService _uiService;
        [Inject] private ISaveService _saveService;
        [Inject] private IEventBus _eventBus;
        [Inject] private SessionManager _sessionManager;
        [Inject] private IGameStateManager _stateManager;
        [Inject] private IAuthService _authService;
        [Inject] private IAnimationService _animationService;
        [Inject] private ILoggingService _loggingService;

        #endregion

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private CharacterTabComponent _characterTab;
        private SettingsTabComponent _settingsTab;
        private CurrencyDisplay _currencyDisplay;

        private Label _playerNameLabel;
        private Label _playerLevelLabel;

        protected override void OnInitialize()
        {
            InitializeCurrencyDisplay();
            InitializePlayerCard();
            InitializeAllTabs();
            SetupPointerInteractions();
        }

        protected override void OnShow()
        {
            UpdatePlayerInfo();
            _lobbyTab?.PlayIntroAnimations(null); 
        }

        public override void Update(float deltaTime) => _lobbyTab?.Update(deltaTime);

        protected override void OnDispose()
        {
            _lobbyTab?.Dispose();
            _settingsTab?.Dispose();
            _currencyDisplay?.Dispose();
        }

        private void SetupPointerInteractions()
        {
            VisualElement topBar = GetElement<VisualElement>("top-bar");
            if (topBar != null) topBar.pickingMode = PickingMode.Ignore;
        }

        private void InitializeCurrencyDisplay()
        {
            if (_sessionManager?.IsSessionActive == true)
            {
                var currencyService = _sessionManager.SessionContainer.Resolve<ICurrencyService>();
                _currencyDisplay = new CurrencyDisplay(Container, _eventBus, currencyService);
            }
        }

        private void InitializePlayerCard()
        {
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
            GetElement<Button>("player-card-button")?.RegisterCallback<ClickEvent>(_ => _uiService?.ShowScreen(UIScreenType.Profile));
        }

        private void InitializeAllTabs()
        {
            if (_sessionManager?.IsSessionActive == false) return;
            var sessionContainer = _sessionManager.SessionContainer;
            
            var lobbyRoot = GetElement<VisualElement>("lobby-root");
            if (lobbyRoot != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                _lobbyTab = new LobbyTabComponent(networking?.MatchmakingService, _stateManager, _uiService, _animationService, _loggingService);
                _lobbyTab.Initialize(lobbyRoot);
            }

            var characterRoot = GetElement<VisualElement>("character-root");
            if (characterRoot != null)
            {
                _characterTab = new CharacterTabComponent(sessionContainer.Resolve<ICharacterService>(), _uiService);
                _characterTab.Initialize(characterRoot);
            }

            var shopRoot = GetElement<VisualElement>("shop-root");
            if (shopRoot != null)
            {
                _shopTab = new ShopTabComponent(sessionContainer.Resolve<ICurrencyService>());
                _shopTab.Initialize(shopRoot);
            }

            var settingsRoot = GetElement<VisualElement>("settings-root");
            if (settingsRoot != null)
            {
                _settingsTab = new SettingsTabComponent(_saveService, _uiService, _authService);
                _settingsTab.Initialize(settingsRoot);
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_saveService == null) return;
            var stats = _saveService.GetPlayerStats();
            var progress = _saveService.GetPlayerProgress();
            if (_playerNameLabel != null) _playerNameLabel.text = stats.PlayerName?.ToUpper() ?? "GUEST";
            if (_playerLevelLabel != null) _playerLevelLabel.text = $"LV. {progress.HighestLevel}";
        }
    }
}