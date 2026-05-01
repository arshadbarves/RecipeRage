using System;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface ILobbyManager
    {
        Task<LobbyInfo> CreateLobbyAsync(LobbyConfig config);
        Task<bool> JoinLobbyAsync(string lobbyId);
        Task LeaveLobbyAsync();
        Task<bool> StartMatchAsync();
        LobbyInfo CurrentLobby { get; }
        event Action<LobbyInfo> OnLobbyUpdated;
    }
}
