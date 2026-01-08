using System;
using Modules.Networking.Common;
using Epic.OnlineServices;

namespace Modules.Networking.Interfaces
{
    /// <summary>
    /// Interface for player-specific operations
    /// </summary>
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
