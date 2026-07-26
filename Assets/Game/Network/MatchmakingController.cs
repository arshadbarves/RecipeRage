using System;
using Playcenter.Net;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Quick-match flow: create/join lobby → when full, host starts server and
    /// clients connect → TeamCompositionState. UI lands in Slice 5; this is the
    /// logic core the UI calls.
    /// </summary>
    public sealed class MatchmakingController
    {
        private readonly ILobbyService _lobby;
        private readonly INetService _net;

        public event Action OnMatchFound;

        public MatchmakingController(ILobbyService lobby, INetService net)
        {
            _lobby = lobby;
            _net = net;
        }

        public async void QuickMatch(int teamSize)
        {
            var lobbyId = await _lobby.QuickMatch(teamSize);
            if (string.IsNullOrEmpty(lobbyId))
            {
                return;
            }

            // Dev flow: first player hosts; when lobby full, host starts server.
            _lobby.OnPlayersChanged += OnLobbyPlayersChanged;
        }

        private void OnLobbyPlayersChanged(int count)
        {
            if (count >= _lobby.MaxPlayers)
            {
                _lobby.OnPlayersChanged -= OnLobbyPlayersChanged;
                _net.StartHost();
                OnMatchFound?.Invoke();

                // Flow: matchmaking done → team compositions (5s) → countdown → match
                if (Playcenter.ServiceLocator.TryGet<IGameStateMachine>(out var stateMachine))
                {
                    stateMachine.ChangeState(new TeamCompositionState());
                }
            }
        }

        public void Cancel()
        {
            _lobby.OnPlayersChanged -= OnLobbyPlayersChanged;
            _ = _lobby.LeaveLobby();
            if (_net.IsRunning)
            {
                _net.Shutdown();
            }
        }
    }
}
