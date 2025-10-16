using System;
using Core.Networking.Common;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for matchmaking operations
    /// </summary>
    public interface IMatchmakingService
    {
        // Events
        event Action OnMatchmakingStarted;
        event Action OnMatchmakingCancelled;
        event Action<int> OnPlayersFoundUpdated;
        event Action OnMatchFound;
        
        // Properties
        bool IsSearchingForMatch { get; }
        float SearchTime { get; }
        
        // Methods
        void StartMatchmaking(GameMode gameMode, int minPlayers = 2, int maxPlayers = 4);
        void CancelMatchmaking();
        void SearchForLobbies(GameMode gameMode);
        void CheckForMatchReady(int currentPlayerCount, bool allPlayersReady);
    }
}
