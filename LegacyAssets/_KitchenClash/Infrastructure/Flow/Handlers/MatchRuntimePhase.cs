using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using UnityEngine.SceneManagement;
using KitchenClash.Application;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Match runtime: load Game + map scenes; StartRound gated until load complete.
    /// </summary>
    public sealed class MatchRuntimePhase
    {
        private readonly IEventBus _eventBus;
        private readonly ISessionContext _sessionContext;
        private readonly IGameModeService _gameModeService;

        private CancellationTokenSource _cts;
        private bool _active;
        private bool _sceneLoadComplete;
        private bool _startRoundRequested;
        private bool _gameStarted;

        public bool IsEntered => _active;

        private IGameModeService GameModes =>
            _gameModeService ?? _sessionContext?.GameModeService;

        public MatchRuntimePhase(
            IEventBus eventBus,
            ISessionContext sessionContext,
            IGameModeService gameModeService = null)
        {
            _eventBus = eventBus;
            _sessionContext = sessionContext;
            _gameModeService = gameModeService;
        }

        public void Enter()
        {
            if (_active)
            {
                // Idempotent re-enter (intro preload → match): only honor pending start.
                if (_startRoundRequested && _sceneLoadComplete && !_gameStarted)
                {
                    StartGameInternal();
                }

                return;
            }

            _active = true;
            _sceneLoadComplete = false;
            _startRoundRequested = false;
            _gameStarted = false;
            _cts = new CancellationTokenSource();
            _eventBus?.Publish(new MusicEvent(MusicTrack.Gameplay_Normal));
            InitializeGameplayAsync(_cts.Token).Forget();
        }

        public void Exit()
        {
            _active = false;
            _sceneLoadComplete = false;
            _startRoundRequested = false;
            _gameStarted = false;

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            GameModes?.UnloadCurrentMapAsync().Forget();
        }

        public void RequestStartRound()
        {
            if (!_active)
            {
                return;
            }

            _startRoundRequested = true;

            if (_sceneLoadComplete && !_gameStarted)
            {
                StartGameInternal();
            }
        }

        private async UniTask InitializeGameplayAsync(CancellationToken ct)
        {
            try
            {
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Game)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Game).ToUniTask(cancellationToken: ct);
                }

                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.Yield(cancellationToken: ct);
                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                IGameModeService gameModes = GameModes;
                if (!string.IsNullOrEmpty(gameModes?.SelectedGameMode?.MapSceneName))
                {
                    await gameModes.LoadMapAsync(gameModes.SelectedGameMode.MapSceneName);
                }

                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                _sceneLoadComplete = true;

                if (_startRoundRequested && !_gameStarted)
                {
                    StartGameInternal();
                }
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[MatchRuntimePhase] Initialization cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private void StartGameInternal()
        {
            if (_gameStarted)
            {
                return;
            }

            try
            {
                _sessionContext.GameStarter?.StartGame();
                _gameStarted = true;
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }
    }
}
