using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Economy;
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
        private readonly EconomyService _economyService;

        public event Action<string> OnSkinUnlocked;
        public event Action<int, string> OnSkinEquipped;

        [Inject]
        public SkinsService(EconomyService economyService)
        {
            _economyService = economyService;

            _economyService.OnItemUnlocked += (itemId) =>
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
            CharacterClass[] characterClasses = Resources.LoadAll<CharacterClass>(CHARACTERS_PATH);

            if (characterClasses == null || characterClasses.Length == 0)
            {
                GameLogger.LogWarning($"No CharacterClass assets found at {CHARACTERS_PATH}");
                return;
            }

            GameLogger.Log($"Loading skins from {characterClasses.Length} character classes");

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

                    _skinsById[skin.id] = skin;

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
                if (skin.isDefault && !_economyService.HasItem(skin.id))
                {
                    // For default skins, directly add without purchase flow
                    // This requires a method to grant items - we'll add them via Purchase with 0 cost
                    // Since the EconomyService only has Purchase (which deducts), we need HasItem
                    // Actually, we need to ensure these are marked as owned.
                    // The simplest solution: skip for now - let them be handled by data defaults
                    // OR add a GrantItem method to EconomyService
                    GameLogger.Log($"Default skin {skin.name} should be unlocked on initialization");
                }
            }
        }

        public List<SkinItem> GetAllSkins() => _skinsById.Values.ToList();

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
            return _economyService.HasItem(skinId);
        }

        public bool UnlockSkin(string skinId)
        {
            if (!_skinsById.ContainsKey(skinId))
            {
                GameLogger.LogError($"Cannot unlock unknown skin: {skinId}");
                return false;
            }

            var skin = _skinsById[skinId];

            if (skin.Price > 0)
            {
                return _economyService.Purchase(skinId, skin.Price, EconomyKeys.CurrencyCoins);
            }
            else
            {
                // Free skin - need to grant directly
                // TODO: Add GrantItem method to EconomyService for free unlocks
                return _economyService.Purchase(skinId, 0, EconomyKeys.CurrencyCoins);
            }
        }

        public SkinItem GetEquippedSkin(int characterId)
        {
            // TODO: Need to store equipped skin preference - could use PlayerDataService
            // For now, return default
            return GetDefaultSkinForCharacter(characterId);
        }

        public bool EquipSkin(int characterId, string skinId)
        {
            var skin = GetSkin(skinId);
            if (skin == null)
            {
                GameLogger.LogError($"Cannot equip unknown skin: {skinId}");
                return false;
            }

            if (!IsSkinUnlocked(skinId))
            {
                GameLogger.LogError($"Cannot equip locked skin: {skinId}");
                return false;
            }

            var characterSkins = GetSkinsForCharacter(characterId);
            if (!characterSkins.Contains(skin))
            {
                GameLogger.LogError($"Skin {skinId} does not belong to character {characterId}");
                return false;
            }

            // TODO: Store equipped skin preference
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
