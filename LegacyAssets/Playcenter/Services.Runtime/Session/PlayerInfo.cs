using System.Collections.Generic;

namespace Playcenter.Services
{
    public sealed class PlayerInfo
    {
        public string PlayerId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = "Player";
        public bool IsLocal { get; set; }
        public bool IsHost { get; set; }
        public TeamId Team { get; set; } = TeamId.TeamA;
        public bool IsReady { get; set; }
        public bool IsBot { get; set; }
        /// <summary>Character class index. Cast to title-specific enum in game code.</summary>
        public int CharacterClassId { get; set; }
        /// <summary>Backend product user id as string.</summary>
        public string ProductUserId { get; set; }
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}
