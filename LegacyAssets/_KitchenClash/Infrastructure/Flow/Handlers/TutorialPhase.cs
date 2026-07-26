using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using Playcenter.GameFlow;
using Playcenter.SDK;
using Playcenter.Shell;
using UnityEngine.SceneManagement;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Tutorial side phase: load Tutorial scene, run ITutorialService, CompleteSidePhase on done.
    /// Uses SDK shell Loading during scene transition (game LoadingScreen removed in Task 7).
    /// </summary>
    public sealed class TutorialPhase
    {
        private readonly IEventBus _eventBus;
        private readonly ITutorialService _tutorialService;
        private readonly IAppFlow _appFlow;
        private readonly IShellUi _shellUi;

        private CancellationTokenSource _cts;
        private bool _active;

        public TutorialPhase(
            IEventBus eventBus,
            IAppFlow appFlow,
            IShellUi shellUi,
            ITutorialService tutorialService = null)
        {
            _eventBus = eventBus;
            _tutorialService = tutorialService;
            _appFlow = appFlow;
            _shellUi = shellUi;
        }

        public void Enter()
        {
            Exit();
            _active = true;
            _cts = new CancellationTokenSource();

            if (_tutorialService != null)
            {
                _tutorialService.OnCompleted += OnTutorialCompleted;
            }

            _eventBus?.Publish(new MusicEvent(MusicTrack.Tutorial));
            EnterAsync(_cts.Token).Forget();
        }

        public void Exit()
        {
            if (_tutorialService != null)
            {
                _tutorialService.OnCompleted -= OnTutorialCompleted;
            }

            _active = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _shellUi?.Hide(ShellScreenId.Loading);
        }

        private async UniTaskVoid EnterAsync(CancellationToken ct)
        {
            try
            {
                _shellUi?.Show(ShellScreenId.Loading);
                _shellUi?.SetProgress(0.1f, "Preparing tutorial kitchen...");
                _eventBus?.Publish(new LoadingProgressEvent(0.1f, "Preparing tutorial kitchen..."));

                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Tutorial)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Tutorial).ToUniTask(cancellationToken: ct);
                }

                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                _shellUi?.SetProgress(1.0f, "Ready!");
                _eventBus?.Publish(new LoadingProgressEvent(1.0f, "Ready!"));
                await UniTask.Delay(300, cancellationToken: ct);
                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                _shellUi?.Hide(ShellScreenId.Loading);

                _tutorialService?.StartTutorial();
                GameLogger.Log("[TutorialPhase] Tutorial scene loaded — tutorial started");
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[TutorialPhase] Tutorial scene load cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[TutorialPhase] Failed to load tutorial scene: {ex.Message}");
                _shellUi?.Hide(ShellScreenId.Loading);
                _tutorialService?.SkipTutorial();
            }
        }

        private void OnTutorialCompleted()
        {
            if (!_active)
            {
                return;
            }

            GameLogger.Log("[TutorialPhase] Tutorial completed → CompleteSidePhase");
            _appFlow?.CompleteSidePhase();
        }
    }
}
