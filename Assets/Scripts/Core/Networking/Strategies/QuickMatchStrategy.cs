using Core.Networking.Common;
using Core.Networking.Interfaces;
using UnityEngine;

namespace Core.Networking.Strategies
{
    /// <summary>
    /// Quick match strategy - creates a public lobby immediately
    /// </summary>
    public class QuickMatchStrategy : IMatchmakingStrategy
    {
        private readonly ILobbyManager _lobbyManager;

        public QuickMatchStrategy(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public void Execute(GameMode gameMode, int minPlayers, int maxPlayers)
        {
            Debug.Log($"[QuickMatchStrategy] Creating public lobby for {gameMode}");
            _lobbyManager.CreateLobby("Matchmaking", maxPlayers, false);
        }

        public void SearchForLobbies(GameMode gameMode)
        {
            Debug.Log($"[QuickMatchStrategy] Searching for lobbies with game mode: {gameMode}");
            // Could be extended to search and join existing lobbies
            // For now, quick match always creates a new lobby
        }
    }
}
