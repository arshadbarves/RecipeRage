using KitchenClash.Domain;
using System.Collections.Generic;

namespace KitchenClash.Application
{
    public interface ITeamManager
    {
        List<PlayerInfo> TeamA { get; }
        List<PlayerInfo> TeamB { get; }
        void UpdateTeams();
        void UpdateTeamsFromLobby(PlayEveryWare.EpicOnlineServices.Samples.Lobby lobby);
        PlayerInfo GetPlayerInfo(string playerId);
        void Clear();
    }
}
