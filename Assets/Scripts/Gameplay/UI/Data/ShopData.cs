using System;
using System.Collections.Generic;

namespace Gameplay.UI.Data
{
    [Serializable]
    public class ShopData
    {
        public List<ShopCategory> categories;
    }

    [Serializable]
    public class ShopCategory
    {
        public string id;
        public string name;
        public List<ShopItem> items;
    }

    [Serializable]
    public class ShopItem
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public string currency; // "coins" or "gems"
        public string type; // "skin", "weapon", "bundle"
        public string icon;
        public string rarity; // "common", "rare", "epic", "legendary"
        public string badge; // "sale", "new", "limited"
    }
}
