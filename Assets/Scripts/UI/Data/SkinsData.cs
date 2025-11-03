using System;
using System.Collections.Generic;

namespace UI.Data
{
    [Serializable]
    public class SkinsData
    {
        public string version;
        public List<SkinItem> skins;
    }

    [Serializable]
    public class SkinItem
    {
        public string id;
        public string name;
        public string description;
        public int characterId;           // NEW: Links skin to specific character
        public string characterName;      // NEW: Character name for display/filtering
        public string rarity;             // "common", "rare", "epic", "legendary"
        public bool unlockedByDefault;    // RENAMED: Was 'unlocked'
        public int unlockCost;            // NEW: Cost to unlock (coins)
        public string unlockType;         // NEW: "default", "purchase", "achievement", "event"
        public string prefabAddress;      // NEW: Addressable address for skin prefab
        public string iconAddress;        // NEW: Addressable address for skin icon
        public List<string> tags;         // NEW: Tags for filtering/searching
        
        // Legacy support - will be removed
        [Obsolete("Use unlockedByDefault instead")]
        public bool unlocked
        {
            get => unlockedByDefault;
            set => unlockedByDefault = value;
        }
        
        [Obsolete("Use iconAddress instead")]
        public string icon
        {
            get => iconAddress;
            set => iconAddress = value;
        }
    }
}
