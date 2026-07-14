using KitchenClash.Application;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Manages team assignments and player info caching from Domain LobbyInfo snapshots.
    /// </summary>
    public class EOSTeamManager : ITeamManager
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

        public void UpdateTeamsFromLobby(LobbyInfo lobby)
        {
            _teamA.Clear();
            _teamB.Clear();

            if (lobby?.Players == null)
            {
                return;
            }

            foreach (PlayerInfo source in lobby.Players)
            {
                if (source == null || string.IsNullOrEmpty(source.PlayerId))
                {
                    continue;
                }

                PlayerInfo playerInfo = GetOrCreatePlayerInfo(source, lobby);

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

        private PlayerInfo GetOrCreatePlayerInfo(PlayerInfo source, LobbyInfo lobby)
        {
            if (_playerInfoCache.TryGetValue(source.PlayerId, out PlayerInfo playerInfo))
            {
                CopyPlayerFields(playerInfo, source, lobby);
            }
            else
            {
                playerInfo = new PlayerInfo();
                CopyPlayerFields(playerInfo, source, lobby);
                _playerInfoCache[source.PlayerId] = playerInfo;
            }

            return playerInfo;
        }

        private static void CopyPlayerFields(PlayerInfo target, PlayerInfo source, LobbyInfo lobby)
        {
            target.PlayerId = source.PlayerId;
            target.DisplayName = source.DisplayName;
            target.IsHost = source.IsHost || lobby.IsOwner(source.PlayerId);
            target.IsLocal = source.IsLocal;
            target.ProductUserId = source.ProductUserId ?? source.PlayerId;
            target.IsReady = source.IsReady;
            target.Team = source.Team;
            target.CharacterClassId = source.CharacterClassId;
        }

        public void Clear()
        {
            _teamA.Clear();
            _teamB.Clear();
            _playerInfoCache.Clear();
        }
    }
}
