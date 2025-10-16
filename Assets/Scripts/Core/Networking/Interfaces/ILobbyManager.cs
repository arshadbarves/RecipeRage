using System;
using System.Collections.Generic;
using Core.Networking.Common;
using Epic.OnlineServices;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for lobby management operations
    /// </summary>
    public interface ILobbyManager
    {
        // Events
        event Action<Result> OnLobbyCreated;
        event Action<Result> OnLobbyJoined;
        event Action<Result> OnLobbyLeft;
        event Action OnLobbyUpdated;
        
        // Properties
        bool IsLobbyOwner { get; }
        int CurrentPlayerCount { get; }
        GameMode CurrentGameMode { get; }
        string CurrentMapName { get; }
        bool IsPrivate { get; }
        
        // Methods
        void Initialize();
        void CreateLobby(string lobbyName, int maxPlayers = 4, bool isPrivate = false);
        void JoinLobby(string lobbyId, bool presenceEnabled = true);
        void LeaveLobby();
        void SetGameMode(GameMode gameMode);
        void SetMapName(string mapName);
        bool AreAllPlayersReady();
    }
}
