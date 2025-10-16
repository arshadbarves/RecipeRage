using System;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Handles matchmaking logic and state
    /// </summary>
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly IMatchmakingStrategy _strategy;

        private bool _isSearchingForMatch;
        private float _searchStartTime;
        private int _minPlayersForMatch = 2;
        private int _maxPlayersForMatch = 4;

        public event Action OnMatchmakingStarted;
        public event Action OnMatchmakingCancelled;
        public event Action<int> OnPlayersFoundUpdated;
        public event Action OnMatchFound;

        public bool IsSearchingForMatch => _isSearchingForMatch;
        public float SearchTime => _isSearchingForMatch ? Time.time - _searchStartTime : 0f;

        public MatchmakingService(ILobbyManager lobbyManager, IMatchmakingStrategy strategy)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public void StartMatchmaking(GameMode gameMode, int minPlayers = 2, int maxPlayers = 4)
        {
            if (_isSearchingForMatch)
            {
                Debug.LogWarning("[MatchmakingService] Already searching for match");
                return;
            }

            _minPlayersForMatch = minPlayers;
            _maxPlayersForMatch = maxPlayers;
            _isSearchingForMatch = true;
            _searchStartTime = Time.time;

            Debug.Log($"[MatchmakingService] Starting matchmaking for {gameMode}");
            OnMatchmakingStarted?.Invoke();

            _strategy.Execute(gameMode, minPlayers, maxPlayers);
        }

        public void CancelMatchmaking()
        {
            if (!_isSearchingForMatch) return;

            _isSearchingForMatch = false;
            _lobbyManager.LeaveLobby();

            OnMatchmakingCancelled?.Invoke();
            Debug.Log("[MatchmakingService] Cancelled matchmaking");
        }

        public void SearchForLobbies(GameMode gameMode)
        {
            _strategy.SearchForLobbies(gameMode);
        }

        public void NotifyMatchmakingFailed()
        {
            if (_isSearchingForMatch)
            {
                _isSearchingForMatch = false;
                OnMatchmakingCancelled?.Invoke();
            }
        }

        public void CheckForMatchReady(int currentPlayerCount, bool allPlayersReady)
        {
            if (!_isSearchingForMatch) return;

            OnPlayersFoundUpdated?.Invoke(currentPlayerCount);

            if (currentPlayerCount >= _minPlayersForMatch && allPlayersReady)
            {
                _isSearchingForMatch = false;
                OnMatchFound?.Invoke();
                Debug.Log($"[MatchmakingService] Match found with {currentPlayerCount} players");
            }
        }
    }
}
