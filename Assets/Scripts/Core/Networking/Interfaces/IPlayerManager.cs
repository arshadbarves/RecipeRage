using System;
using Core.Networking.Common;
using Epic.OnlineServices;

namespace Core.Networking.Interfaces
{
    public interface IPlayerManager
    {
        // Events
        event Action<PlayerInfo> OnPlayerJoined;
        event Action<PlayerInfo> OnPlayerLeft;

        // Methods
        void SetPlayerReady(bool isReady);
        void SetPlayerTeam(TeamId teamId);
        void SetPlayerCharacterClass(CharacterClass characterClass);
        void InviteFriend(ProductUserId friendId);
        void KickPlayer(ProductUserId playerId);
    }
}
