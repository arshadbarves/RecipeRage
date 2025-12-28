using System.Collections.Generic;
using Core.Bootstrap;
using Core.Animation;
using Core.Events;
using Core.Logging;
using Core.Networking;
using Core.Networking.Interfaces;
using DG.Tweening;
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
    [UIScreen(UIScreenType.MainMenu, UIScreenCategory.Screen, "Screens/MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Tab Components

        private LobbyTabComponent _lobbyTab;
        private ShopTabComponent _shopTab;
        private CharacterTabComponent _characterTab;
        private SettingsTabComponent _settingsTab;
        private TabSystem _tabSystem;

        #endregion

        #region Other Components

        private CurrencyDisplay _currencyDisplay;
        private Button _playerCardButton;
        private Label _playerNameLabel;
        private IUIAnimator _animator;

        #endregion

        #region Lifecycle

        private bool _isDataInitialized;

        protected override void OnInitialize()
        {
            ApplyProceduralBackground();
            ApplyTopBarVignette();
            ConfigureTopBarPointerEvents();
            HideAnimatedElements();
        }

        protected override void OnShow()
        {
            if (!_isDataInitialized)
            {
                InitializeData();
                _isDataInitialized = true;
            }

            UpdatePlayerName();

            // Default to Lobby BEFORE animations so elements are visible to animate
            _tabSystem?.SwitchToTab("Lobby", true);

            PlayIntroAnimations();
        }

        private void InitializeData()
        {
            // Initialize animator
            _animator = new DOTweenUIAnimator();

            InitializeCurrencyDisplay();
            InitializePlayerCard();
            InitializeTabContent();
            SubscribeToEvents();

            // Fix TabView header blocking top bar
            TabView tabView = GetElement<TabView>("main-tabs");
            if (tabView != null)
            {
                var headerContainer = tabView.Q(className: "unity-tab-view__header-container");
                if (headerContainer != null) headerContainer.pickingMode = PickingMode.Ignore;

                var tabHeader = tabView.Q(className: "unity-tab-view__header");
                if (tabHeader != null) tabHeader.pickingMode = PickingMode.Ignore;
            }
        }

        protected override void OnHide()
        {
        }

        public override void Update(float deltaTime)
        {
            _tabSystem?.Update(deltaTime);
        }

        protected override void OnDispose()
        {
            UnsubscribeFromEvents();
            _currencyDisplay?.Dispose();
            _tabSystem?.Dispose();

            if (_playerCardButton != null)
            {
                _playerCardButton.clicked -= OnPlayerCardClicked;
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            var eventBus = Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Subscribe<PlayerStatsChangedEvent>(OnPlayerStatsChanged);
            }
        }

        private void UnsubscribeFromEvents()
        {
            var eventBus = Services?.EventBus;
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

                // Make system icons clickable
                var systemContainer = topBar.Q<VisualElement>("system-container");
                if (systemContainer != null)
                {
                    systemContainer.pickingMode = PickingMode.Position;
                }

                RegisterSystemIconListeners();
            }
        }

        private void RegisterSystemIconListeners()
        {
            // Friends
            var friendsBtn = Container.Q<VisualElement>("sys-icon-friends");
            if (friendsBtn != null)
            {
                friendsBtn.pickingMode = PickingMode.Position;
                friendsBtn.RegisterCallback<ClickEvent>(evt => OnFriendsClicked());
            }

            // Settings
            var settingsBtn = Container.Q<VisualElement>("sys-icon-settings");
            if (settingsBtn != null)
            {
                settingsBtn.pickingMode = PickingMode.Position;
                settingsBtn.RegisterCallback<ClickEvent>(evt => OnSettingsClicked());
            }

            // Leaderboard
            var leaderboardBtn = Container.Q<VisualElement>("sys-icon-leaderboard");
            if (leaderboardBtn != null)
            {
                leaderboardBtn.pickingMode = PickingMode.Position;
                leaderboardBtn.RegisterCallback<ClickEvent>(evt => OnLeaderboardClicked());
            }
        }

        private void OnFriendsClicked()
        {
            GameLogger.Log("Opening Friends Popup");
            var uiService = Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.FriendsPopup, true, true);
            }
        }

        private void OnSettingsClicked()
        {
            // Settings is a tab, so switch to it
            _tabSystem?.SwitchToTab("Settings");
        }

        private void OnLeaderboardClicked()
        {
            GameLogger.Log("Leaderboard clicked");
            // TODO: Show leaderboard
        }

        private void InitializeCurrencyDisplay()
        {
            // Currency is in Session scope
            if (Services?.Session != null)
            {
                _currencyDisplay = new CurrencyDisplay(
                    Container,
                    Services.EventBus,
                    Services.Session.CurrencyService
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
            var saveService = Services?.SaveService;
            if (saveService != null && _playerNameLabel != null)
            {
                var stats = saveService.GetPlayerStats();
                string displayName = string.IsNullOrEmpty(stats.PlayerName) ? "Guest Player" : stats.PlayerName;
                _playerNameLabel.text = displayName;
            }
        }

        private void OnPlayerCardClicked()
        {

            var uiService = Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Profile, true, true);
            }
        }

        private void InitializeTabContent()
        {
            TabView tabView = GetElement<TabView>("main-tabs");
            if (tabView == null)
            {
                GameLogger.LogError("TabView not found!");
                return;
            }

            // Initialize TabSystem
            _tabSystem = new TabSystem(Container, _animator);

            InitializeAllTabs();
        }

        private void InitializeAllTabs()
        {
            if (Services?.Session == null)
            {
                GameLogger.LogError("Services or Session is null!");
                return;
            }

            TabView tabView = GetElement<TabView>("main-tabs");
            var tabHeader = tabView.Q(className: "unity-tab-view__header");
            var tabButtons = tabHeader.Query<Button>(className: "unity-tab-button").ToList();

            // Synchronize TabSystem with TabView
            _tabSystem.OnTabChanged += (tabId) => {
                int index = tabId switch {
                    "Lobby" => 0,
                    "Shop" => 1,
                    "Character" => 2,
                    "Settings" => 3,
                    _ => 0
                };
                tabView.selectedTabIndex = index;
            };

            // Lobby tab (Index 0)
            VisualElement lobbyRoot = GetElement<VisualElement>("lobby-content");
            if (lobbyRoot != null && tabButtons.Count > 0)
            {
                IMatchmakingService matchmakingService = Services.Session.NetworkingServices?.MatchmakingService;
                _lobbyTab = new LobbyTabComponent(matchmakingService, Services.StateManager);
                _lobbyTab.Initialize(lobbyRoot);
                _tabSystem.AddTab("Lobby", tabButtons[0], _lobbyTab);
            }

            // Shop tab (Index 1)
            VisualElement shopRoot = GetElement<VisualElement>("shop-content");
            if (shopRoot != null && tabButtons.Count > 1)
            {
                _shopTab = new ShopTabComponent();
                _shopTab.Initialize(shopRoot);
                _tabSystem.AddTab("Shop", tabButtons[1], _shopTab);
            }

            // Character tab (Index 2)
            VisualElement characterRoot = GetElement<VisualElement>("character-content");
            if (characterRoot != null && tabButtons.Count > 2)
            {
                _characterTab = new CharacterTabComponent();
                _characterTab.Initialize(characterRoot);
                _tabSystem.AddTab("Character", tabButtons[2], _characterTab);
            }

            // Settings tab (Index 3)
            VisualElement settingsRoot = GetElement<VisualElement>("settings-root");
            if (settingsRoot != null && tabButtons.Count > 3)
            {
                _settingsTab = new SettingsTabComponent(Services.SaveService);
                _settingsTab.Initialize(settingsRoot);
                _tabSystem.AddTab("Settings", tabButtons[3], _settingsTab);
            }
        }

        #endregion

        #region Intro Animations

        /// <summary>
        /// Hide all animated elements before screen shows
        /// Called during OnInitialize to prevent flash of content
        /// </summary>
        private void HideAnimatedElements()
        {
            // Top bar elements
            HideElement(Container.Q<VisualElement>("profile-btn"));

            var tabView = Container.Q<TabView>("main-tabs");
            if (tabView != null)
            {
                var tabHeader = tabView.Q(className: "unity-tab-view__header");
                if (tabHeader != null)
                {
                    var tabButtons = tabHeader.Query<VisualElement>(className: "unity-tab-button").ToList();
                    foreach (var item in tabButtons)
                    {
                        HideElement(item);
                    }
                }
            }

            HideElement(Container.Q<VisualElement>("currency-pill-coins"));
            HideElement(Container.Q<VisualElement>("currency-pill-gems"));
            HideElement(Container.Q<VisualElement>("sys-icon-leaderboard"));
            HideElement(Container.Q<VisualElement>("sys-icon-friends"));
            HideElement(Container.Q<VisualElement>("sys-icon-settings"));

            // Lobby elements - will be hidden by LobbyTabComponent
        }

        private void HideElement(VisualElement element)
        {
            if (element == null) return;
            element.style.opacity = 0f;
            element.style.translate = new StyleTranslate(new Translate(0, -30, 0));
        }

        private void PlayIntroAnimations()
        {
            if (_animator == null) return;

            const float animDuration = 0.6f;
            const float staggerDelay = 0.1f;

            // === TOP BAR ANIMATIONS (Fade Down) ===

            // Profile button - delay 0
            var profileBtn = Container.Q<VisualElement>("profile-btn");
            AnimateFadeDown(profileBtn, 0f, animDuration);

            // Nav items - staggered delays (TabView buttons)
            var tabView = Container.Q<TabView>("main-tabs");
            if (tabView != null)
            {
                // The header contains the tab buttons
                var tabHeader = tabView.Q(className: "unity-tab-view__header");
                if (tabHeader != null)
                {
                    // Find all tab buttons in the header
                    var tabButtons = tabHeader.Query<VisualElement>(className: "unity-tab-button").ToList();
                    for (int i = 0; i < tabButtons.Count; i++)
                    {
                        AnimateFadeDown(tabButtons[i], staggerDelay * (i + 1), animDuration);
                    }
                }
            }

            // Currency pills - delays 0.3s, 0.4s
            var coinsPill = Container.Q<VisualElement>("currency-pill-coins");
            var gemsPill = Container.Q<VisualElement>("currency-pill-gems");
            AnimateFadeDown(coinsPill, 0.3f, animDuration);
            AnimateFadeDown(gemsPill, 0.4f, animDuration);

            // System icons - delays 0.5s, 0.6s, 0.7s
            var leaderboardBtn = Container.Q<VisualElement>("sys-icon-leaderboard");
            var friendsBtn = Container.Q<VisualElement>("sys-icon-friends");
            var settingsBtn = Container.Q<VisualElement>("sys-icon-settings");
            AnimateFadeDown(leaderboardBtn, 0.5f, animDuration);
            AnimateFadeDown(friendsBtn, 0.6f, animDuration);
            AnimateFadeDown(settingsBtn, 0.7f, animDuration);

            // === LOBBY PANEL ANIMATIONS ===
            // Trigger lobby tab animations (delay slightly to ensure lobby is ready)
            DOVirtual.DelayedCall(0.05f, () =>
            {
                _lobbyTab?.PlayIntroAnimations(_animator);
            });
        }

        private void AnimateFadeDown(VisualElement element, float delay, float duration)
        {
            if (element == null) return;

            // Set initial state (hidden, offset up)
            element.style.translate = new StyleTranslate(new Translate(0, -30, 0));

            // Animate after delay
            DOVirtual.DelayedCall(delay, () =>
            {
                // Fade in
                DOTween.To(() => 0f, x => element.style.opacity = x, 1f, duration)
                    .SetEase(Ease.OutCubic);

                // Slide down
                DOTween.To(() => -30f, y => element.style.translate = new StyleTranslate(new Translate(0, y, 0)), 0f, duration)
                    .SetEase(Ease.OutCubic);
            });
        }

        #endregion

        #region Procedural Graphics

        private void ApplyProceduralBackground()
        {
            // Background: Radial Gradient matching HTML reference
            // HTML: radial-gradient(circle at center, #1a1f2c 0%, #050505 100%)
            if (Container != null)
            {
                var bgTex = GenerateRadialGradient(Screen.width, Screen.height,
                    new Color32(26, 31, 44, 255), // #1a1f2c (dark blue/navy)
                    new Color32(5, 5, 5, 255));   // #050505 (near black)
                Container.style.backgroundImage = new StyleBackground(bgTex);
            }
        }

        private void ApplyTopBarVignette()
        {
            VisualElement topBar = GetElement<VisualElement>("top-bar");
            if (topBar != null)
            {
                // Create a vertical linear gradient from black (0.8 alpha) to transparent
                // Height is approx 120px (from USS)
                var vignetteTex = GenerateLinearGradient(Screen.width, 120,
                    new Color(0, 0, 0, 0.8f), // Top color
                    new Color(0, 0, 0, 0.0f)); // Bottom color

                topBar.style.backgroundImage = new StyleBackground(vignetteTex);
            }
        }

        private Texture2D GenerateRadialGradient(int width, int height, Color centerColor, Color edgeColor)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDist = Mathf.Sqrt(width * width + height * height) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / maxDist);
                    pixels[y * width + x] = Color.Lerp(centerColor, edgeColor, t);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private Texture2D GenerateLinearGradient(int width, int height, Color topColor, Color bottomColor)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                // y=0 is bottom in Unity Texture2D usually, but let's check.
                // Actually loops are y=0 to height.
                // We want topColor at top (y = height-1) and bottomColor at bottom (y = 0)
                float t = (float)y / (float)(height - 1);
                Color col = Color.Lerp(bottomColor, topColor, t);

                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
