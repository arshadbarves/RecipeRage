using System;
using System.Threading;
using Core.Networking.Interfaces;
using Cysharp.Threading.Tasks;
using Core.Localization;
using Core.Networking.Common;
using Core.Shared;
using Core.UI.Core;
using Core.Session;
using UnityEngine;
using VContainer;

namespace Gameplay.UI.Features.Matchmaking
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
        public BindableProperty<string> GameModeText { get; } = new BindableProperty<string>("FREE FOR ALL");
        public BindableProperty<string> MapNameText { get; } = new BindableProperty<string>("Loading...");

        private IMatchmakingService MatchmakingService => _sessionManager.SessionContainer?.Resolve<IMatchmakingService>();

        [Inject]
        public MatchmakingViewModel(SessionManager sessionManager, ILocalizationManager localization)
        {
            _sessionManager = sessionManager;
            _localization = localization;
        }

        public void OnShow()
        {
            StatusText.Value = _localization.GetText("matchmaking_status_searching");
            IsMatchFound.Value = false;
            _rawSearchTime = 0f;

            // Get current game mode from service if available
            var service = MatchmakingService;
            if (service != null)
            {
                // Update player count text with current values
                PlayerCountText.Value = $"{service.PlayersFound}/{service.RequiredPlayers}";
            }

            // Load map data (similar to LobbyViewModel)
            LoadMapData();

            SubscribeToEvents();
            StartTimer();
        }

        private void LoadMapData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null)
            {
                var mapDatabase = JsonUtility.FromJson<Gameplay.UI.Data.MapDatabase>(jsonFile.text);
                var currentMap = mapDatabase?.GetCurrentMap();
                if (currentMap != null)
                {
                    MapNameText.Value = currentMap.name;
                }
            }
        }

        public void OnHide()
        {
            UnsubscribeFromEvents();
            StopTimer();
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
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            UpdateTimerAsync(_cts.Token).Forget();
        }

        private void StopTimer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
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
            Debug.Log("ViewModel: CancelMatchmaking requested");
            var service = MatchmakingService;
            if (service == null)
            {
                Debug.LogError("ViewModel: MatchmakingService is null!");
                return;
            }
            
            service.CancelMatchmaking();
        }

        public override void Dispose()
        {
            StopTimer();
            base.Dispose();
        }
    }
}