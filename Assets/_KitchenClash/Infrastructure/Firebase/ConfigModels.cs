using System;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Firebase
{
    /// <summary>
    /// Stub config models for Firebase Remote Config deserialization.
    /// TODO: Move to shared config location once schemas are finalized.
    /// </summary>

    [Serializable]
    public class CharacterConfig : IConfigModel
    {
        public bool IsValid() => true;
    }

    [Serializable]
    public class MapConfig : IConfigModel
    {
        public bool IsValid() => true;
    }

    [Serializable]
    public class SkinsConfig : IConfigModel
    {
        public bool IsValid() => true;
    }

    [Serializable]
    public class MaintenanceConfig : IConfigModel
    {
        public bool IsValid() => true;
    }

    [Serializable]
    public class ForceUpdateConfig : IConfigModel
    {
        public bool IsValid() => true;
    }

    [Serializable]
    public class ShopConfig : IConfigModel
    {
        public bool IsValid() => true;
    }
}
