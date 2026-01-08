using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Logging;
using Modules.RemoteConfig;
using Modules.RemoteConfig.Models;
using Modules.Skins.Data;
using VContainer;

namespace Modules.Skins
{
    /// <summary>
    /// Skins service - manages character skins
    /// Loads from RemoteConfigService with fallback to local JSON
    /// </summary>
    public class SkinsService : ISkinsService, IDisposable
    {
        private const string SKINS_DATA_PATH = "Data/Skins";
        private const string EQUIPPED_SKINS_KEY = "EquippedSkins";

        private SkinsData _skinsData;
        private readonly Dictionary<string, SkinItem> _skinsById = new Dictionary<string, SkinItem>();
        private readonly Dictionary<int, List<SkinItem>> _skinsByCharacter = new Dictionary<int, List<SkinItem>>();
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IBankService _bankService;

        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;

        [Inject]
        public SkinsService(IRemoteConfigService remoteConfigService, IBankService bankService)
        {
            _remoteConfigService = remoteConfigService;
            _bankService = bankService;

            _bankService.OnSkinUnlocked += (id) => OnSkinUnlocked?.Invoke(id);
            _bankService.OnSkinEquipped += (charId, id) => OnSkinEquipped?.Invoke(charId, id);

            LoadSkinsData();
            EnsureDefaultSkinsUnlocked();
            SubscribeToConfigUpdates();
        }

        private void EnsureDefaultSkinsUnlocked()
        {
            if (_skinsData == null) return;
            foreach (var skin in _skinsData.skins)
            {
                if (skin.unlockedByDefault && !_bankService.IsSkinUnlocked(skin.id))
                {
                    _bankService.UnlockSkin(skin.id);
                }
            }
        }

        private void LoadSkinsData()
        {
            // Try to load from RemoteConfigService first
            if (_remoteConfigService != null && _remoteConfigService.TryGetConfig<SkinsConfig>(out var skinsConfig))
            {
                LoadFromRemoteConfig(skinsConfig);
                return;
            }
        }

        private void LoadFromRemoteConfig(SkinsConfig skinsConfig)
        {
            _skinsData = new SkinsData { skins = new List<SkinItem>() };

            // Convert SkinsConfig to SkinsData
            foreach (var skinDef in skinsConfig.Skins)
            {
                var skinItem = new SkinItem
                {
                    id = skinDef.SkinId,
                    name = skinDef.SkinName,
                    characterId = int.TryParse(skinDef.CharacterId, out int charId) ? charId : 0,
                    characterName = skinDef.CharacterId, // Store as string
                    rarity = skinDef.Rarity,
                    description = skinDef.Description,
                    unlockCost = skinDef.UnlockCost,
                    unlockType = skinDef.UnlockCurrency, // Map currency to unlock type
                    unlockedByDefault = skinDef.IsDefault,
                    prefabAddress = skinDef.PrefabAddress,
                    iconAddress = skinDef.IconAddress,
                    tags = new List<string>() // Empty tags list
                };

                // Add tags based on properties
                if (skinDef.IsDefault)
                {
                    skinItem.tags.Add("default");
                }
                if (skinDef.IsLimitedEdition)
                {
                    skinItem.tags.Add("limited");
                }
                if (!string.IsNullOrEmpty(skinDef.SeasonId))
                {
                    skinItem.tags.Add($"season_{skinDef.SeasonId}");
                }

                _skinsData.skins.Add(skinItem);
            }

            BuildLookupDictionaries();
        }

        private void BuildLookupDictionaries()
        {
            _skinsById.Clear();
            _skinsByCharacter.Clear();

            foreach (var skin in _skinsData.skins)
            {
                // Add to ID lookup
                _skinsById[skin.id] = skin;

                // Add to character lookup
                if (!_skinsByCharacter.ContainsKey(skin.characterId))
                {
                    _skinsByCharacter[skin.characterId] = new List<SkinItem>();
                }
                _skinsByCharacter[skin.characterId].Add(skin);
            }
        }

        private void SubscribeToConfigUpdates()
        {
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated += OnConfigUpdated;
                GameLogger.Log("Subscribed to SkinsConfig updates");
            }
        }
        private void OnConfigUpdated(IConfigModel obj)
        {
            if (obj is SkinsConfig skinsConfig)
            {
                LoadFromRemoteConfig(skinsConfig);
            }
        }

        private void OnConfigUpdated(Type configType, IConfigModel config)
        {
            if (configType == typeof(SkinsConfig) && config is SkinsConfig skinsConfig)
            {
                LoadFromRemoteConfig(skinsConfig);
            }
        }

        public List<SkinItem> GetAllSkins()
        {
            return _skinsData?.skins ?? new List<SkinItem>();
        }

        public List<SkinItem> GetSkinsForCharacter(int characterId)
        {
            if (_skinsByCharacter.TryGetValue(characterId, out var skins))
            {
                return skins;
            }

            GameLogger.LogWarning($"No skins found for character {characterId}");
            return new List<SkinItem>();
        }

        public SkinItem GetDefaultSkinForCharacter(int characterId)
        {
            var skins = GetSkinsForCharacter(characterId);
            return skins.FirstOrDefault(s => s.unlockedByDefault);
        }

        public SkinItem GetSkin(string skinId)
        {
            if (_skinsById.TryGetValue(skinId, out var skin))
            {
                return skin;
            }

            GameLogger.LogWarning($"Skin not found: {skinId}");
            return null;
        }

        public bool IsSkinUnlocked(string skinId)
        {
            return _bankService.IsSkinUnlocked(skinId);
        }

        public bool UnlockSkin(string skinId)
        {
            if (!_skinsById.ContainsKey(skinId))
            {
                GameLogger.LogError($"Cannot unlock unknown skin: {skinId}");
                return false;
            }

            return _bankService.UnlockSkin(skinId);
        }

        public SkinItem GetEquippedSkin(int characterId)
        {
            string skinId = _bankService.GetEquippedSkinId(characterId);
            if (!string.IsNullOrEmpty(skinId))
            {
                return GetSkin(skinId);
            }

            // Return default skin if none equipped
            return GetDefaultSkinForCharacter(characterId);
        }

        public bool EquipSkin(int characterId, string skinId)
        {
            // Validate skin exists
            var skin = GetSkin(skinId);
            if (skin == null)
            {
                GameLogger.LogError($"Cannot equip unknown skin: {skinId}");
                return false;
            }

            // Validate skin belongs to character
            if (skin.characterId != characterId)
            {
                GameLogger.LogError($"Skin {skinId} does not belong to character {characterId}");
                return false;
            }

            return _bankService.EquipSkin(characterId, skinId);
        }

        public void Dispose()
        {
            // Unsubscribe from config updates
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated -= OnConfigUpdated;
            }

            _skinsById.Clear();
            _skinsByCharacter.Clear();
        }
    }
}
