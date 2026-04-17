using System.Collections.Generic;
using Core.Logging;
using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Spawning;
using Gameplay.Stations;
using Gameplay.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Gameplay.Shared
{
    /// <summary>
    /// Scene bridge that publishes gameplay-scene runtime objects into the app-scoped match context.
    /// This keeps the rest of the app off ad-hoc scene queries.
    /// </summary>
    public class MatchRuntimeSceneBinder : MonoBehaviour, IKitchenSupportRuntime, IBotKitchenRuntime
    {
        [Header("Scene Runtime References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private GamePhaseSync _gamePhaseSync;
        [SerializeField] private MatchResultSync _matchResultSync;
        [SerializeField] private RoundTimer _roundTimer;
        [SerializeField] private NetworkScoreManager _networkScoreManager;
        [SerializeField] private MobileControlsManager _mobileControlsManager;
        [SerializeField] private SpawnManager _spawnManager;
        [SerializeField] private IngredientNetworkSpawner _ingredientNetworkSpawner;

        [Header("Kitchen Support Runtime")]
        [SerializeField] private Transform _stationsParent;
        [SerializeField] private ServingStation _servingStationAnchor;
        [SerializeField] private CounterStation _counterStationAnchor;
        [SerializeField] private GameObject _ingredientCratePrefab;
        [SerializeField] private GameObject _plateDispenserPrefab;
        [SerializeField] private Ingredient _tomatoIngredient;
        [SerializeField] private Ingredient _steakIngredient;

        [Header("Discovery")]
        [SerializeField] private bool _resolveMissingReferencesOnSceneLoad = true;

        private IMatchRuntimeRegistry _runtimeRegistry;
        private IMatchContext _matchContext;
        private readonly List<Component> _botStations = new List<Component>();

        public IReadOnlyList<Component> Stations => _botStations;

        private void Awake()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            BindRuntime();
        }

        private void Start()
        {
            BindRuntime();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _runtimeRegistry?.ClearSceneRuntime();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindRuntime();
        }

        private void BindRuntime()
        {
            ResolveRuntimeRegistry();
            if (_runtimeRegistry == null)
            {
                return;
            }

            ResolveMissingReferences();
            RefreshBotStationCache();

            _runtimeRegistry.RegisterSceneRuntime(
                _orderManager,
                _scoreManager,
                _gamePhaseSync,
                _matchResultSync,
                _roundTimer,
                _networkScoreManager,
                _mobileControlsManager,
                _spawnManager,
                _ingredientNetworkSpawner,
                this,
                this);
        }

        private void ResolveRuntimeRegistry()
        {
            if (_runtimeRegistry != null && _matchContext != null)
            {
                return;
            }

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope == null)
            {
                GameLogger.LogWarning("[MatchRuntimeSceneBinder] LifetimeScope not found. Match runtime will stay unbound.");
                return;
            }

            _runtimeRegistry ??= scope.Container.Resolve<IMatchRuntimeRegistry>();
            _matchContext ??= scope.Container.Resolve<IMatchContext>();
        }

        private void ResolveMissingReferences()
        {
            if (!_resolveMissingReferencesOnSceneLoad)
            {
                return;
            }

            _orderManager ??= Object.FindFirstObjectByType<OrderManager>();
            _scoreManager ??= Object.FindFirstObjectByType<ScoreManager>();
            _gamePhaseSync ??= Object.FindFirstObjectByType<GamePhaseSync>();
            _matchResultSync ??= Object.FindFirstObjectByType<MatchResultSync>();
            _roundTimer ??= Object.FindFirstObjectByType<RoundTimer>();
            _networkScoreManager ??= Object.FindFirstObjectByType<NetworkScoreManager>();
            _mobileControlsManager ??= Object.FindFirstObjectByType<MobileControlsManager>();
            _spawnManager ??= Object.FindFirstObjectByType<SpawnManager>();
            _ingredientNetworkSpawner ??= Object.FindFirstObjectByType<IngredientNetworkSpawner>();
            _stationsParent ??= GameObject.Find("Stations")?.transform;
            _servingStationAnchor ??= Object.FindFirstObjectByType<ServingStation>();
            _counterStationAnchor ??= Object.FindFirstObjectByType<CounterStation>();
            _ingredientCratePrefab ??= Resources.Load<GameObject>("Prefabs/Gameplay/Stations/IngredientCrate");
            _plateDispenserPrefab ??= Resources.Load<GameObject>("Prefabs/Gameplay/Stations/PlateDispenser");
            _tomatoIngredient ??= Resources.Load<Ingredient>("ScriptableObjects/Cooking/Ingredients/Tomato");
            _steakIngredient ??= Resources.Load<Ingredient>("ScriptableObjects/Cooking/Ingredients/Steak");
        }

        public void EnsureKitchenSupportStations()
        {
            ResolveMissingReferences();

            if (_matchContext?.IsServer != true)
            {
                return;
            }

            EnsureIngredientCrates();
            EnsurePlateDispenser();
            RefreshBotStationCache();
        }

        private void EnsureIngredientCrates()
        {
            if (Object.FindObjectsByType<IngredientCrate>(FindObjectsSortMode.None).Length > 0)
            {
                return;
            }

            if (_ingredientCratePrefab == null || _tomatoIngredient == null || _steakIngredient == null)
            {
                GameLogger.LogWarning("[MatchRuntimeSceneBinder] Could not create runtime ingredient crates because required references are missing.");
                return;
            }

            Vector3 anchor = GetKitchenAnchor();
            SpawnIngredientCrate(_tomatoIngredient, anchor + new Vector3(0f, 0f, -2f));
            SpawnIngredientCrate(_steakIngredient, anchor + new Vector3(0f, 0f, -4f));
        }

        private void EnsurePlateDispenser()
        {
            if (Object.FindObjectsByType<PlateDispenser>(FindObjectsSortMode.None).Length > 0)
            {
                return;
            }

            if (_plateDispenserPrefab == null)
            {
                GameLogger.LogWarning("[MatchRuntimeSceneBinder] Could not create runtime plate dispenser because the prefab reference is missing.");
                return;
            }

            Vector3 anchor = GetKitchenAnchor();
            GameObject dispenserObject = Object.Instantiate(_plateDispenserPrefab, anchor + new Vector3(0f, 0f, 2f), Quaternion.identity);
            if (_stationsParent != null)
            {
                dispenserObject.transform.SetParent(_stationsParent);
            }

            NetworkObject networkObject = dispenserObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
            }
        }

        private void SpawnIngredientCrate(Ingredient ingredient, Vector3 position)
        {
            GameObject crateObject = Object.Instantiate(_ingredientCratePrefab, position, Quaternion.identity);
            if (_stationsParent != null)
            {
                crateObject.transform.SetParent(_stationsParent);
            }

            IngredientCrate crate = crateObject.GetComponent<IngredientCrate>();
            crate?.ConfigureIngredient(ingredient);

            NetworkObject networkObject = crateObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
            }
        }

        private Vector3 GetKitchenAnchor()
        {
            if (_servingStationAnchor != null)
            {
                return _servingStationAnchor.transform.position;
            }

            if (_counterStationAnchor != null)
            {
                return _counterStationAnchor.transform.position;
            }

            return Vector3.zero;
        }

        private void RefreshBotStationCache()
        {
            _botStations.Clear();
            RegisterBotStations(Object.FindObjectsByType<IngredientCrate>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<CuttingStation>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<CookingStation>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<CounterStation>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<ServingStation>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<SinkStation>(FindObjectsSortMode.None));
            RegisterBotStations(Object.FindObjectsByType<PlateDispenser>(FindObjectsSortMode.None));
        }

        private void RegisterBotStations<T>(IEnumerable<T> stations) where T : Component
        {
            foreach (T station in stations)
            {
                if (station != null)
                {
                    _botStations.Add(station);
                }
            }
        }
    }
}
