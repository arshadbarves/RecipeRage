using System;
using System.Threading;
using Modules.Shared.Interfaces;
using Modules.Localization;
using Modules.Networking;
using Modules.Networking.Common;
using Modules.Networking.Interfaces;
using Modules.Shared;
using Cysharp.Threading.Tasks;
using Modules.UI.Core;
using UnityEngine;
using VContainer;

namespace Gameplay.UI.ViewModels
{
    public class MatchmakingViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private readonly ILocalizationManager _localization;
        private CancellationTokenSource _cts;
        private float _rawSearchTime;

        public BindableProperty<string> StatusText { get; } = new BindableProperty<string>("");
        public BindableProperty<string> PlayerCountText { get; } = new BindableProperty<string>("0/0");
        public BindableProperty<string> SearchTimeText { get; } = new BindableProperty<string>("0:00");
        public BindableProperty<bool> IsMatchFound { get; } = new BindableProperty<bool>(false);

        private IMatchmakingService MatchmakingService => _sessionManager.SessionContainer?.Resolve<IMatchmakingService>();

        [Inject]
        public MatchmakingViewModel(SessionManager sessionManager, ILocalizationManager localization)
        {
            _sessionManager = sessionManager;
            _localization = localization;
        }

        public override void Initialize()
        {
            base.Initialize();
            StatusText.Value = _localization.GetText("matchmaking_status_searching");
            IsMatchFound.Value = false;
            _rawSearchTime = 0f;

            SubscribeToEvents();
            StartTimer();
        }

        private void SubscribeToEvents()
        {
            var service = MatchmakingService;
            if (service != null)
            {
                service.OnPlayersFound += HandlePlayersFound;
                service.OnMatchFound += HandleMatchFound;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var service = MatchmakingService;
            if (service != null)
            {
                service.OnPlayersFound -= HandlePlayersFound;
                service.OnMatchFound -= HandleMatchFound;
            }
        }

        private void StartTimer()
        {
            _cts = new CancellationTokenSource();
            UpdateTimerAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid UpdateTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!IsMatchFound.Value)
                {
                    _rawSearchTime += 1f;
                    int minutes = Mathf.FloorToInt(_rawSearchTime / 60f);
                    int seconds = Mathf.FloorToInt(_rawSearchTime % 60f);
                    SearchTimeText.Value = $"{minutes}:{seconds:00}";
                }
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
        }

        private void HandlePlayersFound(int current, int required)
        {
            PlayerCountText.Value = $"{current}/{required}";
        }

        private void HandleMatchFound(LobbyInfo lobbyInfo)
        {
            IsMatchFound.Value = true;
            StatusText.Value = _localization.GetText("matchmaking_status_found");
        }

        public void CancelMatchmaking()
        {
            MatchmakingService?.CancelMatchmaking();
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}