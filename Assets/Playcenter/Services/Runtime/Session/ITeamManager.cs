using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface ITeamManager
    {
        List<PlayerInfo> TeamA { get; }
        List<PlayerInfo> TeamB { get; }
        void UpdateTeams();
        void UpdateTeamsFromLobby(LobbyInfo lobby);
        PlayerInfo GetPlayerInfo(string playerId);
        void Clear();
    }
}
