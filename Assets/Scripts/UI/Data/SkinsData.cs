using System;
using System.Collections.Generic;

namespace UI.Data
{
    [Serializable]
    public class SkinsData
    {
        public List<SkinItem> skins;
    }

    [Serializable]
    public class SkinItem
    {
        public string id;
        public string name;
        public string description;
        public string rarity; // "common", "rare", "epic", "legendary"
        public bool unlocked;
        public string icon;
    }
}
