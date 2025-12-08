using Core.Logging;
using Core.State.States;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Single entry point for the entire game - bootstraps all services
    /// Flow: Bootstrap ?? ServiceContainer ?? Services ?? BootstrapState
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private UIDocumentProvider _uiDocumentProvider;

        public static GameBootstrap Instance { get; private set; }
        public static ServiceContainer Services { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("Starting game initialization...");

            Services = new ServiceContainer(_uiDocumentProvider);

            // Subscribe to logout handler for full reboot
            Services.EventBus.Subscribe<Core.Events.LogoutEvent>(HandleLogoutAsync);

            GameLogger.Log("Starting State Machine...");

            var bootstrapState = new BootstrapState(
                Services.UIService,
                Services.NTPTimeService,
                Services.RemoteConfigService,
                Services.AuthenticationService,
                Services.MaintenanceService,
                Services.StateManager,
                Services.EventBus,
                Services
            );

            Services.StateManager.Initialize(bootstrapState);
        }

        private async void HandleLogoutAsync(Core.Events.LogoutEvent evt)
        {
            GameLogger.Log($"User {evt.UserId} logged out - performing full reboot");

            Services?.Dispose();

            await UniTask.Yield();

            // Re-initialize container
            Services = new ServiceContainer(_uiDocumentProvider);
            Services.EventBus.Subscribe<Core.Events.LogoutEvent>(HandleLogoutAsync);

            Services.StateManager.Initialize(new LoginState(
                Services.UIService,
                Services.EventBus,
                Services.StateManager,
                Services
            ));

            GameLogger.Log("Reboot complete - ready for new login");
        }

        private void Update()
        {
            Services?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Services?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            Services?.Dispose();
        }
    }
}