using System;
using System.Collections.Generic;
using Core.RemoteConfig.Interfaces;
using Newtonsoft.Json;

namespace Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for character skins
    /// Matches existing Skins.json structure for backward compatibility
    /// </summary>
    [Serializable]
    public class SkinsConfig : IConfigModel
    {
        [JsonProperty("skins")]
        public List<SkinDefinition> Skins { get; set; }

        public SkinsConfig()
        {
            Skins = new List<SkinDefinition>();
        }

        public bool Validate()
        {
            if (Skins == null || Skins.Count == 0)
            {
                return false;
            }

            foreach (var skin in Skins)
            {
                if (string.IsNullOrEmpty(skin.SkinId) || string.IsNullOrEmpty(skin.CharacterId))
                {
                    return false;
                }

                if (skin.UnlockCost < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class SkinDefinition
    {
        [JsonProperty("skinId")]
        public string SkinId { get; set; }

        [JsonProperty("skinName")]
        public string SkinName { get; set; }

        [JsonProperty("characterId")]
        public string CharacterId { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        // Unlock Requirements
        [JsonProperty("unlockCost")]
        public int UnlockCost { get; set; }

        [JsonProperty("unlockCurrency")]
        public string UnlockCurrency { get; set; }

        [JsonProperty("unlockLevel")]
        public int UnlockLevel { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty("isLimitedEdition")]
        public bool IsLimitedEdition { get; set; }

        [JsonProperty("seasonId")]
        public string SeasonId { get; set; }

        // Asset References
        [JsonProperty("prefabAddress")]
        public string PrefabAddress { get; set; }

        [JsonProperty("iconAddress")]
        public string IconAddress { get; set; }

        [JsonProperty("thumbnailAddress")]
        public string ThumbnailAddress { get; set; }

        public SkinDefinition()
        {
            UnlockCurrency = "Coins";
            UnlockLevel = 1;
            IsDefault = false;
            IsLimitedEdition = false;
        }
    }
}
