using System;
using System.Collections.Generic;
using System.Linq;
using Core.Bootstrap;
using Core.Logging;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Core.Skins.Data;
using UnityEngine;
using VContainer;

namespace Core.Skins
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
        private readonly HashSet<string> _unlockedSkins = new HashSet<string>();
        private readonly Dictionary<int, string> _equippedSkins = new Dictionary<int, string>();
        private readonly IRemoteConfigService _remoteConfigService;

        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;

        [Inject]
        public SkinsService(IRemoteConfigService remoteConfigService)
        {
            _remoteConfigService = remoteConfigService;
            LoadPlayerProgress();
            SubscribeToConfigUpdates();
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

                // Auto-unlock default skins
                if (skin.unlockedByDefault)
                {
                    _unlockedSkins.Add(skin.id);
                }
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

        private void LoadPlayerProgress()
        {
            // Load unlocked skins from PlayerPrefs
            string unlockedData = PlayerPrefs.GetString("UnlockedSkins", "");
            if (!string.IsNullOrEmpty(unlockedData))
            {
                var unlockedIds = unlockedData.Split(',');
                foreach (var id in unlockedIds)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _unlockedSkins.Add(id);
                    }
                }
            }

            // Load equipped skins from PlayerPrefs
            string equippedData = PlayerPrefs.GetString(EQUIPPED_SKINS_KEY, "");
            if (!string.IsNullOrEmpty(equippedData))
            {
                var pairs = equippedData.Split(';');
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int characterId))
                    {
                        _equippedSkins[characterId] = parts[1];
                    }
                }
            }

            // Ensure each character has a default skin equipped
            foreach (var characterId in _skinsByCharacter.Keys)
            {
                if (!_equippedSkins.ContainsKey(characterId))
                {
                    var defaultSkin = GetDefaultSkinForCharacter(characterId);
                    if (defaultSkin != null)
                    {
                        _equippedSkins[characterId] = defaultSkin.id;
                    }
                }
            }

            SavePlayerProgress();
        }

        private void SavePlayerProgress()
        {
            // Save unlocked skins
            string unlockedData = string.Join(",", _unlockedSkins);
            PlayerPrefs.SetString("UnlockedSkins", unlockedData);

            // Save equipped skins
            var equippedPairs = _equippedSkins.Select(kvp => $"{kvp.Key}:{kvp.Value}");
            string equippedData = string.Join(";", equippedPairs);
            PlayerPrefs.SetString(EQUIPPED_SKINS_KEY, equippedData);

            PlayerPrefs.Save();
        }

        public List<SkinItem> GetAllSkins()
        {
            return _skinsData.skins;
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
            return _unlockedSkins.Contains(skinId);
        }

        public bool UnlockSkin(string skinId)
        {
            if (!_skinsById.ContainsKey(skinId))
            {
                GameLogger.LogError($"Cannot unlock unknown skin: {skinId}");
                return false;
            }

            if (_unlockedSkins.Contains(skinId))
            {
                GameLogger.Log($"Skin already unlocked: {skinId}");
                return true;
            }

            _unlockedSkins.Add(skinId);
            SavePlayerProgress();

            OnSkinUnlocked?.Invoke(skinId);
            GameLogger.Log($"Unlocked skin: {skinId}");

            return true;
        }

        public SkinItem GetEquippedSkin(int characterId)
        {
            if (_equippedSkins.TryGetValue(characterId, out var skinId))
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

            // Validate skin is unlocked
            if (!IsSkinUnlocked(skinId))
            {
                GameLogger.LogError($"Cannot equip locked skin: {skinId}");
                return false;
            }

            // Equip skin
            _equippedSkins[characterId] = skinId;
            SavePlayerProgress();

            OnSkinEquipped?.Invoke(characterId, skinId);
            GameLogger.Log($"Equipped skin {skinId} for character {characterId}");

            return true;
        }

        public void Dispose()
        {
            // Unsubscribe from config updates
            if (_remoteConfigService != null)
            {
                _remoteConfigService.OnSpecificConfigUpdated -= OnConfigUpdated;
            }

            SavePlayerProgress();
            _skinsById.Clear();
            _skinsByCharacter.Clear();
            _unlockedSkins.Clear();
            _equippedSkins.Clear();
        }
    }
}
