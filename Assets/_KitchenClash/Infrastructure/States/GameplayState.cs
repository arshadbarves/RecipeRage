using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;

namespace KitchenClash.Infrastructure.States
{
    public class GameplayState : BaseState
    {
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IGameModeService _gameModeService;

        private bool _sceneLoadComplete;
        private bool _startRoundRequested;
        private bool _gameStarted;

        public GameplayState(IUIService uiService, ISessionContext sessionContext, IEventBus eventBus, IGameModeService gameModeService)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _eventBus = eventBus;
            _gameModeService = gameModeService;
        }

        public override void Enter()
        {
            base.Enter();
            _sceneLoadComplete = false;
            _startRoundRequested = false;
            _gameStarted = false;
            _eventBus?.Publish(new MusicEvent(MusicTrack.Gameplay_Normal));
            InitializeGameplayAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();
            _gameModeService?.UnloadCurrentMapAsync().Forget();
        }

        private async UniTask InitializeGameplayAsync()
        {
            try
            {
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Game)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Game).ToUniTask();
                }
                if (!IsStateActive)
                {
                    return;
                }

                await UniTask.Yield(cancellationToken: StateCancellationToken);
                if (!IsStateActive)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_gameModeService?.SelectedGameMode?.MapSceneName))
                {
                    await _gameModeService.LoadMapAsync(_gameModeService.SelectedGameMode.MapSceneName);
                }
                if (!IsStateActive)
                {
                    return;
                }

                _sceneLoadComplete = true;

                // Start game only if RequestStartRound already called (queued start).
                if (_startRoundRequested && !_gameStarted)
                {
                    StartGameInternal();
                }
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[GameplayState] Initialization cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Called by MatchRuntimeFlowPort after countdown GO. Safe if Enter already started the game.
        /// </summary>
        public void RequestStartRound()
        {
            if (!IsStateActive)
            {
                return;
            }

            _startRoundRequested = true;

            // Start immediately if scene load is complete; otherwise queued until load finishes.
            if (_sceneLoadComplete && !_gameStarted)
            {
                StartGameInternal();
            }
        }

        private void StartGameInternal()
        {
            if (_gameStarted)
            {
                return; // Idempotent: only start once per Enter
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

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}
