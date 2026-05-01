using KitchenClash.Domain;
using System;
using KitchenClash.Application.Models;

namespace KitchenClash.Application
{
    public interface IPlayerManager
    {
        event Action<PlayerInfo> OnPlayerJoined;
        event Action<PlayerInfo> OnPlayerLeft;

        void SetCurrentLobby(PlayEveryWare.EpicOnlineServices.Samples.Lobby lobby);
        void SetPlayerReady(bool isReady);
        void SetPlayerTeam(TeamId teamId);
        void SetPlayerCharacterClass(int characterClassId);
        void InviteFriend(string friendProductUserId);
        void KickPlayer(string playerProductUserId);
    }
}
