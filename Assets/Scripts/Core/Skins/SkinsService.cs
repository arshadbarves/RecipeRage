using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using UI.Data;
using UnityEngine;

namespace Core.Skins
{
    /// <summary>
    /// Skins service - manages character skins
    /// Loads from cloud (future) or local JSON
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
        
        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;
        
        public SkinsService()
        {
            LoadSkinsData();
            LoadPlayerProgress();
        }
        
        private void LoadSkinsData()
        {
            // Load from Resources (will be replaced with cloud loading)
            TextAsset jsonFile = Resources.Load<TextAsset>(SKINS_DATA_PATH);
            
            if (jsonFile == null)
            {
                GameLogger.LogError($"Failed to load skins data from {SKINS_DATA_PATH}");
                _skinsData = new SkinsData { skins = new List<SkinItem>() };
                return;
            }
            
            _skinsData = JsonUtility.FromJson<SkinsData>(jsonFile.text);
            
            if (_skinsData == null || _skinsData.skins == null)
            {
                GameLogger.LogError("Failed to parse skins data");
                _skinsData = new SkinsData { skins = new List<SkinItem>() };
                return;
            }
            
            // Build lookup dictionaries
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
            
            GameLogger.Log($"Loaded {_skinsData.skins.Count} skins for {_skinsByCharacter.Count} characters");
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
            SavePlayerProgress();
            _skinsById.Clear();
            _skinsByCharacter.Clear();
            _unlockedSkins.Clear();
            _equippedSkins.Clear();
            GameLogger.Log("SkinsService disposed");
        }
    }
}
