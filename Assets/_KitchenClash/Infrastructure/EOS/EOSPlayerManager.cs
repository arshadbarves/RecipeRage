using KitchenClash.Application;
using System;
using KitchenClash.Domain;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Handles player-specific operations
    /// </summary>
    public class EOSPlayerManager : IPlayerManager
    {
        private readonly EOSLobbyManager _eosLobbyManager;
        private Lobby _currentLobby;

        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;

        public EOSPlayerManager(EOSLobbyManager eosLobbyManager)
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

        public void SetPlayerCharacterClass(int characterClassId)
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

            AddMemberAttribute("CharacterClass", characterClassId.ToString());
            GameLogger.Log($"Setting player character class: {characterClassId}");
        }

        public void InviteFriend(string friendProductUserId)
        {
            if (_currentLobby == null)
            {
                GameLogger.LogError("No current lobby to invite to");
                return;
            }

            if (string.IsNullOrEmpty(friendProductUserId))
            {
                GameLogger.LogError("Invalid friend ProductUserId");
                return;
            }

            var friendId = ProductUserId.FromString(friendProductUserId);
            _eosLobbyManager.SendInvite(friendId);
            GameLogger.Log($"Sent invite to friend: {friendProductUserId}");
        }

        public void KickPlayer(string playerProductUserId)
        {
            if (_currentLobby == null || !_currentLobby.IsOwner(EOSManager.Instance.GetProductUserId()))
            {
                GameLogger.LogWarning("Only lobby owner can kick players");
                return;
            }

            if (string.IsNullOrEmpty(playerProductUserId))
            {
                GameLogger.LogError("Invalid player ProductUserId");
                return;
            }

            var playerId = ProductUserId.FromString(playerProductUserId);
            _eosLobbyManager.KickMember(playerId, (result) =>
            {
                if (result == Result.Success)
                {
                    GameLogger.Log($"Kicked player: {playerProductUserId}");
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
