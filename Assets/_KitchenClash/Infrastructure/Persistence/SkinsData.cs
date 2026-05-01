using System;
using UnityEngine;

namespace KitchenClash.Infrastructure.Persistence
{
    [Serializable]
    public class SkinItem
    {
        public string id;
        public string name;
        public Sprite icon;
        public GameObject prefab;
        public SkinRarity rarity = SkinRarity.Common;
        public bool isDefault;
        public int priceOverride = -1;

        public int Price => priceOverride >= 0 ? priceOverride : GetPriceForRarity(rarity);

        private static int GetPriceForRarity(SkinRarity rarity) => rarity switch
        {
            SkinRarity.Common => 100,
            SkinRarity.Rare => 500,
            SkinRarity.Epic => 1500,
            SkinRarity.Legendary => 5000,
            _ => 0
        };
    }

    public enum SkinRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
