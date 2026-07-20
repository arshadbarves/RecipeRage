using KitchenClash.Domain;
using Playcenter.Services;

namespace KitchenClash.Application
{
    public interface IPlayerManager
    {
        void SetCurrentLobby(LobbyInfo lobby);
        void SetPlayerReady(bool isReady);
        void SetPlayerTeam(TeamId teamId);
        void SetPlayerCharacterClass(int characterClassId);
        void InviteFriend(string friendProductUserId);
        void KickPlayer(string playerProductUserId);
    }
}
