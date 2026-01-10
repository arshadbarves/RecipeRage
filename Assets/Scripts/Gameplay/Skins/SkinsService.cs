using System;
using System.Collections.Generic;
using System.Linq;
using Core.Banking.Interfaces;
using Core.Logging;
using Gameplay.Characters;
using Gameplay.Skins.Data;
using UnityEngine;
using VContainer;

namespace Gameplay.Skins
{
    /// <summary>
    /// Skins service - manages character skins
    /// Loads skins from CharacterClass ScriptableObjects
    /// </summary>
    public class SkinsService : ISkinsService, IDisposable
    {
        private const string EQUIPPED_SKIN_PREFIX = "EquippedSkin_Character_";
        private const string CHARACTERS_PATH = "ScriptableObjects/CharacterClasses";

        private readonly Dictionary<string, SkinItem> _skinsById = new Dictionary<string, SkinItem>();
        private readonly Dictionary<int, List<SkinItem>> _skinsByCharacter = new Dictionary<int, List<SkinItem>>();
        private readonly IBankService _bankService;

        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;

        [Inject]
        public SkinsService(IBankService bankService)
        {
            _bankService = bankService;

            // Subscribe to item unlocked events (skins are items in the new system)
            _bankService.OnItemUnlocked += (itemId) =>
            {
                if (_skinsById.ContainsKey(itemId))
                {
                    OnSkinUnlocked?.Invoke(itemId);
                }
            };

            LoadSkinsFromCharacterClasses();
            EnsureDefaultSkinsUnlocked();
        }

        private void LoadSkinsFromCharacterClasses()
        {
            // Load all CharacterClass assets from Resources
            CharacterClass[] characterClasses = Resources.LoadAll<CharacterClass>(CHARACTERS_PATH);

            if (characterClasses == null || characterClasses.Length == 0)
            {
                GameLogger.LogWarning($"No CharacterClass assets found at {CHARACTERS_PATH}");
                return;
            }

            GameLogger.Log($"Loading skins from {characterClasses.Length} character classes");

            // Build lookup dictionaries from character skins
            foreach (var character in characterClasses)
            {
                if (character.Skins == null || character.Skins.Count == 0)
                {
                    GameLogger.LogWarning($"Character {character.DisplayName} has no skins");
                    continue;
                }

                foreach (var skin in character.Skins)
                {
                    if (string.IsNullOrEmpty(skin.id))
                    {
                        GameLogger.LogWarning($"Skin in {character.DisplayName} has no ID, skipping");
                        continue;
                    }

                    // Add to ID lookup
                    _skinsById[skin.id] = skin;

                    // Add to character lookup
                    if (!_skinsByCharacter.ContainsKey(character.Id))
                    {
                        _skinsByCharacter[character.Id] = new List<SkinItem>();
                    }

                    _skinsByCharacter[character.Id].Add(skin);
                }

                GameLogger.Log($"Loaded {character.Skins.Count} skins for {character.DisplayName}");
            }

            GameLogger.Log($"Total skins loaded: {_skinsById.Count}");
        }

        private void EnsureDefaultSkinsUnlocked()
        {
            foreach (var skin in _skinsById.Values)
            {
                if (skin.isDefault && !_bankService.HasItem(skin.id))
                {
                    _bankService.AddItem(skin.id);
                    GameLogger.Log($"Unlocked default skin: {skin.name}");
                }
            }
        }

        public List<SkinItem> GetAllSkins()
        {
            return _skinsById.Values.ToList();
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
            return skins.FirstOrDefault(s => s.isDefault);
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
            // Use generic HasItem instead of skin-specific method
            return _bankService.HasItem(skinId);
        }

        public bool UnlockSkin(string skinId)
        {
            if (!_skinsById.ContainsKey(skinId))
            {
                GameLogger.LogError($"Cannot unlock unknown skin: {skinId}");
                return false;
            }

            var skin = _skinsById[skinId];

            // Use the Purchase method for paid skins, or AddItem for free unlocks
            if (skin.Price > 0)
            {
                return _bankService.Purchase(skinId, skin.Price, "coins");
            }
            else
            {
                _bankService.AddItem(skinId);
                return true;
            }
        }

        public SkinItem GetEquippedSkin(int characterId)
        {
            // Use generic data storage for equipped skin
            string key = EQUIPPED_SKIN_PREFIX + characterId;
            string skinId = _bankService.GetData(key);

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

            // Check if skin is unlocked
            if (!IsSkinUnlocked(skinId))
            {
                GameLogger.LogError($"Cannot equip locked skin: {skinId}");
                return false;
            }

            // Verify skin belongs to this character
            var characterSkins = GetSkinsForCharacter(characterId);
            if (!characterSkins.Contains(skin))
            {
                GameLogger.LogError($"Skin {skinId} does not belong to character {characterId}");
                return false;
            }

            // Save equipped skin using generic data storage
            string key = EQUIPPED_SKIN_PREFIX + characterId;
            _bankService.SetData(key, skinId);

            OnSkinEquipped?.Invoke(characterId, skinId);
            return true;
        }

        public void Dispose()
        {
            _skinsById.Clear();
            _skinsByCharacter.Clear();
        }
    }
}
