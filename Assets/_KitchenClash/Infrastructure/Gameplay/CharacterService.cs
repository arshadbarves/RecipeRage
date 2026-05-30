using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class CharacterService : ICharacterService, IDisposable
    {
        private const string CharactersPath = "ScriptableObjects/CharacterClasses";

        // ── New GDD v3 ──
        private readonly ChefRegistry _registry;
        private readonly IPlayerDataService _playerData;
        private readonly IEconomyService _economy;
        private ChefDefinition _selectedChef;

        // ── Legacy SO ──
        private readonly Dictionary<int, CharacterClass> _characters = new();
        private readonly HashSet<int> _unlockedCharacters = new();
        private CharacterClass _selectedCharacter;

        // ── Events (new) ──
        public event Action<ChefDefinition> OnChefSelected;
        public event Action<ChefId> OnChefUnlocked;

        // ── Events (legacy) ──
        public event Action<CharacterClass> OnCharacterSelected;
        public event Action<int> OnCharacterUnlocked;

        // ── Properties ──
        public ChefDefinition SelectedChef => _selectedChef;
        public CharacterClass SelectedCharacter => _selectedCharacter;

        public CharacterService(ChefRegistry registry, IPlayerDataService playerData, IEconomyService economy)
        {
            _registry = registry;
            _playerData = playerData;
            _economy = economy;

            // Default to Rosa (starter)
            _selectedChef = _registry.Get(ChefId.Rosa);

            // Load legacy SO characters for backward-compat UI
            LoadLegacyCharacters();

            GameLogger.Log($"CharacterService initialized. Default chef: {_selectedChef?.DisplayName}");
        }

        // ══════════════════════════════════════════
        //  New GDD v3 API
        // ══════════════════════════════════════════

        public IReadOnlyList<ChefDefinition> GetAllChefs() => _registry.GetAll();

        public IReadOnlyList<ChefDefinition> GetUnlockedChefs()
        {
            int playerLevel = _playerData?.GetProgress()?.HighestLevel ?? 0;
            return _registry.GetUnlockedChefs(playerLevel, _economy);
        }

        public bool IsUnlocked(ChefId chefId)
        {
            var chef = _registry.Get(chefId);
            if (chef == null) return false;
            int playerLevel = _playerData?.GetProgress()?.HighestLevel ?? 0;
            return _registry.IsUnlocked(chef, playerLevel, _economy);
        }

        public bool SelectChef(ChefId chefId)
        {
            if (!IsUnlocked(chefId)) return false;
            var chef = _registry.Get(chefId);
            if (chef == null) return false;
            if (_selectedChef?.Id == chef.Id) return true;

            _selectedChef = chef;
            OnChefSelected?.Invoke(chef);

            // Sync legacy selection
            SyncLegacySelection(chef);

            GameLogger.Log($"Chef selected: {chef.DisplayName}");
            return true;
        }

        public bool TryPurchaseChef(ChefId chefId)
        {
            var chef = _registry.Get(chefId);
            if (chef == null || chef.Unlock.Type != UnlockType.Shop) return false;
            if (IsUnlocked(chefId)) return true;

            string itemId = $"chef_{chefId}";
            bool purchased = _economy.Purchase(itemId, chef.Unlock.Value, "coins");
            if (purchased)
            {
                OnChefUnlocked?.Invoke(chefId);
                GameLogger.Log($"Chef purchased: {chef.DisplayName} for {chef.Unlock.Value} coins");
            }
            return purchased;
        }

        // ══════════════════════════════════════════
        //  Legacy SO API (backward compat)
        // ══════════════════════════════════════════

        private void LoadLegacyCharacters()
        {
            CharacterClass[] characters = Resources.LoadAll<CharacterClass>(CharactersPath);
            foreach (var character in characters)
            {
                if (character != null)
                {
                    _characters[character.Id] = character;
                    if (character.UnlockData != null && character.UnlockData.UnlockedByDefault)
                    {
                        _unlockedCharacters.Add(character.Id);
                    }
                }
            }

            if (_unlockedCharacters.Count > 0)
            {
                int firstUnlocked = _unlockedCharacters.First();
                _selectedCharacter = _characters[firstUnlocked];
            }

            GameLogger.Log($"Legacy: loaded {_characters.Count} SO characters, {_unlockedCharacters.Count} unlocked");
        }

        public CharacterClass[] GetAvailableCharacters() => _characters.Values.ToArray();

        public CharacterClass[] GetUnlockedCharacters()
        {
            return _unlockedCharacters
                .Where(id => _characters.ContainsKey(id))
                .Select(id => _characters[id])
                .ToArray();
        }

        public CharacterClass GetCharacter(int id) => _characters.TryGetValue(id, out var c) ? c : null;
        public bool IsUnlocked(int characterId) => _unlockedCharacters.Contains(characterId);

        public bool Unlock(int characterId)
        {
            if (!_characters.ContainsKey(characterId)) return false;
            if (_unlockedCharacters.Contains(characterId)) return true;
            _unlockedCharacters.Add(characterId);
            OnCharacterUnlocked?.Invoke(characterId);
            return true;
        }

        public bool SelectCharacter(int characterId)
        {
            if (!IsUnlocked(characterId)) return false;
            var character = GetCharacter(characterId);
            if (character == null) return false;
            if (_selectedCharacter == character) return true;
            _selectedCharacter = character;
            OnCharacterSelected?.Invoke(character);
            return true;
        }

        private void SyncLegacySelection(ChefDefinition chef)
        {
            // Try to find matching SO by display name
            foreach (var kvp in _characters)
            {
                if (kvp.Value.DisplayName == chef.DisplayName)
                {
                    _selectedCharacter = kvp.Value;
                    OnCharacterSelected?.Invoke(kvp.Value);
                    return;
                }
            }
        }

        public void Dispose()
        {
            _characters.Clear();
            _unlockedCharacters.Clear();
            _selectedCharacter = null;
            _selectedChef = null;
        }
    }
}
