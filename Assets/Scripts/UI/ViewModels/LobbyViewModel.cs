using System;
using System.Threading;
using Core.Networking.Interfaces;
using Core.Reactive;
using Core.State;
using Core.State.States;
using Cysharp.Threading.Tasks;
using UI.Core;
using UI.Data;
using UnityEngine;
using VContainer;

namespace UI.ViewModels
{
    public class LobbyViewModel : BaseViewModel
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameStateManager _stateManager;
        private MapDatabase _mapDatabase;
        private CancellationTokenSource _cts;

        public BindableProperty<bool> IsMatchmaking { get; } = new BindableProperty<bool>(false);
        public BindableProperty<int> PlayerCount { get; } = new BindableProperty<int>(1);
        
        // Map Data
        public BindableProperty<string> MapName { get; } = new BindableProperty<string>("Loading...");
        public BindableProperty<string> MapSubtitle { get; } = new BindableProperty<string>("");
        public BindableProperty<string> RotationTimer { get; } = new BindableProperty<string>("");

        [Inject]
        public LobbyViewModel(IMatchmakingService matchmakingService, IGameStateManager stateManager)
        {
            _matchmakingService = matchmakingService;
            _stateManager = stateManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadMapData();
            StartTimerLoop();
        }

        private void LoadMapData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null)
            {
                _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
                UpdateMapInfo();
            }
        }

        private void UpdateMapInfo()
        {
            if (_mapDatabase == null) return;
            var currentMap = _mapDatabase.GetCurrentMap();
            if (currentMap != null)
            {
                MapName.Value = currentMap.name;
                MapSubtitle.Value = currentMap.subtitle;
            }
        }

        private void StartTimerLoop()
        {
            _cts = new CancellationTokenSource();
            UpdateTimerAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid UpdateTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_mapDatabase != null)
                {
                    TimeSpan remaining = _mapDatabase.GetTimeUntilRotation();
                    RotationTimer.Value = remaining.TotalSeconds > 0 
                        ? $"NEW MAP IN : {remaining.Hours}h {remaining.Minutes}m" 
                        : "NEW MAP IN : --h --m";
                }
                await UniTask.Delay(TimeSpan.FromMinutes(1), cancellationToken: token);
            }
        }

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
            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}