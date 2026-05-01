using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Domain;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Infrastructure.Logging;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class CharacterService : ICharacterService, IDisposable
    {
        private const string CharactersPath = "ScriptableObjects/CharacterClasses";
        private readonly Dictionary<int, CharacterClass> _characters = new Dictionary<int, CharacterClass>();
        private readonly HashSet<int> _unlockedCharacters = new HashSet<int>();
        private CharacterClass _selectedCharacter;

        public event Action<CharacterClass> OnCharacterSelected;
        public event Action<int> OnCharacterUnlocked;

        public CharacterClass SelectedCharacter => _selectedCharacter;

        public CharacterService()
        {
            LoadCharacters();
        }

        private void LoadCharacters()
        {
            CharacterClass[] characters = Resources.LoadAll<CharacterClass>(CharactersPath);

            foreach (var character in characters)
            {
                if (character != null)
                {
                    _characters[character.Id] = character;
                    if (character.UnlockData.UnlockedByDefault)
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

            GameLogger.Log($"Loaded {_characters.Count} characters, {_unlockedCharacters.Count} unlocked");
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

        public void Dispose()
        {
            _characters.Clear();
            _unlockedCharacters.Clear();
            _selectedCharacter = null;
        }
    }

    // ICharacterService is defined in Domain/Interfaces/ICharacterService.cs
}
