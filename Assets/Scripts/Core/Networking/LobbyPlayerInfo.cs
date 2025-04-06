using System;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Contains information about a player in the lobby.
    /// </summary>
    [Serializable]
    public class LobbyPlayerInfo
    {
        /// <summary>
        /// The player's unique ID.
        /// </summary>
        public string PlayerId { get; set; }
        
        /// <summary>
        /// The player's display name.
        /// </summary>
        public string PlayerName { get; set; }
        
        /// <summary>
        /// The player's team ID.
        /// </summary>
        public int TeamId { get; set; }
        
        /// <summary>
        /// The player's character type.
        /// </summary>
        public int CharacterType { get; set; }
        
        /// <summary>
        /// Whether the player is ready to start the game.
        /// </summary>
        public bool IsReady { get; set; }
        
        /// <summary>
        /// Whether the player is the host of the lobby.
        /// </summary>
        public bool IsHost { get; set; }
        
        /// <summary>
        /// Create a new LobbyPlayerInfo instance.
        /// </summary>
        public LobbyPlayerInfo()
        {
            PlayerId = Guid.NewGuid().ToString();
            PlayerName = "Player";
            TeamId = 0;
            CharacterType = 0;
            IsReady = false;
            IsHost = false;
        }
        
        /// <summary>
        /// Create a new LobbyPlayerInfo instance with the specified values.
        /// </summary>
        /// <param name="playerId">The player's unique ID</param>
        /// <param name="playerName">The player's display name</param>
        /// <param name="teamId">The player's team ID</param>
        /// <param name="characterType">The player's character type</param>
        /// <param name="isReady">Whether the player is ready</param>
        /// <param name="isHost">Whether the player is the host</param>
        public LobbyPlayerInfo(string playerId, string playerName, int teamId, int characterType, bool isReady, bool isHost)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            TeamId = teamId;
            CharacterType = characterType;
            IsReady = isReady;
            IsHost = isHost;
        }
        
        /// <summary>
        /// Create a LobbyPlayerInfo from a NetworkPlayer.
        /// </summary>
        /// <param name="networkPlayer">The NetworkPlayer to convert</param>
        /// <returns>A new LobbyPlayerInfo instance</returns>
        public static LobbyPlayerInfo FromNetworkPlayer(NetworkPlayer networkPlayer)
        {
            return new LobbyPlayerInfo(
                networkPlayer.PlayerId,
                networkPlayer.DisplayName,
                networkPlayer.TeamId,
                networkPlayer.CharacterType,
                networkPlayer.IsReady,
                networkPlayer.IsHost
            );
        }
    }
}
