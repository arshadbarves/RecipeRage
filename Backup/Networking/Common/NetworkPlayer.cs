using System.Collections.Generic;

namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// Class representing a player in the network.
    /// </summary>
    public class NetworkPlayer
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
        /// Create a new network player.
        /// </summary>
        public NetworkPlayer()
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
    }
}
