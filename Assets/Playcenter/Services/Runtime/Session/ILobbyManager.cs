using System;

namespace Playcenter.Services
{
    public interface ILobbyManager : IDisposable
    {
        event Action<LobbyOpResult, LobbyInfo> OnMatchLobbyCreated;
        event Action<LobbyOpResult, LobbyInfo> OnMatchLobbyJoined;
        event Action OnMatchLobbyLeft;
        event Action OnMatchLobbyUpdated;

        event Action<LobbyState> OnLobbyStateChanged;
        event Action<string> OnError;

        LobbyInfo CurrentPartyLobby { get; }
        LobbyInfo CurrentMatchLobby { get; }
        LobbyState CurrentState { get; }
        bool IsInParty { get; }
        bool IsInMatchLobby { get; }
        bool IsPartyLeader { get; }
        bool IsMatchLobbyOwner { get; }

        void Initialize();
        void CreatePartyLobby(LobbyConfig config);
        void InviteToParty(string friendProductUserId);
        void LeaveParty();
        void UpdatePartySettings(LobbyConfig config);

        void CreateMatchLobby(LobbyConfig config);
        void JoinMatchLobby(string lobbyId);
        void LeaveMatchLobby();
        void DestroyMatchLobby();

        void SetGameMode(string gameModeId);
        void SetMapName(string mapName);

        bool AreAllPlayersReady();
        LobbyInfo GetLobbyInfo(string lobbyId);
    }
}
