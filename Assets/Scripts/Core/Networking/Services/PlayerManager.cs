using System;
using Core.Logging;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace Core.Networking.Services
{
    /// <summary>
    /// Handles player-specific operations
    /// </summary>
    public class PlayerManager : IPlayerManager
    {
        private readonly EOSLobbyManager _eosLobbyManager;
        private Lobby _currentLobby;

        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;

        public PlayerManager(EOSLobbyManager eosLobbyManager)
        {
            _eosLobbyManager = eosLobbyManager ?? throw new ArgumentNullException(nameof(eosLobbyManager));
        }

        public void SetCurrentLobby(Lobby lobby)
        {
            _currentLobby = lobby;
        }

        public void SetPlayerReady(bool isReady)
        {
            if (_currentLobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute("IsReady", isReady.ToString());
            GameLogger.Log($"Setting player ready: {isReady}");
        }

        public void SetPlayerTeam(TeamId teamId)
        {
            if (_currentLobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute("TeamId", ((int)teamId).ToString());
            GameLogger.Log($"Setting player team: {teamId}");
        }

        public void SetPlayerCharacterClass(CharacterClass characterClass)
        {
            if (_currentLobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute("CharacterClass", ((int)characterClass).ToString());
            GameLogger.Log($"Setting player character class: {characterClass}");
        }

        public void InviteFriend(ProductUserId friendId)
        {
            if (_currentLobby == null)
            {
                GameLogger.LogError("No current lobby to invite to");
                return;
            }

            if (!friendId.IsValid())
            {
                GameLogger.LogError("Invalid friend ProductUserId");
                return;
            }

            _eosLobbyManager.SendInvite(friendId);
            GameLogger.Log($"Sent invite to friend: {friendId}");
        }

        public void KickPlayer(ProductUserId playerId)
        {
            if (_currentLobby == null || !_currentLobby.IsOwner(EOSManager.Instance.GetProductUserId()))
            {
                GameLogger.LogWarning("Only lobby owner can kick players");
                return;
            }

            if (!playerId.IsValid())
            {
                GameLogger.LogError("Invalid player ProductUserId");
                return;
            }

            _eosLobbyManager.KickMember(playerId, (result) =>
            {
                if (result == Result.Success)
                {
                    GameLogger.Log($"Kicked player: {playerId}");
                }
                else
                {
                    GameLogger.LogError($"Failed to kick player: {result}");
                }
            });
        }

        private bool IsPlayerInLobby(ProductUserId userId)
        {
            foreach (LobbyMember member in _currentLobby.Members)
            {
                if (member.ProductId == userId)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddMemberAttribute(string key, string value)
        {
            LobbyAttribute attribute = new LobbyAttribute
            {
                Key = key,
                ValueType = AttributeType.String,
                AsString = value,
                Visibility = LobbyAttributeVisibility.Public
            };

            _eosLobbyManager.SetMemberAttribute(attribute);
        }
    }
}
