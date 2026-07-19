using KitchenClash.Application;
using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Player ops against dual-track party/match lobbies.
    /// Uses explicit lobby ids via <see cref="LobbyInterface"/> — never sample
    /// <c>EOSLobbyManager.CurrentLobby</c> (create/join never populate it).
    /// </summary>
    public class EOSPlayerManager : IPlayerManager
    {
        private readonly ILobbyManager _lobbyManager;
        private LobbyInfo _currentLobby;

        public EOSPlayerManager(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
        }

        public void SetCurrentLobby(LobbyInfo lobby)
        {
            _currentLobby = lobby;
        }

        public void SetPlayerReady(bool isReady)
        {
            LobbyInfo lobby = ResolveActiveLobby();
            if (lobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(lobby, localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute(lobby.LobbyId, "IsReady", isReady.ToString());
            GameLogger.Log($"Setting player ready: {isReady} on lobby {lobby.LobbyId}");
        }

        public void SetPlayerTeam(TeamId teamId)
        {
            LobbyInfo lobby = ResolveActiveLobby();
            if (lobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(lobby, localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute(lobby.LobbyId, "TeamId", ((int)teamId).ToString());
            GameLogger.Log($"Setting player team: {teamId} on lobby {lobby.LobbyId}");
        }

        public void SetPlayerCharacterClass(int characterClassId)
        {
            LobbyInfo lobby = ResolveActiveLobby();
            if (lobby == null)
            {
                GameLogger.LogError("No current lobby");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!IsPlayerInLobby(lobby, localUserId))
            {
                GameLogger.LogError("Local player not found in lobby");
                return;
            }

            AddMemberAttribute(lobby.LobbyId, "CharacterClass", characterClassId.ToString());
            GameLogger.Log($"Setting player character class: {characterClassId} on lobby {lobby.LobbyId}");
        }

        public void InviteFriend(string friendProductUserId)
        {
            if (string.IsNullOrEmpty(friendProductUserId))
            {
                GameLogger.LogError("Invalid friend ProductUserId");
                return;
            }

            // Social invites target the party when present (Brawl-style); otherwise match lobby.
            if (_lobbyManager.IsInParty)
            {
                _lobbyManager.InviteToParty(friendProductUserId);
                GameLogger.Log($"Party invite delegated for: {friendProductUserId}");
                return;
            }

            LobbyInfo lobby = ResolveActiveLobby();
            if (lobby == null || string.IsNullOrEmpty(lobby.LobbyId))
            {
                GameLogger.LogError("No current lobby to invite to");
                return;
            }

            SendInvite(lobby.LobbyId, friendProductUserId);
            GameLogger.Log($"Sent invite to friend: {friendProductUserId} for lobby {lobby.LobbyId}");
        }

        public void KickPlayer(string playerProductUserId)
        {
            LobbyInfo lobby = ResolveActiveLobby();
            if (lobby == null || string.IsNullOrEmpty(lobby.LobbyId))
            {
                GameLogger.LogWarning("No current lobby to kick from");
                return;
            }

            string localUserId = EOSManager.Instance.GetProductUserId()?.ToString();
            if (!lobby.IsOwner(localUserId))
            {
                GameLogger.LogWarning("Only lobby owner can kick players");
                return;
            }

            if (string.IsNullOrEmpty(playerProductUserId))
            {
                GameLogger.LogError("Invalid player ProductUserId");
                return;
            }

            ProductUserId localPuid = EOSManager.Instance.GetProductUserId();
            ProductUserId targetPuid = ProductUserId.FromString(playerProductUserId);
            if (localPuid == null || !localPuid.IsValid() || targetPuid == null || !targetPuid.IsValid())
            {
                GameLogger.LogError("Invalid user id for kick");
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null)
            {
                GameLogger.LogError("LobbyInterface unavailable for kick");
                return;
            }

            var kickOptions = new KickMemberOptions
            {
                LobbyId = lobby.LobbyId,
                LocalUserId = localPuid,
                TargetUserId = targetPuid
            };

            lobbyInterface.KickMember(ref kickOptions, null, (ref KickMemberCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    GameLogger.Log($"Kicked player: {playerProductUserId} from {lobby.LobbyId}");
                }
                else
                {
                    GameLogger.LogError($"Failed to kick player: {data.ResultCode}");
                }
            });
        }

        /// <summary>
        /// Prefer explicit SetCurrentLobby, else match lobby (ready/team UI), else party.
        /// </summary>
        private LobbyInfo ResolveActiveLobby()
        {
            if (_currentLobby != null && !string.IsNullOrEmpty(_currentLobby.LobbyId))
            {
                return _currentLobby;
            }

            if (_lobbyManager.CurrentMatchLobby != null &&
                !string.IsNullOrEmpty(_lobbyManager.CurrentMatchLobby.LobbyId))
            {
                return _lobbyManager.CurrentMatchLobby;
            }

            if (_lobbyManager.CurrentPartyLobby != null &&
                !string.IsNullOrEmpty(_lobbyManager.CurrentPartyLobby.LobbyId))
            {
                return _lobbyManager.CurrentPartyLobby;
            }

            return null;
        }

        private static bool IsPlayerInLobby(LobbyInfo lobby, ProductUserId userId)
        {
            if (lobby == null || string.IsNullOrEmpty(lobby.LobbyId))
            {
                return false;
            }

            // Empty roster after create is common before first refresh — allow attribute write.
            if (lobby.Players == null || lobby.Players.Count == 0 || userId == null)
            {
                return true;
            }

            string userIdString = userId.ToString();
            if (string.IsNullOrEmpty(userIdString))
            {
                return false;
            }

            foreach (PlayerInfo player in lobby.Players)
            {
                if (player != null &&
                    (player.PlayerId == userIdString || player.ProductUserId == userIdString))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddMemberAttribute(string lobbyId, string key, string value)
        {
            if (string.IsNullOrEmpty(lobbyId))
            {
                GameLogger.LogError("AddMemberAttribute: empty lobby id");
                return;
            }

            if (EOSManager.Instance == null)
            {
                GameLogger.LogError("AddMemberAttribute: EOSManager unavailable");
                return;
            }

            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (localUserId == null || !localUserId.IsValid())
            {
                GameLogger.LogError("AddMemberAttribute: local user invalid");
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null)
            {
                GameLogger.LogError("AddMemberAttribute: LobbyInterface unavailable");
                return;
            }

            var modOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            Result result = lobbyInterface.UpdateLobbyModification(ref modOptions, out LobbyModification modification);
            if (result != Result.Success || modification == null)
            {
                GameLogger.LogError($"AddMemberAttribute: could not create modification: {result}");
                return;
            }

            var attributeData = new AttributeData
            {
                Key = key,
                Value = new AttributeDataValue { AsUtf8 = value }
            };

            var attrOptions = new LobbyModificationAddMemberAttributeOptions
            {
                Attribute = attributeData,
                Visibility = LobbyAttributeVisibility.Public
            };

            result = modification.AddMemberAttribute(ref attrOptions);
            if (result != Result.Success)
            {
                GameLogger.LogError($"AddMemberAttribute: could not add {key}: {result}");
                modification.Release();
                return;
            }

            var updateOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modification
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    GameLogger.LogError($"AddMemberAttribute: update failed for {key}: {data.ResultCode}");
                }

                modification.Release();
            });
        }

        private void SendInvite(string lobbyId, string friendProductUserId)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            ProductUserId targetUserId = ProductUserId.FromString(friendProductUserId);
            if (localUserId == null || !localUserId.IsValid() || targetUserId == null || !targetUserId.IsValid())
            {
                GameLogger.LogError("SendInvite: invalid user id");
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null)
            {
                GameLogger.LogError("SendInvite: LobbyInterface unavailable");
                return;
            }

            var options = new SendInviteOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId,
                TargetUserId = targetUserId
            };

            lobbyInterface.SendInvite(ref options, null, (ref SendInviteCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    GameLogger.LogError($"SendInvite failed: {data.ResultCode}");
                }
            });
        }
    }
}
