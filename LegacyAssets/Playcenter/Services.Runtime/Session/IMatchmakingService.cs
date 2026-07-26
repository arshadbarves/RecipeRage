using System;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface IMatchmakingService
    {
        event Action OnMatchmakingStarted;
        event Action OnMatchmakingCancelled;
        event Action<string> OnMatchmakingFailed;
        event Action<int, int> OnPlayersFound;
        event Action<LobbyInfo> OnMatchFound;

        bool IsSearching { get; }
        int PlayersFound { get; }
        int RequiredPlayers { get; }

        void Initialize();
        void FindMatch(string gameModeId, int teamSize);
        void CancelMatchmaking();
        void SearchForMatchLobbies(string gameModeId, int teamSize, int neededPlayers);
        void CreateAndWaitForPlayers(string gameModeId, int teamSize);
        void FillMatchWithBots();
        List<BotPlayer> GetActiveBots();
    }
}
