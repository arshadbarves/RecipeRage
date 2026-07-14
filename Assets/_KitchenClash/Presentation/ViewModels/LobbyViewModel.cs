using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using System.Threading;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Application.Services;
using UnityEngine;
using VContainer;
using Playcenter.GameFlow;

namespace KitchenClash.Presentation.ViewModels
{
    public class LobbyViewModel : BaseViewModel
    {
        private readonly ISessionContext _sessionContext;
        private readonly IAppFlow _appFlow;
        private CancellationTokenSource _cts;

        private IGameModeService GameModeService => _sessionContext.GameModeService;

        public BindableProperty<bool> IsMatchmaking { get; } = new BindableProperty<bool>(false);
        public BindableProperty<int> PlayerCount { get; } = new BindableProperty<int>(1);

        public BindableProperty<string> MapName { get; } = new BindableProperty<string>("Loading...");
        public BindableProperty<string> MapSubtitle { get; } = new BindableProperty<string>("");
        public BindableProperty<string> RotationTimer { get; } = new BindableProperty<string>("");

        [Inject]
        public LobbyViewModel(ISessionContext sessionContext, IAppFlow appFlow)
        {
            _sessionContext = sessionContext;
            _appFlow = appFlow;
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
            string modeId = GameModeService?.SelectedGameMode?.Id;
            int teamSize = 2; // Default team size for 2v2
            _appFlow.RequestPlay(new PlayRequest
            {
                ModeId = modeId,
                TeamSize = teamSize
            });
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
