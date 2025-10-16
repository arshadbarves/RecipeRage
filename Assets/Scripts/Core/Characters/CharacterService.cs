using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Character service - pure C# class, no MonoBehaviour
    /// </summary>
    public class CharacterService : ICharacterService
    {
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
            // Load from Resources
            CharacterClass[] characters = Resources.LoadAll<CharacterClass>("Characters");
            
            foreach (var character in characters)
            {
                if (character != null)
                {
                    _characters[character.Id] = character;
                    
                    if (character.UnlockedByDefault)
                    {
                        _unlockedCharacters.Add(character.Id);
                    }
                }
            }

            // Set default
            if (_unlockedCharacters.Count > 0)
            {
                int firstUnlocked = _unlockedCharacters.First();
                _selectedCharacter = _characters[firstUnlocked];
            }

            Debug.Log($"[CharacterService] Loaded {_characters.Count} characters, {_unlockedCharacters.Count} unlocked");
        }

        public CharacterClass[] GetAvailableCharacters()
        {
            return _characters.Values.ToArray();
        }

        public CharacterClass[] GetUnlockedCharacters()
        {
            return _unlockedCharacters
                .Where(id => _characters.ContainsKey(id))
                .Select(id => _characters[id])
                .ToArray();
        }

        public CharacterClass GetCharacter(int id)
        {
            return _characters.TryGetValue(id, out var character) ? character : null;
        }

        public bool IsUnlocked(int characterId)
        {
            return _unlockedCharacters.Contains(characterId);
        }

        public bool Unlock(int characterId)
        {
            if (!_characters.ContainsKey(characterId)) return false;
            if (_unlockedCharacters.Contains(characterId)) return true;

            _unlockedCharacters.Add(characterId);
            OnCharacterUnlocked?.Invoke(characterId);
            
            Debug.Log($"[CharacterService] Unlocked: {_characters[characterId].DisplayName}");
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
            
            Debug.Log($"[CharacterService] Selected: {character.DisplayName}");
            return true;
        }
    }
}
