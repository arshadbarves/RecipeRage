using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public sealed class LobbyInfo
    {
        public string LobbyId { get; set; }
        public LobbyType Type { get; set; }
        public string LobbyName { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public bool IsPrivate { get; set; }
        public string GameModeId { get; set; } = "classic";
        public string MapName { get; set; }
        public int TeamSize { get; set; }
        public string LeaderId { get; set; }
        public string OwnerId { get; set; }
        public List<PlayerInfo> Players { get; set; } = new();
        public int AvailableSlots => MaxPlayers - CurrentPlayers;
        public bool IsFull => CurrentPlayers >= MaxPlayers;
        public bool IsSearching { get; set; }
        public string Status { get; set; } = "Idle";
        public Dictionary<string, string> Attributes { get; set; } = new();

        public bool IsOwner(string userId) => OwnerId == userId;
        public bool IsLeader(string userId) => LeaderId == userId;
    }
}
