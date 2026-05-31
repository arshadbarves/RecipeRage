using Epic.OnlineServices;
using KitchenClash.Domain;
using System;

namespace KitchenClash.Application
{
    public interface ILobbyManager : IDisposable
    {
        // Events - Match
        event Action<Result, LobbyInfo> OnMatchLobbyCreated;
        event Action<Result, LobbyInfo> OnMatchLobbyJoined;
        event Action OnMatchLobbyLeft;
        event Action OnMatchLobbyUpdated;

        // Events - State
        event Action<LobbyState> OnLobbyStateChanged;
        event Action<string> OnError;

        // Properties
        LobbyInfo CurrentPartyLobby { get; }
        LobbyInfo CurrentMatchLobby { get; }
        LobbyState CurrentState { get; }
        bool IsInParty { get; }
        bool IsInMatchLobby { get; }
        bool IsPartyLeader { get; }
        bool IsMatchLobbyOwner { get; }

        // Party methods
        void Initialize();
        void CreatePartyLobby(LobbyConfig config);
        void InviteToParty(string friendProductUserId);
        void LeaveParty();
        void UpdatePartySettings(LobbyConfig config);

        // Match methods
        void CreateMatchLobby(LobbyConfig config);
        void JoinMatchLobby(string lobbyId);
        void LeaveMatchLobby();
        void DestroyMatchLobby();

        // Game settings
        void SetGameMode(string gameModeId);
        void SetMapName(string mapName);

        // Utility
        bool AreAllPlayersReady();
        LobbyInfo GetLobbyInfo(string lobbyId);
    }
}
