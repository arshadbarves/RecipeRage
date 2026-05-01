using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public sealed class LobbyConfig
    {
        public LobbyType Type { get; set; } = LobbyType.Party;
        public string LobbyName { get; set; }
        public int MaxPlayers { get; set; } = 4;
        public bool IsPrivate { get; set; } = true;
        public string GameModeId { get; set; } = "classic";
        public string MapName { get; set; }
        public int TeamSize { get; set; } = 2;
        public bool AllowInvites { get; set; } = true;
        public bool PresenceEnabled { get; set; }
        public bool RTCEnabled { get; set; }
        public Dictionary<string, string> CustomAttributes { get; set; } = new();
    }
}
