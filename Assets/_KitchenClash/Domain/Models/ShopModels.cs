using System;
using System.Collections.Generic;

namespace KitchenClash.Domain
{
    [Serializable]
    public class ShopData
    {
        public List<ShopCategory> categories = new();
    }

    [Serializable]
    public class ShopCategory
    {
        public string id;
        public string name;
        public List<ShopItem> items = new();
    }

    [Serializable]
    public class ShopItem
    {
        public string id;
        public string name;
        public string description;
        public int price;
        public string currency;
        public string rarity;
        public string badge;
        public string category;
    }
}
