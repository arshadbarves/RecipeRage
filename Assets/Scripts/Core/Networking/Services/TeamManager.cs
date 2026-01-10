using System.Collections.Generic;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace Core.Networking.Services
{
    /// <summary>
    /// Manages team assignments and player info caching
    /// </summary>
    public class TeamManager : ITeamManager
    {
        private readonly List<PlayerInfo> _teamA = new List<PlayerInfo>();
        private readonly List<PlayerInfo> _teamB = new List<PlayerInfo>();
        private readonly Dictionary<string, PlayerInfo> _playerInfoCache = new Dictionary<string, PlayerInfo>();

        public List<PlayerInfo> TeamA => _teamA;
        public List<PlayerInfo> TeamB => _teamB;

        public void UpdateTeams()
        {
            _teamA.Clear();
            _teamB.Clear();
        }

        public void UpdateTeamsFromLobby(Lobby lobby)
        {
            _teamA.Clear();
            _teamB.Clear();

            if (lobby == null) return;

            foreach (LobbyMember member in lobby.Members)
            {
                PlayerInfo playerInfo = GetOrCreatePlayerInfo(member, lobby);

                if (playerInfo.Team == TeamId.TeamA)
                {
                    _teamA.Add(playerInfo);
                }
                else
                {
                    _teamB.Add(playerInfo);
                }
            }
        }

        public PlayerInfo GetPlayerInfo(string playerId)
        {
            _playerInfoCache.TryGetValue(playerId, out PlayerInfo playerInfo);
            return playerInfo;
        }

        private PlayerInfo GetOrCreatePlayerInfo(LobbyMember member, Lobby lobby)
        {
            string playerId = member.ProductId.ToString();

            if (_playerInfoCache.TryGetValue(playerId, out PlayerInfo playerInfo))
            {
                UpdatePlayerInfo(playerInfo, member, lobby);
            }
            else
            {
                playerInfo = CreatePlayerInfo(member, lobby);
                _playerInfoCache[playerId] = playerInfo;
            }

            ExtractMemberAttributes(playerInfo, member);
            return playerInfo;
        }

        private PlayerInfo CreatePlayerInfo(LobbyMember member, Lobby lobby)
        {
            return new PlayerInfo
            {
                PlayerId = member.ProductId.ToString(),
                DisplayName = member.DisplayName,
                IsHost = lobby.IsOwner(member.ProductId),
                IsLocal = member.ProductId == EOSManager.Instance.GetProductUserId(),
                ProductUserId = member.ProductId
            };
        }

        private void UpdatePlayerInfo(PlayerInfo playerInfo, LobbyMember member, Lobby lobby)
        {
            playerInfo.DisplayName = member.DisplayName;
            playerInfo.IsHost = lobby.IsOwner(member.ProductId);
            playerInfo.IsLocal = member.ProductId == EOSManager.Instance.GetProductUserId();
            playerInfo.ProductUserId = member.ProductId;
        }

        private void ExtractMemberAttributes(PlayerInfo playerInfo, LobbyMember member)
        {
            foreach (var kvp in member.MemberAttributes)
            {
                LobbyAttribute attribute = kvp.Value;

                switch (attribute.Key)
                {
                    case "IsReady":
                        playerInfo.IsReady = bool.TryParse(attribute.AsString, out bool isReady) && isReady;
                        break;
                    case "TeamId":
                        if (int.TryParse(attribute.AsString, out int teamId))
                        {
                            playerInfo.Team = (TeamId)teamId;
                        }
                        break;
                    case "CharacterClass":
                        if (int.TryParse(attribute.AsString, out int characterClass))
                        {
                            playerInfo.CharacterClass = (CharacterClass)characterClass;
                        }
                        break;
                }
            }
        }

        public void Clear()
        {
            _teamA.Clear();
            _teamB.Clear();
            _playerInfoCache.Clear();
        }
    }
}
