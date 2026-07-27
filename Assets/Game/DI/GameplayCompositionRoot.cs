using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Game-side composition root. Waits for the Playcenter SDK, then constructs
    /// game services (gameplay logic ONLY — core logic lives in the SDK).
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class GameplayCompositionRoot : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private RecipeDefinition[] _allRecipes;
        [SerializeField] private ChefDefinition[] _allChefs;
        [SerializeField] private MapDefinition[] _allMaps;

        private IGameStateMachine _stateMachine;
        private IInputService _input;
        private bool _splashShown;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            PlaycenterCompositionRoot.OnPlaycenterInitialized += OnPlaycenterReady;
        }

        private void ShowSplashOnce()
        {
            if (_splashShown)
            {
                return;
            }
            if (ServiceLocator.TryGet<Playcenter.UI.IUIService>(out var ui))
            {
                ui.Show<UI.SplashScreen>();
                _splashShown = true;
            }
        }

        private void OnPlaycenterReady()
        {
            _input = new DualStickInputService();
            var sceneLoader = new AddressablesSceneLoader();
            _stateMachine = new GameStateMachine();

            var recipeCatalog = new RecipeCatalog(_allRecipes);
            var matchController = new MatchController(
                recipeCatalog,
                ServiceLocator.Get<IConfigService>(),
                ServiceLocator.Get<IEventBus>(),
                ServiceLocator.Get<ITimeService>());

            ServiceLocator.Register(_input);
            ServiceLocator.Register<ISceneLoader>(sceneLoader);
            ServiceLocator.Register(_stateMachine);
            ServiceLocator.Register<IRecipeCatalog>(recipeCatalog);
            ServiceLocator.Register(matchController);

            var lobbyService = new Playcenter.Net.EOSLobbyService(
                ServiceLocator.Get<IAuthService>(), ServiceLocator.Get<ILoggingService>());
            ServiceLocator.Register<Playcenter.Net.ILobbyService>(lobbyService);

            // INetService wraps the scene's NetworkManager (create one if absent).
            // Registered before MatchmakingController, which depends on it.
            var networkManager = UnityEngine.Object.FindFirstObjectByType<Unity.Netcode.NetworkManager>();
            if (networkManager == null)
            {
                var nmGo = new GameObject("NetworkManager");
                nmGo.AddComponent<Unity.Netcode.NetworkManager>();
                networkManager = nmGo.GetComponent<Unity.Netcode.NetworkManager>();
                UnityEngine.Object.DontDestroyOnLoad(nmGo);
            }
            var netService = new Playcenter.Net.NetService(networkManager);
            ServiceLocator.Register<Playcenter.Net.INetService>(netService);

            ServiceLocator.Register(new Net.MatchmakingController(lobbyService, netService));

            var planner = new Bots.TaskPlanner();
            planner.Register(new Bots.ClearBurntEvaluator());
            planner.Register(new Bots.ServeEvaluator());
            planner.Register(new Bots.CollectCookEvaluator());
            planner.Register(new Bots.StartCookEvaluator());
            planner.Register(new Bots.ChopEvaluator());
            planner.Register(new Bots.TakePlateEvaluator());
            planner.Register(new Bots.ArrangePlateEvaluator());
            planner.Register(new Bots.FetchEvaluator());
            planner.Register(new Bots.WanderEvaluator());
            ServiceLocator.Register(planner);
            ServiceLocator.Register(new Bots.BotClaimRegistry());

            // Adaptive difficulty: track human recipe pace, scale bot dwell to match
            var skillTracker = new Bots.SkillTracker();
            var adaptiveDifficulty = new Bots.AdaptiveDifficulty(ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register(skillTracker);
            ServiceLocator.Register(adaptiveDifficulty);
            var eventBusRef = ServiceLocator.Get<IEventBus>();
            eventBusRef.Subscribe<RecipeServedEvent>(e =>
            {
                if (ServiceLocator.TryGet<MatchController>(out var m))
                {
                    skillTracker.TrackRecipeCompleted(300f - m.RemainingSeconds);
                }
            });

            var chefCatalog = new ChefCatalog(_allChefs);
            var chefProgression = new ChefProgressionService(
                chefCatalog,
                ServiceLocator.Get<IWalletService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAnalyticsService>());
            ServiceLocator.Register<IChefCatalog>(chefCatalog);
            ServiceLocator.Register<IChefProgressionService>(chefProgression);

            var mapRotation = new MapRotationService(_allMaps, ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register(mapRotation);

            var dailyRewards = new DailyRewardsService(
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IWalletService>(),
                ServiceLocator.Get<IAnalyticsService>());
            ServiceLocator.Register(dailyRewards);

            new GameplayAudioWiring().Initialize(eventBusRef, ServiceLocator.Get<IAudioService>());

            // Apply Clay Kitchen fonts (Fredoka headings, Nunito body) to every screen on Show
            Playcenter.UI.BaseUIScreen.FontThemeHook = root => UI.UIFontTheme.Apply(root);

            // UI presenters (RecipeRage.UI knows both gameplay events and screens)
            var uiService = ServiceLocator.Get<Playcenter.UI.IUIService>();
            new UI.MainMenuPresenter().Initialize(eventBusRef, uiService, ServiceLocator.Get<IAuthService>());
            new UI.ResultsPresenter().Initialize(eventBusRef, uiService);

            var trophyService = new TrophyService(
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAnalyticsService>());
            ServiceLocator.Register<ITrophyService>(trophyService);

            eventBusRef.Subscribe<MatchEndedEvent>(e =>
            {
                trophyService.ApplyMatchResult(e.Won);

                // Match rewards: coins + chef XP (spec: 50 win / 20 loss + 5 per recipe)
                var wallet = ServiceLocator.Get<IWalletService>();
                var coins = e.Won ? 50 : 20;
                coins += e.TeamRecipes * 5;
                wallet.AddCoins(coins);

                chefProgression.AddXp(chefProgression.GetSelectedChef(), 25);
            });

            // Hold the splash for a minimum display time, then go to MainMenu.
            // (Login shows if signed out — see MainMenuPresenter.)
            StartCoroutine(SplashThenMainMenu());
            ServiceLocator.Get<ILoggingService>().Log("[Game] Gameplay initialized");
        }

        private System.Collections.IEnumerator SplashThenMainMenu()
        {
            // Ensure splash is visible before holding (screens register this frame).
            ShowSplashOnce();
            // Hold long enough for the premium reveal (black pause + title + subtitle).
            yield return new WaitForSeconds(3f);
            _stateMachine.ChangeState(new MainMenuState());
        }

        private void Update()
        {
            // Splash shows once screens are registered (before state machine runs).
            ShowSplashOnce();

            if (_stateMachine == null)
            {
                return;
            }

            _input.Tick();
            _stateMachine.Update(ServiceLocator.Get<ITimeService>().DeltaTime);

            if (ServiceLocator.TryGet<MatchController>(out var match))
            {
                match.Tick();
            }
        }

        private void OnDestroy()
        {
            PlaycenterCompositionRoot.OnPlaycenterInitialized -= OnPlaycenterReady;
        }
    }
}
