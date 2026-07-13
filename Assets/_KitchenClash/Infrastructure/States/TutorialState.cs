using System;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Application.State;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.States
{
    /// <summary>
    /// Loads the dedicated Tutorial scene (a simplified kitchen map) and
    /// delegates step-by-step progression to ITutorialService.
    ///
    /// When ITutorialService fires OnCompleted (or the player skips):
    ///   → unloads Tutorial scene
    ///   → transitions to SessionLoadingState for a session refresh
    ///     (which will route to AccountUpgradeState on next pass)
    ///
    /// Tutorial UI overlays are shown via IUIService inside this state;
    /// the in-game HUD is suppressed until the tutorial match is over.
    /// </summary>
    public class TutorialState : BaseState
    {
        private const string LoadingScreenTypeName = "KitchenClash.Presentation.Screens.LoadingScreen, KitchenClash.Presentation";

        private readonly IUIService        _uiService;
        private readonly IEventBus         _eventBus;
        private readonly IGameStateManager _stateManager;
        private readonly ITutorialService  _tutorialService;
        private readonly IAppFlow          _appFlow;

        private Type _loadingScreenType;

        public TutorialState(
            IUIService        uiService,
            IEventBus         eventBus,
            IGameStateManager stateManager,
            ITutorialService  tutorialService,
            IAppFlow          appFlow = null)
        {
            _uiService        = uiService;
            _eventBus         = eventBus;
            _stateManager     = stateManager;
            _tutorialService  = tutorialService;
            _appFlow          = appFlow;
        }

        public override void Enter()
        {
            base.Enter();

            // Subscribe before starting so we never miss the completion event
            _tutorialService.OnCompleted += OnTutorialCompleted;

            _eventBus?.Publish(new MusicEvent(MusicTrack.Tutorial));

            EnterAsync().Forget();
        }

        public override void Exit()
        {
            _tutorialService.OnCompleted -= OnTutorialCompleted;
            base.Exit();
        }

        // ── Scene load ────────────────────────────────────────────────────

        private async UniTaskVoid EnterAsync()
        {
            try
            {
                // Show loading screen while the tutorial scene loads
                _loadingScreenType ??= Type.GetType(LoadingScreenTypeName);
                if (_loadingScreenType != null) _uiService.Show(_loadingScreenType);
                _eventBus?.Publish(new LoadingProgressEvent(0.1f, "Preparing tutorial kitchen..."));

                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Tutorial)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Tutorial).ToUniTask(
                        cancellationToken: StateCancellationToken);
                }
                if (!IsStateActive) return;

                _eventBus?.Publish(new LoadingProgressEvent(1.0f, "Ready!"));
                await UniTask.Delay(300, cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

                if (_loadingScreenType != null) _uiService.Hide(_loadingScreenType);

                // Start the tutorial step sequence
                _tutorialService.StartTutorial();

                LogMessage("Tutorial scene loaded — tutorial started");
            }
            catch (OperationCanceledException)
            {
                LogMessage("Tutorial scene load cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Failed to load tutorial scene: {ex.Message}");
                // Skip tutorial on hard failure — don't gate the whole game
                _tutorialService.SkipTutorial();
            }
        }

        // ── Completion ────────────────────────────────────────────────────

        private void OnTutorialCompleted()
        {
            if (!IsStateActive) return;

            LogMessage("Tutorial completed — going to SessionLoadingState for session refresh");

            // If entered as side phase, notify completion
            _appFlow?.CompleteSidePhase();

            // SessionLoadingState will see tutorial.IsComplete == true and
            // route to AccountUpgradeState (first time) or MainMenuState (never).
            _stateManager.ChangeState<SessionLoadingState>();
        }
    }
}
