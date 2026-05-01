using System.Collections.Generic;

namespace KitchenClash.Domain
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
        /// <summary>Character class index. Cast to CharacterClass in Infrastructure.</summary>
        public int CharacterClassId { get; set; }
        /// <summary>EOS ProductUserId as string.</summary>
        public string ProductUserId { get; set; }
        public Dictionary<string, string> CustomData { get; set; } = new();
    }
}
