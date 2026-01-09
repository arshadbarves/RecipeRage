using System;
using UnityEngine;

namespace Gameplay.Skins.Data
{
    /// <summary>
    /// Skin item data - now embedded in CharacterClass
    /// </summary>
    [Serializable]
    public class SkinItem
    {
        [Tooltip("Unique identifier for this skin")]
        public string id;

        [Tooltip("Display name of the skin")]
        public string name;

        [Tooltip("Character prefab for rendering (replaces CharacterClass.CharacterPrefab)")]
        public GameObject prefab;

        [Tooltip("Rarity tier - determines base price")]
        public SkinRarity rarity = SkinRarity.Common;

        [Tooltip("Is this the default skin? (unlocked by default)")]
        public bool isDefault;

        [Tooltip("Optional price override. If >= 0, uses this instead of rarity price. 0 = free, -1 = use rarity")]
        public int priceOverride = -1;

        /// <summary>
        /// Computed price - uses override if set (>=0), otherwise rarity-based price
        /// </summary>
        public int Price => priceOverride >= 0 ? priceOverride : GetPriceForRarity(rarity);

        private static int GetPriceForRarity(SkinRarity rarity)
        {
            return rarity switch
            {
                SkinRarity.Common => 100,
                SkinRarity.Rare => 500,
                SkinRarity.Epic => 1500,
                SkinRarity.Legendary => 5000,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Skin rarity tiers
    /// </summary>
    public enum SkinRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
