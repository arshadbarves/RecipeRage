using System;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.Net
{
    /// <summary>
    /// EOS Lobby integration. Dev mode: in-editor lobbies are simulated with a
    /// local registry so the full flow is testable before EOS credentials land.
    /// Production wiring uses EOS Lobby + Sessions (product/sandbox/deployment
    /// from the EOS plugin config).
    /// </summary>
    public sealed class EOSLobbyService : ILobbyService
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;

        public event Action<int> OnPlayersChanged;
        public int ConnectedPlayerCount { get; private set; }
        public int MaxPlayers { get; private set; }
        public string CurrentLobbyId { get; private set; }

        public EOSLobbyService(IAuthService auth, ILoggingService log)
        {
            _auth = auth;
            _log = log;
        }

        public Task<string> CreateLobby(int maxPlayers, int teamSize)
        {
            // EOS: LobbyInterface.CreateLobby with BucketId = $"team{teamSize}"
            MaxPlayers = maxPlayers;
            ConnectedPlayerCount = 1;
            CurrentLobbyId = "dev_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            _log.Log($"[Lobby] Created {CurrentLobbyId} ({maxPlayers}p, team {teamSize})");
            OnPlayersChanged?.Invoke(ConnectedPlayerCount);
            return Task.FromResult(CurrentLobbyId);
        }

        public Task<bool> JoinLobby(string lobbyId)
        {
            // EOS: LobbyInterface.JoinLobby
            CurrentLobbyId = lobbyId;
            ConnectedPlayerCount++;
            _log.Log($"[Lobby] Joined {lobbyId} ({ConnectedPlayerCount} players)");
            OnPlayersChanged?.Invoke(ConnectedPlayerCount);
            return Task.FromResult(true);
        }

        public Task<string> QuickMatch(int teamSize)
        {
            // EOS: LobbyInterface.CreateLobbySearch by BucketId, join or create.
            _log.Log($"[Lobby] QuickMatch team {teamSize} (dev mode: auto-create)");
            return CreateLobby(teamSize * 2, teamSize);
        }

        public Task LeaveLobby()
        {
            _log.Log($"[Lobby] Left {CurrentLobbyId}");
            CurrentLobbyId = null;
            ConnectedPlayerCount = 0;
            OnPlayersChanged?.Invoke(0);
            return Task.CompletedTask;
        }
    }
}
