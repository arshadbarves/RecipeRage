using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public interface IPlayerManager
    {
        void SetCurrentLobby(PlayEveryWare.EpicOnlineServices.Samples.Lobby lobby);
        void SetPlayerReady(bool isReady);
        void SetPlayerTeam(TeamId teamId);
        void SetPlayerCharacterClass(int characterClassId);
        void InviteFriend(string friendProductUserId);
        void KickPlayer(string playerProductUserId);
    }
}
