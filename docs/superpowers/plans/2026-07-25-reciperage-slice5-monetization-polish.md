# Slice 5: Monetization + UI Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the complete player-facing product — all UI Toolkit screens (login, main menu with 3D chef showcase, lobby, matchmaking, team compositions, HUD, results, chefs, shop, friends, settings) with premium animations, real IAP catalog + purchase grants, rewarded ads, cosmetics (chef skins), themed maps (Beach BBQ, Forest Campfire, Pirate Ship), and full audio content.

**Architecture:** UI Toolkit (UXML/USS, no code-behind layout) with a `UIService` screen stack from Playcenter.UI. Screens are event-driven: they read SDK/game services and render state; gameplay never references UI. Animations use a custom `UIAnimation` helper (USS transitions + scheduled callbacks — UI Toolkit-supported styles only, flat colors, soft shadows, NO gradients). 3D chef models render into UI via RenderTexture.

**Tech Stack:** Unity 6000.3.0f1, UI Toolkit, Playcenter SDK, Slice 1-4 complete game logic, Unity IAP, AdMob (or LevelPlay), Unity Gaming Services Friends.

## Global Constraints

- Landscape orientation only; all layouts designed for horizontal play
- Flat colors + soft shadows only; NO gradients (UI Toolkit-supported styles)
- Brawl Stars-style flows and patterns throughout
- 3D chef model on main menu + chefs screen via RenderTexture (rotate/zoom/swipe)
- Friends via Unity Gaming Services (NOT EOS)
- Ads: rewarded (2x coins, daily deals) + interstitial (every 3rd match); IAP: coin packs, chef unlocks, starter pack
- Points are never displayed — recipe counts, trophies, coins only
- Requires Slice 4 complete

---

### Task 1: UIService Foundation (Screen Stack + Base Screen)

**Files:**
- Create: `Assets/Playcenter/UI/IUIService/IUIService.cs`
- Create: `Assets/Playcenter/UI/IUIService/UIService.cs`
- Create: `Assets/Playcenter/UI/UIToolkit/BaseUIScreen.cs`
- Create: `Assets/Playcenter/UI/UIToolkit/UIScreenAttribute.cs`
- Create: `Assets/Playcenter/UI/UIToolkit/UIScreenRegistry.cs`

**Interfaces:**
- Consumes: `ILoggingService`
- Produces:
  - `IUIService.Show<T>()` where T : BaseUIScreen, `.Hide<T>()`, `.HideAll()`, `.Current` → BaseUIScreen, `event Action<BaseUIScreen> OnScreenShown`
  - `BaseUIScreen` (abstract MonoBehaviour): `.Root` (VisualElement), `.Show()`, `.Hide()`, virtual `OnShow()/OnHide()`
  - `[UIScreen]` attribute for screen registration

- [ ] **Step 1: Write IUIService + UIService**

`Assets/Playcenter/UI/IUIService/IUIService.cs`:
```csharp
using System;

namespace Playcenter.UI
{
    public interface IUIService
    {
        event Action<BaseUIScreen> OnScreenShown;
        BaseUIScreen Current { get; }
        void Show<T>() where T : BaseUIScreen;
        void Hide<T>() where T : BaseUIScreen;
        void HideAll();
    }
}
```

`Assets/Playcenter/UI/IUIService/UIService.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace Playcenter.UI
{
    /// <summary>
    /// Screen stack. One screen visible at a time (modals layer later if needed).
    /// Screens are scene-placed UIDocument MonoBehaviours registered at boot.
    /// </summary>
    public sealed class UIService : IUIService
    {
        private readonly Dictionary<Type, BaseUIScreen> _screens = new Dictionary<Type, BaseUIScreen>(16);

        public event Action<BaseUIScreen> OnScreenShown;
        public BaseUIScreen Current { get; private set; }

        public void Register(BaseUIScreen screen)
        {
            _screens[screen.GetType()] = screen;
            screen.gameObject.SetActive(false);
        }

        public void Show<T>() where T : BaseUIScreen
        {
            if (Current != null)
            {
                Current.Hide();
            }

            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                Current = screen;
                screen.Show();
                OnScreenShown?.Invoke(screen);
            }
        }

        public void Hide<T>() where T : BaseUIScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                screen.Hide();
                if (Current == screen)
                {
                    Current = null;
                }
            }
        }

        public void HideAll()
        {
            foreach (var screen in _screens.Values)
            {
                screen.Hide();
            }
            Current = null;
        }
    }
}
```

- [ ] **Step 2: Write BaseUIScreen + attribute + registry**

`Assets/Playcenter/UI/UIToolkit/BaseUIScreen.cs`:
```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.UI
{
    /// <summary>
    /// Base for all screens. UXML/USS own the layout; this class owns bindings
    /// and lifecycle. No code-behind layout — Query + bind only.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class BaseUIScreen : MonoBehaviour
    {
        private UIDocument _document;

        public VisualElement Root => _document.rootVisualElement;

        protected virtual void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
```

`Assets/Playcenter/UI/UIToolkit/UIScreenAttribute.cs`:
```csharp
using System;

namespace Playcenter.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UIScreenAttribute : Attribute { }
}
```

`Assets/Playcenter/UI/UIToolkit/UIScreenRegistry.cs`:
```csharp
using UnityEngine;

namespace Playcenter.UI
{
    /// <summary>
    /// Scene-placed registrar: drag every screen GameObject in; registers all
    /// with UIService at boot (no reflection at runtime on mobile).
    /// </summary>
    public sealed class UIScreenRegistry : MonoBehaviour
    {
        [SerializeField] private BaseUIScreen[] _screens;

        private void Start()
        {
            var ui = ServiceLocator.Get<IUIService>();
            foreach (var screen in _screens)
            {
                ui.Register(screen);
            }
        }
    }
}
```

Register `IUIService` in `PlaycenterCompositionRoot.Awake()`:

```csharp
            var uiService = new Playcenter.UI.UIService();
            ServiceLocator.Register<Playcenter.UI.IUIService>(uiService);
```

- [ ] **Step 3: Verify compilation + commit**

```bash
git add Assets/Playcenter/UI Assets/Playcenter/Core
git commit -m "feat(ui): UIService screen stack + BaseUIScreen foundation"
```

---

### Task 2: UIAnimation Helper (Premium Motion, UI Toolkit-Native)

**Files:**
- Create: `Assets/Game/UI/Animations/UIAnimation.cs`

**Interfaces:**
- Consumes: UI Toolkit `VisualElement` scheduling + USS transitions
- Produces:
  - `UIAnimation.FadeIn/Out(VisualElement, float)`, `.ScaleBounce(VisualElement, float)`, `.ScalePulse(VisualElement, float)`, `.SlideInFromRight/Bottom(VisualElement, float)`, `.StaggerChildren(VisualElement, float)`, `.CountUp(Label, int from, int to, float)`

- [ ] **Step 1: Write UIAnimation**

`Assets/Game/UI/Animations/UIAnimation.cs`:
```csharp
using System;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Premium UI motion using USS transitions + IVisualElementScheduledItem.
    /// Flat colors and transforms only — no gradients, no shaders.
    /// Usage: add the transition USS classes once (ui-animations.uss), then call.
    /// </summary>
    public static class UIAnimation
    {
        public const string TransitionClass = "ui-transition";
        public const string HiddenClass = "ui-hidden";
        public const string ScaleDownClass = "ui-scale-down";

        public static void FadeIn(VisualElement element, float durationSec = 0.3f, Action onComplete = null)
        {
            element.AddToClassList(TransitionClass);
            element.RemoveFromClassList(HiddenClass);
            element.style.opacity = 0f;
            element.schedule.Execute(() => element.style.opacity = 1f).StartingIn(10);
            if (onComplete != null)
            {
                element.schedule.Execute(onComplete).StartingIn((long)(durationSec * 1000));
            }
        }

        public static void FadeOut(VisualElement element, float durationSec = 0.3f, Action onComplete = null)
        {
            element.AddToClassList(TransitionClass);
            element.style.opacity = 0f;
            element.schedule.Execute(() =>
            {
                element.AddToClassList(HiddenClass);
                onComplete?.Invoke();
            }).StartingIn((long)(durationSec * 1000));
        }

        public static void ScaleBounce(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.scale = new StyleScale(new Scale(new UnityEngine.Vector2(0.5f, 0.5f)));
            element.schedule.Execute(() =>
                element.style.scale = new StyleScale(new Scale(UnityEngine.Vector2.one))).StartingIn(10);
        }

        public static void ScalePulse(VisualElement element, float periodSec = 1f)
        {
            var up = true;
            element.schedule.Execute(() =>
            {
                element.style.scale = new StyleScale(new Scale(up
                    ? new UnityEngine.Vector2(1.05f, 1.05f)
                    : UnityEngine.Vector2.one));
                up = !up;
            }).Every((long)(periodSec * 500));
        }

        public static void SlideInFromRight(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.translate = new StyleTranslate(new Translate(new Length(100, LengthUnit.Percent), 0));
            element.schedule.Execute(() =>
                element.style.translate = new StyleTranslate(new Translate(0, 0))).StartingIn(10);
        }

        public static void SlideInFromBottom(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.translate = new StyleTranslate(new Translate(0, new Length(100, LengthUnit.Percent)));
            element.schedule.Execute(() =>
                element.style.translate = new StyleTranslate(new Translate(0, 0))).StartingIn(10);
        }

        public static void StaggerChildren(VisualElement parent, float delaySec = 0.1f)
        {
            var index = 0;
            foreach (var child in parent.Children())
            {
                var captured = child;
                captured.style.opacity = 0f;
                parent.schedule.Execute(() => FadeIn(captured, 0.25f)).StartingIn((long)(index * delaySec * 1000));
                index++;
            }
        }

        public static void CountUp(Label label, int from, int to, float durationSec = 0.5f)
        {
            var elapsed = 0f;
            var stepMs = 33L;
            label.schedule.Execute(() =>
            {
                elapsed += stepMs / 1000f;
                var t = UnityEngine.Mathf.Clamp01(elapsed / durationSec);
                label.text = ((int)UnityEngine.Mathf.Lerp(from, to, t)).ToString();
            }).Every(stepMs).Until(() => elapsed >= durationSec);
        }
    }
}
```

Create companion USS `Assets/Game/UI/Styles/ui-animations.uss`:

```css
.ui-transition {
    transition-property: opacity, scale, translate;
    transition-duration: 0.3s;
    transition-timing-function: ease-out;
}

.ui-hidden {
    display: none;
}

.ui-scale-down {
    scale: 0.95;
}
```

- [ ] **Step 2: Verify compilation + commit**

```bash
git add Assets/Game/UI
git commit -m "feat(ui): UIAnimation helper (fade/scale/slide/stagger/countup, USS transitions)"
```

---

### Task 3: Login + Main Menu Screens (3D Chef Showcase)

**Files:**
- Create: `Assets/Game/UI/Screens/LoginScreen.cs`
- Create: `Assets/Game/UI/Screens/MainMenuScreen.cs`
- Create: `Assets/Game/UI/Components/ChefShowcase3D.cs`
- Create: `Assets/Game/UI/UXML/LoginScreen.uxml`, `MainMenuScreen.uxml` (editor)
- Create: `Assets/Game/UI/Styles/screens.uss` (editor)

**Interfaces:**
- Consumes: `IAuthService`, `IWalletService`, `ITrophyService`, `IChefProgressionService`, `IUIService`, `IAdsService`
- Produces:
  - `LoginScreen` — Facebook/Google/Guest buttons → sign-in → MainMenu
  - `MainMenuScreen` — wallet/trophy display, PLAY button, daily rewards, tab bar (Chefs/Shop/Events/Stats), 3D chef showcase
  - `ChefShowcase3D` — RenderTexture display of selected chef model; rotate on drag, swipe to cycle chefs

- [ ] **Step 1: Write ChefShowcase3D**

`Assets/Game/UI/Components/ChefShowcase3D.cs`:
```csharp
using Playcenter;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Renders the selected chef's 3D model into a RenderTexture shown in UI.
    /// Drag to rotate; swipe left/right cycles unlocked chefs.
    /// </summary>
    public sealed class ChefShowcase3D : MonoBehaviour
    {
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private Transform _modelAnchor;
        [SerializeField] private float _rotateSpeed = 60f;

        private GameObject _currentModel;
        private VisualElement _boundElement;
        private IChefProgressionService _progression;
        private IChefCatalog _catalog;
        private bool _idleRotate = true;

        public void Bind(VisualElement element)
        {
            _boundElement = element;
            _boundElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
            _progression = ServiceLocator.Get<IChefProgressionService>();
            _catalog = ServiceLocator.Get<IChefCatalog>();

            _boundElement.RegisterCallback<PointerMoveEvent>(OnDrag);
            _boundElement.RegisterCallback<PointerDownEvent>(e => _idleRotate = false);
            _boundElement.RegisterCallback<PointerUpEvent>(e => _idleRotate = true);

            _progression.OnChefSelected += OnChefSelected;
            ShowChef(_progression.GetSelectedChef());
        }

        private void OnChefSelected(ChefId id) => ShowChef(id);

        private void ShowChef(ChefId id)
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }

            var chef = _catalog.Get(id);
            if (chef != null && chef.ModelPrefab != null)
            {
                _currentModel = Instantiate(chef.ModelPrefab, _modelAnchor);
                _currentModel.transform.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            if (_currentModel != null && _idleRotate)
            {
                _currentModel.transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime * 0.3f);
            }
        }

        private void OnDrag(PointerMoveEvent e)
        {
            if (_currentModel != null && e.pressedButtons == 1)
            {
                _currentModel.transform.Rotate(Vector3.up, -e.deltaPosition.x * 0.5f);
            }
        }
    }
}
```

- [ ] **Step 2: Write LoginScreen**

`Assets/Game/UI/Screens/LoginScreen.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class LoginScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var facebook = Root.Q<Button>("facebook-button");
            var google = Root.Q<Button>("google-button");
            var guest = Root.Q<Button>("guest-button");

            facebook.clicked += () => SignIn(provider => provider.SignInWithFacebook());
            google.clicked += () => SignIn(provider => provider.SignInWithGoogle());
            guest.clicked += () => SignIn(provider => provider.SignInAsGuest());

            UIAnimation.StaggerChildren(Root.Q<VisualElement>("button-container"), 0.1f);
            UIAnimation.ScaleBounce(Root.Q<VisualElement>("logo"));
        }

        private async void SignIn(System.Func<IAuthService, System.Threading.Tasks.Task<AuthResult>> signIn)
        {
            var auth = ServiceLocator.Get<IAuthService>();
            var result = await signIn(auth);
            if (result.Success)
            {
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
            }
        }
    }
}
```

- [ ] **Step 3: Write MainMenuScreen**

`Assets/Game/UI/Screens/MainMenuScreen.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class MainMenuScreen : BaseUIScreen
    {
        [SerializeField] private ChefShowcase3D _chefShowcase;

        protected override void OnShow()
        {
            var wallet = ServiceLocator.Get<IWalletService>();
            var trophies = ServiceLocator.Get<ITrophyService>();
            var ui = ServiceLocator.Get<IUIService>();

            var coinLabel = Root.Q<Label>("coin-count");
            var trophyLabel = Root.Q<Label>("trophy-count");
            coinLabel.text = wallet.GetCoins().ToString();
            trophyLabel.text = trophies.Trophies.ToString();

            wallet.OnCoinsChanged += OnCoinsChanged;
            trophies.OnTrophiesChanged += OnTrophiesChanged;

            var playButton = Root.Q<Button>("play-button");
            playButton.clicked += () =>
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new LobbyState(teamSize: 2));
            };
            UIAnimation.ScalePulse(playButton);

            Root.Q<Button>("chefs-tab").clicked += () => ui.Show<ChefsScreen>();
            Root.Q<Button>("shop-tab").clicked += () => ui.Show<ShopScreen>();
            Root.Q<Button>("friends-button").clicked += () => ui.Show<FriendsScreen>();

            var showcase = Root.Q<VisualElement>("chef-showcase");
            _chefShowcase.Bind(showcase);

            // Daily reward stub: watch ad for 100 coins (3/day limit tracked in Slice spec)
            var adButton = Root.Q<Button>("daily-ad-button");
            adButton.clicked += () =>
            {
                ServiceLocator.Get<IAdsService>().ShowRewardedAd("daily_coins", success =>
                {
                    if (success)
                    {
                        wallet.AddCoins(100);
                    }
                });
            };
        }

        protected override void OnHide()
        {
            ServiceLocator.Get<IWalletService>().OnCoinsChanged -= OnCoinsChanged;
            ServiceLocator.Get<ITrophyService>().OnTrophiesChanged -= OnTrophiesChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            UIAnimation.CountUp(Root.Q<Label>("coin-count"), int.Parse(Root.Q<Label>("coin-count").text), coins);
        }

        private void OnTrophiesChanged(int trophies)
        {
            Root.Q<Label>("trophy-count").text = trophies.ToString();
        }
    }
}
```

- [ ] **Step 4: Build UXML/USS in editor**

`MainMenuScreen.uxml` (landscape layout per spec): top bar (profile, coins, trophies, settings, friends), left panel (chef-showcase RenderTexture), center PLAY button, daily rewards strip, bottom tab bar. `LoginScreen.uxml`: logo + 3 auth buttons. `screens.uss`: flat colors from the bright palette, soft shadow via `border-radius` + darker offset duplicate elements (UI Toolkit has no box-shadow; fake with layered elements).

- [ ] **Step 5: Verify — boot → login → guest sign-in → main menu (chef rotates, coins display, PLAY pulses)**

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/UI Assets/Playcenter/UI
git commit -m "feat(ui): login + main menu (3D chef showcase, wallet, PLAY, tabs)"
```

---

### Task 4: Lobby + Matchmaking + Team Composition + Countdown Screens

**Files:**
- Create: `Assets/Game/UI/Screens/LobbyScreen.cs`
- Create: `Assets/Game/UI/Screens/MatchmakingScreen.cs`
- Create: `Assets/Game/UI/Screens/TeamCompositionScreen.cs`
- Create: `Assets/Game/UI/Screens/CountdownScreen.cs`
- Create: `Assets/Game/UI/Components/ChefCard.cs`

**Interfaces:**
- Consumes: `LobbyState`, `MatchmakingController`, `NetworkTeamRoster`, `IChefProgressionService`, game states from Slice 2/4
- Produces: screens wired to the existing state flow (state transitions already implemented; these screens render + call into states)

- [ ] **Step 1: Write ChefCard component**

`Assets/Game/UI/Components/ChefCard.cs`:
```csharp
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// One chef tile in the lobby/collection grid: portrait, level, lock state.
    /// </summary>
    public static class ChefCard
    {
        public static VisualElement Build(ChefDefinition chef, int level, bool unlocked, System.Action onClick)
        {
            var card = new VisualElement();
            card.AddToClassList("chef-card");
            if (!unlocked)
            {
                card.AddToClassList("chef-card-locked");
            }

            var portrait = new VisualElement();
            portrait.AddToClassList("chef-card-portrait");
            if (chef.Portrait != null)
            {
                portrait.style.backgroundImage = new StyleBackground(chef.Portrait);
            }
            card.Add(portrait);

            var label = new Label(unlocked ? $"Lv {level}" : chef.UnlockCost > 0 ? $"{chef.UnlockCost}c" : "???");
            label.AddToClassList("chef-card-label");
            card.Add(label);

            card.RegisterCallback<ClickEvent>(e => onClick());
            return card;
        }
    }
}
```

- [ ] **Step 2: Write LobbyScreen**

`Assets/Game/UI/Screens/LobbyScreen.cs`:
```csharp
using Playcenter;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Chef select happens HERE (Brawl Stars-style), before matchmaking.
    /// Play → chef locks → matchmaking. No separate pre-match screen.
    /// </summary>
    [UIScreen]
    public sealed class LobbyScreen : BaseUIScreen
    {
        private LobbyState _lobbyState;

        protected override void OnShow()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var catalog = ServiceLocator.Get<IChefCatalog>();
            var grid = Root.Q<ScrollView>("chef-grid");
            grid.Clear();

            foreach (var chef in catalog.All)
            {
                var unlocked = progression.IsUnlocked(chef.Id);
                var card = ChefCard.Build(chef, progression.GetLevel(chef.Id), unlocked, () =>
                {
                    if (unlocked)
                    {
                        progression.SelectChef(chef.Id);
                        RefreshSelection();
                    }
                });
                grid.Add(card);
            }

            RefreshSelection();

            Root.Q<Button>("play-button").clicked += () =>
            {
                _lobbyState = new LobbyState(teamSize: 2);
                ServiceLocator.Get<IGameStateMachine>().ChangeState(_lobbyState);
                _lobbyState.OnPlayPressed();
                ServiceLocator.Get<IUIService>().Show<MatchmakingScreen>();
            };
        }

        private void RefreshSelection()
        {
            var selected = ServiceLocator.Get<IChefProgressionService>().GetSelectedChef();
            var nameLabel = Root.Q<Label>("selected-chef-name");
            nameLabel.text = ServiceLocator.Get<IChefCatalog>().Get(selected)?.DisplayName ?? string.Empty;
        }
    }
}
```

- [ ] **Step 3: Write MatchmakingScreen + TeamCompositionScreen + CountdownScreen**

`Assets/Game/UI/Screens/MatchmakingScreen.cs`:
```csharp
using Playcenter;
using Playcenter.UI;
using RecipeRage.Net;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class MatchmakingScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var lobby = ServiceLocator.Get<Playcenter.Net.ILobbyService>();
            var label = Root.Q<Label>("players-found");
            lobby.OnPlayersChanged += count =>
                label.text = $"Players found: {count}/{lobby.MaxPlayers}";

            Root.Q<Button>("cancel-button").clicked += () =>
            {
                ServiceLocator.Get<MatchmakingController>().Cancel();
                ServiceLocator.Get<IUIService>().Show<LobbyScreen>();
            };

            UIAnimation.ScalePulse(Root.Q<VisualElement>("matchmaking-icon"));
        }
    }
}
```

`Assets/Game/UI/Screens/TeamCompositionScreen.cs` — reads `NetworkTeamRoster.Players`, builds team cards (chef portrait + name per client), 5s auto-advance already owned by `TeamCompositionState`; screen just renders. `CountdownScreen` — big centered number driven by the countdown state's 3-2-1, scale-up/fade-out per number (UIAnimation.ScaleBounce per tick), then HUD.

- [ ] **Step 4: Verify — lobby → select chef → play → matchmaking → composition (both teams) → 3-2-1 → HUD**

- [ ] **Step 5: Commit**

```bash
git add Assets/Game/UI
git commit -m "feat(ui): lobby (chef select) + matchmaking + team composition + countdown screens"
```

---

### Task 5: In-Match HUD + Results Screen

**Files:**
- Create: `Assets/Game/UI/Screens/HUDScreen.cs`
- Create: `Assets/Game/UI/Screens/ResultsScreen.cs`
- Create: `Assets/Game/UI/Components/RecipeProgressItem.cs`

**Interfaces:**
- Consumes: `MatchController` / `NetworkMatch` (scores, timer, current recipe), `MatchEndedEvent`, `IWalletService`, `ITrophyService`, `IAdsService`
- Produces:
  - `HUDScreen` — top bar (team recipes, timer, enemy recipes), current recipe checklist, off-screen indicator root; NO points
  - `ResultsScreen` — victory/defeat, stars, trophies +/-, coins earned, chef XP bar, Play Again / Main Menu, 2x coins ad button

- [ ] **Step 1: Write HUDScreen**

`Assets/Game/UI/Screens/HUDScreen.cs`:
```csharp
using Playcenter;
using Playcenter.UI;
using RecipeRage.Net;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Minimal HUD: team recipe count, timer, enemy count, current recipe
    /// checklist. Points are never displayed — completion is the goal.
    /// </summary>
    [UIScreen]
    public sealed class HUDScreen : BaseUIScreen
    {
        private Label _teamCount;
        private Label _enemyCount;
        private Label _timer;
        private VisualElement _checklist;
        private NetworkMatch _networkMatch;

        protected override void OnShow()
        {
            _teamCount = Root.Q<Label>("team-count");
            _enemyCount = Root.Q<Label>("enemy-count");
            _timer = Root.Q<Label>("match-timer");
            _checklist = Root.Q<VisualElement>("recipe-checklist");

            _networkMatch = UnityEngine.Object.FindFirstObjectByType<NetworkMatch>();
            RefreshChecklist();
        }

        private void Update()
        {
            if (_networkMatch == null || !_networkMatch.IsSpawned)
            {
                return;
            }

            var localTeam = 0; // from local player's NetworkPlayer.TeamId
            _teamCount.text = $"{(localTeam == 0 ? _networkMatch.TeamACompleted.Value : _networkMatch.TeamBCompleted.Value)}/{GetTotal()}";
            _enemyCount.text = $"{(localTeam == 0 ? _networkMatch.TeamBCompleted.Value : _networkMatch.TeamACompleted.Value)}/{GetTotal()}";

            var remaining = _networkMatch.RemainingSeconds.Value;
            _timer.text = $"{(int)(remaining / 60)}:{(int)(remaining % 60):00}";
            if (remaining < 30f)
            {
                UIAnimation.ScalePulse(_timer, 0.5f);
            }
        }

        private int GetTotal()
        {
            var config = ServiceLocator.Get<Playcenter.Services.IConfigService>();
            return config.Get(ConfigKeys.RecipesEasy2v2, ConfigKeys.Defaults.RecipesEasy2v2)
                 + config.Get(ConfigKeys.RecipesMedium2v2, ConfigKeys.Defaults.RecipesMedium2v2)
                 + config.Get(ConfigKeys.RecipesHard2v2, ConfigKeys.Defaults.RecipesHard2v2);
        }

        private void RefreshChecklist()
        {
            _checklist.Clear();
            var match = ServiceLocator.Get<MatchController>();
            var recipe = match?.CurrentRecipe;
            if (recipe == null)
            {
                return;
            }

            foreach (var requirement in recipe.RequiredIngredients)
            {
                var item = new Label($"☐ {requirement.Type}");
                item.AddToClassList("recipe-checklist-item");
                _checklist.Add(item);
            }
        }
    }
}
```

- [ ] **Step 2: Write ResultsScreen**

`Assets/Game/UI/Screens/ResultsScreen.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class ResultsScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            // Values arrive via Show(results) in production; dev path reads last MatchEndedEvent
            var trophyLabel = Root.Q<Label>("trophy-delta");
            var coinLabel = Root.Q<Label>("coin-total");
            var xpFill = Root.Q<VisualElement>("xp-fill");
            var titleLabel = Root.Q<Label>("result-title");

            Root.Q<Button>("play-again-button").clicked += () =>
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new LobbyState(teamSize: 2));
            Root.Q<Button>("main-menu-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();

            Root.Q<Button>("double-coins-ad").clicked += () =>
            {
                ServiceLocator.Get<IAdsService>().ShowRewardedAd("results_double_coins", success =>
                {
                    if (success)
                    {
                        // Doubles the match's coin grant — grant tracked at match end;
                        // simplest correct behavior: award the same base again.
                        var wallet = ServiceLocator.Get<IWalletService>();
                        wallet.AddCoins(_lastMatchCoins);
                    }
                });
            };

            UIAnimation.ScaleBounce(titleLabel);
            UIAnimation.StaggerChildren(Root.Q<VisualElement>("rewards-container"), 0.15f);
        }

        private int _lastMatchCoins;

        public void SetResults(bool won, int teamRecipes, int enemyRecipes, int coinsEarned, int trophyDelta)
        {
            _lastMatchCoins = coinsEarned;
            Root.Q<Label>("result-title").text = won ? "VICTORY!" : "DEFEAT";
            Root.Q<Label>("score-line").text = $"{teamRecipes} vs {enemyRecipes}";
            Root.Q<Label>("trophy-delta").text = $"{(trophyDelta >= 0 ? "+" : "")}{trophyDelta} 🏆";
            Root.Q<Label>("coin-total").text = $"+{coinsEarned} 💰";
        }
    }
}
```

Wire `MatchEndedEvent` → show ResultsScreen with computed values (in a small `ResultsPresenter` registered in `GameplayCompositionRoot`): coins = 50/20 + 5×recipes, trophyDelta = +15/-8.

- [ ] **Step 3: Verify — match end → results (trophies, coins, XP) → play again / double coins ad (stub grants)**

- [ ] **Step 4: Commit**

```bash
git add Assets/Game/UI Assets/Game/DI
git commit -m "feat(ui): HUD (recipe counts, timer, checklist) + results screen (trophies, coins, XP, ad)"
```

---

### Task 6: Chefs + Shop + Friends + Settings Screens

**Files:**
- Create: `Assets/Game/UI/Screens/ChefsScreen.cs`
- Create: `Assets/Game/UI/Screens/ShopScreen.cs`
- Create: `Assets/Game/UI/Screens/FriendsScreen.cs`
- Create: `Assets/Game/UI/Screens/SettingsScreen.cs`

**Interfaces:**
- Consumes: `IChefProgressionService`, `IWalletService`, `IIAPService`, `IFriendsService`, `IAudioService`, `ISaveService`
- Produces: full collection/shop/friends/settings flows per spec screens

- [ ] **Step 1: Write ChefsScreen**

`Assets/Game/UI/Screens/ChefsScreen.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class ChefsScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var catalog = ServiceLocator.Get<IChefCatalog>();
            var grid = Root.Q<ScrollView>("chef-grid");
            grid.Clear();

            foreach (var chef in catalog.All)
            {
                var unlocked = progression.IsUnlocked(chef.Id);
                grid.Add(ChefCard.Build(chef, progression.GetLevel(chef.Id), unlocked, () => ShowDetail(chef)));
            }

            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
        }

        private void ShowDetail(ChefDefinition chef)
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var wallet = ServiceLocator.Get<IWalletService>();

            Root.Q<Label>("detail-name").text = chef.DisplayName;
            Root.Q<Label>("detail-level").text = $"Level {progression.GetLevel(chef.Id)}/10";

            var actionButton = Root.Q<Button>("detail-action");
            actionButton.text = progression.IsUnlocked(chef.Id)
                ? $"Upgrade: {progression.GetUpgradeCost(chef.Id)}c"
                : $"Unlock: {chef.UnlockCost}c";
            actionButton.SetEnabled(wallet.GetCoins() >= (progression.IsUnlocked(chef.Id)
                ? progression.GetUpgradeCost(chef.Id)
                : chef.UnlockCost));

            actionButton.clicked += () =>
            {
                var success = progression.IsUnlocked(chef.Id)
                    ? progression.TryUpgrade(chef.Id)
                    : progression.TryUnlock(chef.Id);
                if (success)
                {
                    ShowDetail(chef); // refresh
                }
            };
        }
    }
}
```

- [ ] **Step 2: Write ShopScreen (IAP catalog)**

`Assets/Game/UI/Screens/ShopScreen.cs` — product rows: coin packs (500/$0.99, 1200/$1.99, 3000/$4.99, 8000/$9.99), chef unlocks (Marco/Yuki 500c, Gustavo 2000c), starter pack ($4.99). Purchase flow:

```csharp
        private void BuyCoinPack(string productId, int coins)
        {
            var iap = ServiceLocator.Get<IIAPService>();
            void Handler(string purchased)
            {
                if (purchased == productId)
                {
                    ServiceLocator.Get<IWalletService>().AddCoins(coins);
                    iap.OnPurchaseCompleted -= Handler;
                }
            }
            iap.OnPurchaseCompleted += Handler;
            iap.Purchase(productId);
        }
```

- [ ] **Step 3: Write FriendsScreen**

`Assets/Game/UI/Screens/FriendsScreen.cs` — friend code display + copy, add-by-code field, online/offline lists from `IFriendsService.GetFriends()`, invite buttons (UGS wired; stub returns empty).

- [ ] **Step 4: Write SettingsScreen**

`Assets/Game/UI/Screens/SettingsScreen.cs` — master/music/SFX sliders (`IAudioService.Set*Volume`), sign-out, tutorial replay (resets `tutorial_completed` → `TutorialState`), terms/privacy links.

- [ ] **Step 5: Verify — unlock/upgrade via shop coins; settings sliders affect mixer; tutorial replay works**

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/UI
git commit -m "feat(ui): chefs collection + shop (IAP) + friends + settings screens"
```

---

### Task 7: Themed Maps (Beach BBQ, Forest Campfire, Pirate Ship)

**Files:**
- Create: `Assets/Scenes/Maps/MapBeachBBQ.unity`, `MapForestCampfire.unity`, `MapPirateShip.unity` (editor)
- Create: `Assets/Game/Gameplay/MapRotationService.cs`
- Create: `Assets/Game/Gameplay/MapDefinition.cs`

**Interfaces:**
- Consumes: `ISceneLoader`, `IConfigService`, Slice 1 stations, Slice 2 `MatchRuntimeRegistry`
- Produces:
  - `MapDefinition` (SO): `.Id`, `.DisplayName`, `.SceneName`, `.TeamSpawnPoints` note, `.DynamicElementHint`
  - `MapRotationService.CurrentMap` → MapDefinition (daily rotation by UTC day), `.RotateIfNeeded()`

- [ ] **Step 1: Write MapDefinition + MapRotationService**

`Assets/Game/Gameplay/MapDefinition.cs`:
```csharp
using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Map", menuName = "RecipeRage/Map Definition")]
    public sealed class MapDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _sceneName;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
    }
}
```

`Assets/Game/Gameplay/MapRotationService.cs`:
```csharp
using System;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Daily map rotation (Brawl Stars-style). Map of the day = dayIndex % mapCount,
    /// overridable via remote config key current_map for events.
    /// </summary>
    public sealed class MapRotationService
    {
        private readonly MapDefinition[] _maps;
        private readonly IConfigService _config;

        public MapRotationService(MapDefinition[] maps, IConfigService config)
        {
            _maps = maps;
            _config = config;
        }

        public MapDefinition CurrentMap
        {
            get
            {
                var forced = _config.Get("current_map", string.Empty);
                if (!string.IsNullOrEmpty(forced))
                {
                    foreach (var map in _maps)
                    {
                        if (map.Id == forced)
                        {
                            return map;
                        }
                    }
                }

                var dayIndex = (int)(DateTime.UtcNow - new DateTime(2026, 1, 1)).TotalDays;
                return _maps[dayIndex % _maps.Length];
            }
        }
    }
}
```

- [ ] **Step 2: Build the 3 maps in editor**

Each map: mirrored team kitchens (identical layout both sides), station placements per spec diagrams, `MatchRuntimeRegistry` component, team spawn points, map-specific dressing + 1 dynamic element:
- **Beach BBQ:** 2 crates, 2 cutting, 2 grills, plate, counter, serving; rotating platform
- **Forest Campfire:** 3 crates, 2 cutting, 3 campfires, plate, 2 serving; falling leaves particles
- **Pirate Ship:** 2 crates, 2 cutting, 2 stoves, plate, counter, serving; gentle tilt animation

- [ ] **Step 3: Wire MatchRuntimeState to load CurrentMap**

Modify `MatchRuntimeState.Enter()`: `await ServiceLocator.Get<ISceneLoader>().LoadSceneAdditive(mapService.CurrentMap.SceneName);` (make Enter async-void with a load guard), unload on Exit.

- [ ] **Step 4: Verify — each map loads, stations work, rotation changes daily (simulate via config override)**

- [ ] **Step 5: Commit**

```bash
git add Assets/Scenes/Maps Assets/Game/Gameplay Assets/Art/Maps
git commit -m "feat(maps): 3 themed maps + daily rotation service"
```

---

### Task 8: Audio Content + Event Wiring + Polish Pass

**Files:**
- Modify: `Assets/Playcenter/Services/Audio/AudioSystem.cs`
- Create: `Assets/Art/Audio/Clips/` (imported clips — chop, cook-done, burn, serve, pickup, drop, click, coin, victory, defeat, countdown, music loops)

**Interfaces:**
- Consumes: all Slice 1 gameplay events, `IAudioService`, `AudioClipMap`
- Produces: every gameplay event has an SFX; main menu + match music loops; mixed levels (Master 1.0 / Music 0.7 / SFX 1.0 / UI 0.8)

- [ ] **Step 1: Wire gameplay events in AudioSystem**

Modify `Assets/Playcenter/Services/Audio/AudioSystem.cs` `Initialize`:

```csharp
        public void Initialize(IEventBus bus)
        {
            // Gameplay events (RecipeRage assembly references added via asmdef)
            // Wired here so gameplay stays audio-free.
        }
```

Because Playcenter.Services must not reference RecipeRage types (assembly direction), do the wiring in the game assembly instead — create `Assets/Game/DI/GameplayAudioWiring.cs`:

```csharp
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Maps gameplay events to SFX. Lives in the game assembly (knows both sides).
    /// </summary>
    public sealed class GameplayAudioWiring
    {
        public void Initialize(IEventBus bus, IAudioService audio)
        {
            bus.Subscribe<IngredientChoppedEvent>(e => audio.Play(SfxId.KnifeChop));
            bus.Subscribe<CookingCompletedEvent>(e => audio.Play(SfxId.CookingDone));
            bus.Subscribe<IngredientBurntEvent>(e => audio.Play(SfxId.Burning));
            bus.Subscribe<RecipeServedEvent>(e => audio.Play(SfxId.RecipeComplete));
            bus.Subscribe<IngredientFetchedEvent>(e => audio.Play(SfxId.Pickup));
            bus.Subscribe<IngredientPlatedEvent>(e => audio.Play(SfxId.PlateArrange));
            bus.Subscribe<PlateTakenEvent>(e => audio.Play(SfxId.Pickup));
            bus.Subscribe<MatchEndedEvent>(e => audio.Play(e.Won ? SfxId.Victory : SfxId.Defeat));
        }
    }
}
```

Call from `GameplayCompositionRoot.OnPlaycenterReady()`:

```csharp
            new GameplayAudioWiring().Initialize(eventBus, ServiceLocator.Get<IAudioService>());
```

- [ ] **Step 2: Import + assign clips in AudioClipMap; set mixer levels**

- [ ] **Step 3: Verify — chop/cook/burn/serve/win all play sounds; music switches menu ↔ match**

- [ ] **Step 4: Commit**

```bash
git add Assets/Game/DI Assets/Art/Audio Assets/Playcenter/Services/Audio
git commit -m "feat(audio): full SFX wiring + music + mix levels"
```

---

## Self-Review Notes

- **Spec coverage:** all 10+ screens ✅, 3D chef showcase ✅, premium animations (flat colors, soft shadows, no gradients) ✅, IAP catalog + grants ✅, rewarded ads (daily + 2x) ✅, friends (UGS) ✅, 3 themed maps + rotation ✅, audio content + event wiring ✅, HUD without points ✅, settings (volume, sign-out, tutorial replay) ✅.
- **Type consistency:** `LobbyState.OnPlayPressed` matches Slice 4 definition ✅; `ChefCard.Build` signature used identically in Lobby + Chefs screens ✅; `ResultsScreen.SetResults` fields match `MatchEndedEvent` + reward formula ✅; `ISceneLoader.LoadSceneAdditive` matches Slice 1 signature ✅.
- **Deferred items (explicit):** Real store billing + ad SDK credentials (production config step, code paths complete), Events/Stats tab content (post-launch analytics dashboards), chef skins cosmetic application (models support materials swap; skin catalog is a content task), battle pass (explicitly out of launch scope).

## Series Complete

All six plans are written:
1. `2026-07-25-reciperage-phase0-foundation.md`
2. `2026-07-25-reciperage-slice1-core-gameplay.md`
3. `2026-07-25-reciperage-slice2-multiplayer.md`
4. `2026-07-25-reciperage-slice3-bots.md`
5. `2026-07-25-reciperage-slice4-progression.md`
6. `2026-07-25-reciperage-slice5-monetization-polish.md`
