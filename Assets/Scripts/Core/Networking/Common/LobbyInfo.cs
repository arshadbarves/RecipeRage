using System.Collections.Generic;
using Epic.OnlineServices;

namespace Core.Networking.Common
{
    /// <summary>
    /// Information about a lobby
    /// </summary>
    public class LobbyInfo
    {
        /// <summary>
        /// Unique lobby identifier
        /// </summary>
        public string LobbyId { get; set; }
        
        /// <summary>
        /// Type of lobby (Party or Match)
        /// </summary>
        public LobbyType Type { get; set; }
        
        /// <summary>
        /// Display name of the lobby
        /// </summary>
        public string LobbyName { get; set; }
        
        /// <summary>
        /// Maximum number of players
        /// </summary>
        public int MaxPlayers { get; set; }
        
        /// <summary>
        /// Current number of players
        /// </summary>
        public int CurrentPlayers { get; set; }
        
        /// <summary>
        /// Whether the lobby is private (invite-only)
        /// </summary>
        public bool IsPrivate { get; set; }
        
        /// <summary>
        /// Game mode for this lobby
        /// </summary>
        public GameMode GameMode { get; set; }
        
        /// <summary>
        /// Selected map name
        /// </summary>
        public string MapName { get; set; }
        
        /// <summary>
        /// Team size (for team-based modes)
        /// </summary>
        public int TeamSize { get; set; }
        
        /// <summary>
        /// Party leader's product user ID (for party lobbies)
        /// </summary>
        public ProductUserId PartyLeaderId { get; set; }
        
        /// <summary>
        /// Lobby owner's product user ID
        /// </summary>
        public ProductUserId OwnerId { get; set; }
        
        /// <summary>
        /// List of players in the lobby
        /// </summary>
        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();
        
        /// <summary>
        /// Available slots in the lobby
        /// </summary>
        public int AvailableSlots => MaxPlayers - CurrentPlayers;
        
        /// <summary>
        /// Whether the lobby is full
        /// </summary>
        public bool IsFull => CurrentPlayers >= MaxPlayers;
        
        /// <summary>
        /// Whether matchmaking is active (for party lobbies)
        /// </summary>
        public bool IsSearching { get; set; }
        
        /// <summary>
        /// Current status of the lobby
        /// </summary>
        public string Status { get; set; } = "Idle";
        
        /// <summary>
        /// Custom attributes for the lobby
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Check if the specified user is the owner
        /// </summary>
        public bool IsOwner(ProductUserId userId)
        {
            return OwnerId == userId;
        }
        
        /// <summary>
        /// Check if the specified user is the party leader
        /// </summary>
        public bool IsPartyLeader(ProductUserId userId)
        {
            return PartyLeaderId == userId;
        }
    }
}
