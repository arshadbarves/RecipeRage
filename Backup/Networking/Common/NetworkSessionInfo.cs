using System.Collections.Generic;

namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// Class representing a network session.
    /// </summary>
    public class NetworkSessionInfo
    {
        /// <summary>
        /// The session's unique ID.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The session's name.
        /// </summary>
        public string SessionName { get; set; }

        /// <summary>
        /// The number of players in the session.
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// The maximum number of players allowed in the session.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Whether the session is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// The host's display name.
        /// </summary>
        public string HostName { get; set; }
        
        /// <summary>
        /// The game mode ID.
        /// </summary>
        public string GameMode { get; set; }
        
        /// <summary>
        /// The map name.
        /// </summary>
        public string MapName { get; set; }
        
        /// <summary>
        /// Custom data associated with the session.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Create a new network session info.
        /// </summary>
        public NetworkSessionInfo()
        {
            SessionId = string.Empty;
            SessionName = "Game Session";
            PlayerCount = 0;
            MaxPlayers = 4;
            IsPrivate = false;
            HostName = "Host";
            GameMode = "classic";
            MapName = "Kitchen";
        }
    }
}
