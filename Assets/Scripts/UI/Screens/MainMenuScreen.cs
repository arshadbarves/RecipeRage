using Core.Bootstrap;
using Core.Events;
using Core.Networking;
using Core.Networking.Interfaces;
using UI.Components;
using UI.Components.Tabs;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Main menu screen - container for tab-based navigation
    /// Delegates content to tab components (MainMenuUI, ShopUI, SkinsUI, SettingsUI)
    /// </summary>
    [UIScreen(UIScreenType.MainMenu, UIScreenPriority.Menu, "Screens/MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Tab Components

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private SkinsTabComponent _skinsTab;
        private SettingsTabComponent _settingsTab;

        #endregion

        #region Other Components

        private CurrencyDisplay _currencyDisplay;
        private Button _playerCardButton;
        private Label _playerNameLabel;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            ConfigureTopBarPointerEvents();
            InitializeCurrencyDisplay();
            InitializePlayerCard();
            InitializeTabContent();
            SubscribeToEvents();

            Debug.Log("[MainMenuScreen] Initialized");
        }

        protected override void OnShow()
        {
            // Update player name when screen is shown
            UpdatePlayerName();
        }

        protected override void OnHide()
        {
            // Screen is being hidden
        }

        public override void Update(float deltaTime)
        {
            // Update logic if needed
        }

        protected override void OnDispose()
        {
            UnsubscribeFromEvents();
            _currencyDisplay?.Dispose();

            if (_playerCardButton != null)
            {
                _playerCardButton.clicked -= OnPlayerCardClicked;
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            var eventBus = GameBootstrap.Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Subscribe<PlayerStatsChangedEvent>(OnPlayerStatsChanged);
            }
        }

        private void UnsubscribeFromEvents()
        {
            var eventBus = GameBootstrap.Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<PlayerStatsChangedEvent>(OnPlayerStatsChanged);
            }
        }

        private void OnPlayerStatsChanged(PlayerStatsChangedEvent evt)
        {
            UpdatePlayerName();
        }

        #endregion

        #region Initialization

        private void ConfigureTopBarPointerEvents()
        {
            VisualElement topBar = GetElement<VisualElement>("top-bar");
            if (topBar != null)
            {
                // Make top-bar ignore pointer events so clicks pass through
                topBar.pickingMode = PickingMode.Ignore;

                // Make child elements clickable
                VisualElement playerCard = topBar.Q<VisualElement>("player-card");
                if (playerCard != null)
                {
                    playerCard.pickingMode = PickingMode.Position;
                }

                VisualElement currencyContainer = topBar.Q<VisualElement>("currency-container");
                if (currencyContainer != null)
                {
                    currencyContainer.pickingMode = PickingMode.Position;

                    // Make currency displays and buttons clickable
                    var currencyDisplays = currencyContainer.Query<VisualElement>(className: "currency-display").ToList();
                    foreach (var display in currencyDisplays)
                    {
                        display.pickingMode = PickingMode.Position;
                    }

                    var addButtons = currencyContainer.Query<Button>(className: "add-currency-button").ToList();
                    foreach (var button in addButtons)
                    {
                        button.pickingMode = PickingMode.Position;
                    }
                }
            }
        }

        private void InitializeCurrencyDisplay()
        {
            var services = GameBootstrap.Services;
            if (services != null)
            {
                _currencyDisplay = new CurrencyDisplay(
                    Container,
                    services.EventBus,
                    services.CurrencyService
                );
            }
        }

        private void InitializePlayerCard()
        {
            _playerCardButton = GetElement<Button>("player-card-button");
            _playerNameLabel = GetElement<Label>("player-title");

            if (_playerCardButton != null)
            {
                _playerCardButton.clicked += OnPlayerCardClicked;
            }

            // Update player name
            UpdatePlayerName();
        }

        private void UpdatePlayerName()
        {
            var saveService = GameBootstrap.Services?.SaveService;
            if (saveService != null && _playerNameLabel != null)
            {
                var stats = saveService.GetPlayerStats();
                string displayName = string.IsNullOrEmpty(stats.PlayerName) ? "Guest Player" : stats.PlayerName;
                _playerNameLabel.text = displayName;
            }
        }

        private void OnPlayerCardClicked()
        {
            Debug.Log("[MainMenuScreen] Player card clicked - showing profile");

            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Profile, true, true);
            }
        }

        private void InitializeTabContent()
        {
            TabView tabView = GetElement<TabView>("main-tabs");
            if (tabView != null)
            {
                tabView.RegisterCallback<ChangeEvent<int>>(OnTabChanged);
                InitializeAllTabs();
            }
            else
            {
                Debug.LogError("[MainMenuScreen] TabView not found!");
            }
        }

        private void OnTabChanged(ChangeEvent<int> evt)
        {
            Debug.Log($"[MainMenuScreen] Tab changed to index: {evt.newValue}");
        }

        private void InitializeAllTabs()
        {
            Debug.Log("[MainMenuScreen] Initializing all tabs...");

            var services = GameBootstrap.Services;
            if (services == null)
            {
                Debug.LogError("[MainMenuScreen] GameBootstrap.Services is null!");
                return;
            }

            // Lobby tab (Main Menu/Home)
            // Get the chef-tab which contains both lobby-view and bottom-bar
            TabView tabView = GetElement<TabView>("main-tabs");
            Tab chefTab = tabView?.Q<Tab>("chef-tab");
            
            if (chefTab != null)
            {
                // Get matchmaking service from networking services
                IMatchmakingService matchmakingService = services.NetworkingServices?.MatchmakingService;

                if (matchmakingService != null && services.StateManager != null)
                {
                    _lobbyTab = new LobbyTabComponent(matchmakingService, services.StateManager);
                    _lobbyTab.Initialize(chefTab); // Pass the tab root, not just lobby-view
                    Debug.Log("[MainMenuScreen] Initialized Lobby tab with chef-tab root");
                }
                else
                {
                    Debug.LogError("[MainMenuScreen] Failed to get required services for LobbyTab");
                }
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Chef tab not found");
            }

            // Skins tab
            VisualElement skinsRoot = GetElement<VisualElement>("skins-root");
            if (skinsRoot != null)
            {
                _skinsTab = new SkinsTabComponent();
                _skinsTab.Initialize(skinsRoot);
                Debug.Log("[MainMenuScreen] Initialized Skins tab");
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Skins root not found");
            }

            // Shop tab
            VisualElement shopRoot = GetElement<VisualElement>("shop-root");
            if (shopRoot != null)
            {
                _shopTab = new ShopTabComponent();
                _shopTab.Initialize(shopRoot);
                Debug.Log("[MainMenuScreen] Initialized Shop tab");
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Shop root not found");
            }

            // Settings tab
            VisualElement settingsRoot = GetElement<VisualElement>("settings-root");
            if (settingsRoot != null)
            {
                if (services.SaveService != null)
                {
                    _settingsTab = new SettingsTabComponent(services.SaveService);
                    _settingsTab.Initialize(settingsRoot);
                    Debug.Log("[MainMenuScreen] Initialized Settings tab");
                }
                else
                {
                    Debug.LogError("[MainMenuScreen] SaveService not available!");
                }
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Settings root not found");
            }
        }

        #endregion
    }
}
