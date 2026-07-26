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

        private IGameStateMachine _stateMachine;
        private IInputService _input;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            PlaycenterCompositionRoot.OnPlaycenterInitialized += OnPlaycenterReady;
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
            ServiceLocator.Register(new Net.MatchmakingController(lobbyService, ServiceLocator.Get<Playcenter.Net.INetService>()));

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

            var tutorialDone = ServiceLocator.Get<ISaveService>().Load("tutorial_completed", false);
            _stateMachine.ChangeState(tutorialDone ? (IGameState)new MainMenuState() : new TutorialState());
            ServiceLocator.Get<ILoggingService>().Log("[Game] Gameplay initialized");
        }

        private void Update()
        {
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
