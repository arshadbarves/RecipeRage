using System.Collections.Generic;
using Core.Networking.Common;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for team management
    /// </summary>
    public interface ITeamManager
    {
        List<PlayerInfo> TeamA { get; }
        List<PlayerInfo> TeamB { get; }
        
        void UpdateTeams();
        PlayerInfo GetPlayerInfo(string playerId);
    }
}
