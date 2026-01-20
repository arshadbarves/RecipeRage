using System;
using System.Threading;
using Core.Networking.Interfaces;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Cysharp.Threading.Tasks;
using Gameplay.UI.Data;
using Core.Shared;
using Core.UI.Core;
using Core.Session;
using Gameplay.GameModes;
using UnityEngine;
using VContainer;

namespace Gameplay.UI.Features.Lobby
{
    public class LobbyViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly IGameStateManager _stateManager;
        private CancellationTokenSource _cts;

        private IGameModeService GameModeService => _sessionManager.SessionContainer?.Resolve<IGameModeService>();

        public BindableProperty<bool> IsMatchmaking { get; } = new BindableProperty<bool>(false);
        public BindableProperty<int> PlayerCount { get; } = new BindableProperty<int>(1);

        // Map Data (Now Game Mode Data)
        public BindableProperty<string> MapName { get; } = new BindableProperty<string>("Loading...");
        public BindableProperty<string> MapSubtitle { get; } = new BindableProperty<string>("");
        public BindableProperty<string> RotationTimer { get; } = new BindableProperty<string>("");

        [Inject]
        public LobbyViewModel(SessionManager sessionManager, IGameStateManager stateManager)
        {
            _sessionManager = sessionManager;
            _stateManager = stateManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            UpdateGameModeInfo();

            // Subscribe to game mode changes if service is available
            var service = GameModeService;
            if (service != null)
            {
                service.OnGameModeChanged += OnGameModeChanged;
            }
        }

        private void OnGameModeChanged(GameMode mode)
        {
            UpdateGameModeInfo();
        }

        private void UpdateGameModeInfo()
        {
            var service = GameModeService;
            if (service?.SelectedGameMode != null)
            {
                MapName.Value = service.SelectedGameMode.DisplayName.ToUpper();
                MapSubtitle.Value = service.SelectedGameMode.Subtitle;
            }

            // Hide rotation timer for now as GameMode doesn't support it yet
            RotationTimer.Value = "";
        }

        // Timer removed for now
        private void StartTimerLoop() { }
        private async UniTaskVoid UpdateTimerAsync(CancellationToken token) { await UniTask.Yield(); }



        public void Play()
        {
            // Simple pass-through for now
            _stateManager.ChangeState<MatchmakingState>();
        }

        public void StartMatchmaking()
        {
            IsMatchmaking.Value = true;
        }

        public override void Dispose()
        {
            var service = GameModeService;
            if (service != null)
            {
                service.OnGameModeChanged -= OnGameModeChanged;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}