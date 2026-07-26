using System;
using System.Threading.Tasks;

namespace Playcenter.Net
{
    public interface ILobbyService
    {
        event Action<int> OnPlayersChanged;
        int ConnectedPlayerCount { get; }
        int MaxPlayers { get; }
        string CurrentLobbyId { get; }

        Task<string> CreateLobby(int maxPlayers, int teamSize);
        Task<bool> JoinLobby(string lobbyId);
        Task<string> QuickMatch(int teamSize);
        Task LeaveLobby();
    }
}
