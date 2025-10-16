using System;
using Core.Networking.Interfaces;

namespace Core.Networking
{
    /// <summary>
    /// Interface for all networking services
    /// </summary>
    public interface INetworkingServices : IDisposable
    {
        ILobbyManager LobbyManager { get; }
        IPlayerManager PlayerManager { get; }
        IMatchmakingService MatchmakingService { get; }
        ITeamManager TeamManager { get; }
    }
}
