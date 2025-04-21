using System.Collections.Generic;

namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// Class representing a player in the lobby.
    /// </summary>
    public class LobbyPlayerInfo
    {
        /// <summary>
        /// The player's unique ID.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The player's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this player is the local player.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Whether this player is the host.
        /// </summary>
        public bool IsHost { get; set; }

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
        /// Custom data associated with the player.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// Create a new lobby player info.
        /// </summary>
        public LobbyPlayerInfo()
        {
            PlayerId = string.Empty;
            DisplayName = "Player";
            IsLocal = false;
            IsHost = false;
            TeamId = 0;
            CharacterType = 0;
            IsReady = false;
            CustomData = new Dictionary<string, string>();
        }

        /// <summary>
        /// Create a lobby player info from a network player.
        /// </summary>
        /// <param name="player">The network player</param>
        /// <returns>A new lobby player info</returns>
        public static LobbyPlayerInfo FromNetworkPlayer(NetworkPlayer player)
        {
            return new LobbyPlayerInfo
            {
                PlayerId = player.PlayerId,
                DisplayName = player.DisplayName,
                IsLocal = player.IsLocal,
                IsHost = player.IsHost,
                TeamId = player.TeamId,
                CharacterType = player.CharacterType,
                IsReady = player.IsReady,
                CustomData = new Dictionary<string, string>(player.CustomData)
            };
        }
    }
}
