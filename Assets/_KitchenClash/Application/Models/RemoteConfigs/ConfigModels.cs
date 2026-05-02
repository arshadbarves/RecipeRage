using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Models.RemoteConfigs
{
    /// <summary>
    /// Application-layer config models for remote configuration.
    /// These are plain POCOs with no infrastructure dependencies.
    /// </summary>

    [Serializable]
    public class CharacterConfig : IConfigModel
    {
        /// <summary>Per-chef stat overrides keyed by ChefId name (e.g. "Rosa").</summary>
        public List<CharacterStatOverride> Overrides = new();

        public bool IsValid() => true;
    }

    [Serializable]
    public class CharacterStatOverride
    {
        public string ChefId;
        public float SpeedMultiplier = -1f;
        public float CookingSpeedMultiplier = -1f;
        public float FireResistance = -1f;
        public int CarryCapacity = -1;
        public float ScoreMultiplier = -1f;
        public float InteractRange = -1f;
    }

    [Serializable]
    public class MapConfig : IConfigModel
    {
        public List<MapOverride> Overrides = new();

        public bool IsValid() => true;
    }

    [Serializable]
    public class MapOverride
    {
        public string MapId;
        public float FireChanceMultiplier = -1f;
        public bool? HasSpecialHazards;
    }

    [Serializable]
    public class SkinsConfig : IConfigModel
    {
        public List<SkinPriceOverride> PriceOverrides = new();

        public bool IsValid() => true;
    }

    [Serializable]
    public class SkinPriceOverride
    {
        public string SkinId;
        public int Price = -1;
        public string Badge;
    }

    [Serializable]
    public class MaintenanceConfig : IConfigModel
    {
        public bool IsEnabled;
        public string Message = "We are currently performing maintenance. Please try again later.";
        public string EstimatedEndTimeUtc = "";
        public bool AllowRetry = true;

        public bool IsValid() => true;
    }

    [Serializable]
    public class ForceUpdateConfig : IConfigModel
    {
        public string MinimumVersion = "1.0.0";
        public string UpdateUrl = "";
        public string UpdateMessage = "A new version is available. Please update to continue.";

        public bool IsValid() => !string.IsNullOrEmpty(MinimumVersion);
    }

    [Serializable]
    public class ShopConfig : IConfigModel
    {
        public List<ShopItemOverride> ItemOverrides = new();

        public bool IsValid() => true;
    }

    [Serializable]
    public class ShopItemOverride
    {
        public string ItemId;
        public int Price = -1;
        public string Badge;
        public bool? Enabled;
    }
}
