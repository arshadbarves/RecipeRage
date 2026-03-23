using Core.Logging;
using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Spawning;
using Gameplay.UI;
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
    public class MatchRuntimeSceneBinder : MonoBehaviour
    {
        [Header("Scene Runtime References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private GamePhaseSync _gamePhaseSync;
        [SerializeField] private RoundTimer _roundTimer;
        [SerializeField] private NetworkScoreManager _networkScoreManager;
        [SerializeField] private MobileControlsManager _mobileControlsManager;
        [SerializeField] private SpawnManager _spawnManager;

        [Header("Discovery")]
        [SerializeField] private bool _resolveMissingReferencesOnSceneLoad = true;

        private IMatchRuntimeRegistry _runtimeRegistry;

        private void Awake()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
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

            _runtimeRegistry.RegisterSceneRuntime(
                _orderManager,
                _scoreManager,
                _gamePhaseSync,
                _roundTimer,
                _networkScoreManager,
                _mobileControlsManager,
                _spawnManager);
        }

        private void ResolveRuntimeRegistry()
        {
            if (_runtimeRegistry != null)
            {
                return;
            }

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope == null)
            {
                GameLogger.LogWarning("[MatchRuntimeSceneBinder] LifetimeScope not found. Match runtime will stay unbound.");
                return;
            }

            _runtimeRegistry = scope.Container.Resolve<IMatchRuntimeRegistry>();
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
            _roundTimer ??= Object.FindFirstObjectByType<RoundTimer>();
            _networkScoreManager ??= Object.FindFirstObjectByType<NetworkScoreManager>();
            _mobileControlsManager ??= Object.FindFirstObjectByType<MobileControlsManager>();
            _spawnManager ??= Object.FindFirstObjectByType<SpawnManager>();
        }
    }
}
