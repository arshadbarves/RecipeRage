using System;
using System.Threading;
using Core.Networking.Interfaces;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Cysharp.Threading.Tasks;
using Gameplay.UI.Data;
using Core.Shared;
using KitchenClash.Presentation.Common;
using Core.Session;
using Gameplay.GameModes;
using UnityEngine;
using VContainer;

namespace KitchenClash.Application.ViewModels
{
    public class LobbyViewModel : BaseViewModel
    {
        private readonly ISessionContext _sessionContext;
        private readonly IGameStateManager _stateManager;
        private CancellationTokenSource _cts;

        private IGameModeService GameModeService => _sessionContext.GameModeService;

        public BindableProperty<bool> IsMatchmaking { get; } = new BindableProperty<bool>(false);
        public BindableProperty<int> PlayerCount { get; } = new BindableProperty<int>(1);

        public BindableProperty<string> MapName { get; } = new BindableProperty<string>("Loading...");
        public BindableProperty<string> MapSubtitle { get; } = new BindableProperty<string>("");
        public BindableProperty<string> RotationTimer { get; } = new BindableProperty<string>("");

        [Inject]
        public LobbyViewModel(ISessionContext sessionContext, IGameStateManager stateManager)
        {
            _sessionContext = sessionContext;
            _stateManager = stateManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            UpdateGameModeInfo();

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

            RotationTimer.Value = "";
        }

        public void Play()
        {
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
