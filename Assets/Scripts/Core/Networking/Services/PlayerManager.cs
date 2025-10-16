using System;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Sessions;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

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
                Debug.LogError("[PlayerManager] No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                Debug.LogError("[PlayerManager] Local player not found in lobby");
                return;
            }

            AddMemberAttribute("IsReady", isReady.ToString());
            Debug.Log($"[PlayerManager] Setting player ready: {isReady}");
        }

        public void SetPlayerTeam(TeamId teamId)
        {
            if (_currentLobby == null)
            {
                Debug.LogError("[PlayerManager] No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                Debug.LogError("[PlayerManager] Local player not found in lobby");
                return;
            }

            AddMemberAttribute("TeamId", ((int)teamId).ToString());
            Debug.Log($"[PlayerManager] Setting player team: {teamId}");
        }

        public void SetPlayerCharacterClass(CharacterClass characterClass)
        {
            if (_currentLobby == null)
            {
                Debug.LogError("[PlayerManager] No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(localUserId))
            {
                Debug.LogError("[PlayerManager] Local player not found in lobby");
                return;
            }

            AddMemberAttribute("CharacterClass", ((int)characterClass).ToString());
            Debug.Log($"[PlayerManager] Setting player character class: {characterClass}");
        }

        public void InviteFriend(ProductUserId friendId)
        {
            if (_currentLobby == null)
            {
                Debug.LogError("[PlayerManager] No current lobby to invite to");
                return;
            }

            if (!friendId.IsValid())
            {
                Debug.LogError("[PlayerManager] Invalid friend ProductUserId");
                return;
            }

            _eosLobbyManager.SendInvite(friendId);
            Debug.Log($"[PlayerManager] Sent invite to friend: {friendId}");
        }

        public void KickPlayer(ProductUserId playerId)
        {
            if (_currentLobby == null || !_currentLobby.IsOwner(EOSManager.Instance.GetProductUserId()))
            {
                Debug.LogWarning("[PlayerManager] Only lobby owner can kick players");
                return;
            }

            if (!playerId.IsValid())
            {
                Debug.LogError("[PlayerManager] Invalid player ProductUserId");
                return;
            }

            _eosLobbyManager.KickMember(playerId, (result) =>
            {
                if (result == Result.Success)
                {
                    Debug.Log($"[PlayerManager] Kicked player: {playerId}");
                }
                else
                {
                    Debug.LogError($"[PlayerManager] Failed to kick player: {result}");
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
