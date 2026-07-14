using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using Playcenter.GameFlow;
using UnityEngine.SceneManagement;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Tutorial side phase: load Tutorial scene, run ITutorialService, CompleteSidePhase on done.
    /// </summary>
    public sealed class TutorialPhase
    {
        private const string LoadingScreenTypeName =
            "KitchenClash.Presentation.Screens.LoadingScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IEventBus _eventBus;
        private readonly ITutorialService _tutorialService;
        private readonly IAppFlow _appFlow;

        private CancellationTokenSource _cts;
        private Type _loadingScreenType;
        private bool _active;

        public TutorialPhase(
            IUIService uiService,
            IEventBus eventBus,
            IAppFlow appFlow,
            ITutorialService tutorialService = null)
        {
            _uiService = uiService;
            _eventBus = eventBus;
            _tutorialService = tutorialService;
            _appFlow = appFlow;
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
        }

        private async UniTaskVoid EnterAsync(CancellationToken ct)
        {
            try
            {
                _loadingScreenType ??= Type.GetType(LoadingScreenTypeName);
                if (_loadingScreenType != null)
                {
                    _uiService?.Show(_loadingScreenType);
                }

                _eventBus?.Publish(new LoadingProgressEvent(0.1f, "Preparing tutorial kitchen..."));

                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Tutorial)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Tutorial).ToUniTask(cancellationToken: ct);
                }

                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                _eventBus?.Publish(new LoadingProgressEvent(1.0f, "Ready!"));
                await UniTask.Delay(300, cancellationToken: ct);
                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                if (_loadingScreenType != null)
                {
                    _uiService?.Hide(_loadingScreenType);
                }

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
