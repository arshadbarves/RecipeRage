using System.Collections.Generic;

namespace Core.Networking.Common
{
    /// <summary>
    /// Configuration for creating a lobby
    /// </summary>
    public class LobbyConfig
    {
        /// <summary>
        /// Type of lobby to create
        /// </summary>
        public LobbyType Type { get; set; } = LobbyType.Party;
        
        /// <summary>
        /// Display name for the lobby
        /// </summary>
        public string LobbyName { get; set; }
        
        /// <summary>
        /// Maximum number of players
        /// </summary>
        public int MaxPlayers { get; set; } = 4;
        
        /// <summary>
        /// Whether the lobby is private (invite-only)
        /// </summary>
        public bool IsPrivate { get; set; } = true;
        
        /// <summary>
        /// Game mode
        /// </summary>
        public GameMode GameMode { get; set; } = GameMode.Classic;
        
        /// <summary>
        /// Map name
        /// </summary>
        public string MapName { get; set; }
        
        /// <summary>
        /// Team size (for team-based modes)
        /// </summary>
        public int TeamSize { get; set; } = 4;
        
        /// <summary>
        /// Whether to allow invites
        /// </summary>
        public bool AllowInvites { get; set; } = true;
        
        /// <summary>
        /// Whether to enable presence (show in friends list)
        /// </summary>
        public bool PresenceEnabled { get; set; } = true;
        
        /// <summary>
        /// Whether to enable RTC (voice chat)
        /// </summary>
        public bool RTCEnabled { get; set; } = false;
        
        /// <summary>
        /// Custom attributes for the lobby
        /// </summary>
        public Dictionary<string, string> CustomAttributes { get; set; } = new Dictionary<string, string>();
    }
}
