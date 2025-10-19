using Core.Bootstrap;
using UI.Components;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Main menu screen - container for tab-based navigation
    /// Delegates content to tab components (MainMenuUI, ShopUI, SkinsUI, SettingsUI)
    /// </summary>
    [UIScreen(UIScreenType.Menu, UIScreenPriority.Menu, "MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Tab Components

        private MainMenuUI _mainMenuUI;
        private ShopUI _shopUI;
        private SkinsUI _skinsUI;
        private SettingsUI _settingsUI;

        #endregion

        #region Other Components

        private CurrencyDisplay _currencyDisplay;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            ConfigureTopBarPointerEvents();
            InitializeCurrencyDisplay();
            InitializeTabContent();

            Debug.Log("[MainMenuScreen] Initialized");
        }

        protected override void OnShow()
        {
            // Screen is being shown
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
            _currencyDisplay?.Dispose();
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

            // Main Menu/Lobby tab
            VisualElement lobbyView = GetElement<VisualElement>("lobby-view");
            if (lobbyView != null)
            {
                _mainMenuUI = new MainMenuUI();
                _mainMenuUI.Initialize(Container);
                Debug.Log("[MainMenuScreen] Initialized Main Menu tab");
            }
            else
            {
                Debug.LogWarning("[MainMenuScreen] Lobby view not found");
            }

            // Skins tab
            VisualElement skinsRoot = GetElement<VisualElement>("skins-root");
            if (skinsRoot != null)
            {
                _skinsUI = new SkinsUI();
                _skinsUI.Initialize(skinsRoot);
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
                _shopUI = new ShopUI();
                _shopUI.Initialize(shopRoot);
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
                var saveService = GameBootstrap.Services?.SaveService;
                if (saveService != null)
                {
                    _settingsUI = new SettingsUI(saveService);
                    _settingsUI.Initialize(settingsRoot);
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
