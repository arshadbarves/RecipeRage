using System;
using System.Collections.Generic;
using Core.Utilities.Patterns;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// Manages character classes and abilities in RecipeRage.
    /// </summary>
    public class CharacterManager : MonoBehaviourSingleton<CharacterManager>
    {
        [Header("Character Settings")]
        [SerializeField] private List<CharacterClass> _availableCharacterClasses = new List<CharacterClass>();
        [SerializeField] private int _defaultCharacterClassId = 0;

        /// <summary>
        /// Event triggered when the selected character class changes.
        /// </summary>
        public event Action<CharacterClass> OnCharacterClassChanged;

        /// <summary>
        /// The currently selected character class.
        /// </summary>
        public CharacterClass SelectedCharacterClass { get; private set; }

        /// <summary>
        /// Dictionary of available character classes by ID.
        /// </summary>
        private Dictionary<int, CharacterClass> _characterClassDict = new Dictionary<int, CharacterClass>();

        /// <summary>
        /// Dictionary of unlocked character classes.
        /// </summary>
        private HashSet<int> _unlockedCharacterClasses = new HashSet<int>();

        /// <summary>
        /// Initialize the character manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Build the character class dictionary
            foreach (CharacterClass characterClass in _availableCharacterClasses)
            {
                if (characterClass != null)
                {
                    _characterClassDict[characterClass.Id] = characterClass;

                    // Add to unlocked if unlocked by default
                    if (characterClass.UnlockedByDefault)
                    {
                        _unlockedCharacterClasses.Add(characterClass.Id);
                    }
                }
            }

            // Set the default character class
            if (_characterClassDict.ContainsKey(_defaultCharacterClassId))
            {
                SelectedCharacterClass = _characterClassDict[_defaultCharacterClassId];
            }
            else if (_availableCharacterClasses.Count > 0)
            {
                SelectedCharacterClass = _availableCharacterClasses[0];
                _defaultCharacterClassId = SelectedCharacterClass.Id;
            }

            // Register the character manager with the service locator
            ServiceLocator.Instance.Register<CharacterManager>(this);

            Debug.Log($"[CharacterManager] Initialized with {_availableCharacterClasses.Count} character classes, default: {_defaultCharacterClassId}");
        }

        /// <summary>
        /// Get a character class by ID.
        /// </summary>
        /// <param name="characterClassId">The character class ID</param>
        /// <returns>The character class, or null if not found</returns>
        public CharacterClass GetCharacterClass(int characterClassId)
        {
            if (!_characterClassDict.ContainsKey(characterClassId))
            {
                Debug.LogWarning($"[CharacterManager] Character class not found: {characterClassId}");
                return null;
            }

            return _characterClassDict[characterClassId];
        }

        /// <summary>
        /// Get all available character classes.
        /// </summary>
        /// <returns>List of available character classes</returns>
        public List<CharacterClass> GetAllCharacterClasses()
        {
            return new List<CharacterClass>(_availableCharacterClasses);
        }

        /// <summary>
        /// Get all unlocked character classes.
        /// </summary>
        /// <returns>List of unlocked character classes</returns>
        public List<CharacterClass> GetUnlockedCharacterClasses()
        {
            var unlockedClasses = new List<CharacterClass>();

            foreach (int characterClassId in _unlockedCharacterClasses)
            {
                if (_characterClassDict.ContainsKey(characterClassId))
                {
                    unlockedClasses.Add(_characterClassDict[characterClassId]);
                }
            }

            return unlockedClasses;
        }

        /// <summary>
        /// Check if a character class is unlocked.
        /// </summary>
        /// <param name="characterClassId">The character class ID</param>
        /// <returns>True if the character class is unlocked</returns>
        public bool IsCharacterClassUnlocked(int characterClassId)
        {
            return _unlockedCharacterClasses.Contains(characterClassId);
        }

        /// <summary>
        /// Unlock a character class.
        /// </summary>
        /// <param name="characterClassId">The character class ID</param>
        /// <returns>True if the character class was unlocked</returns>
        public bool UnlockCharacterClass(int characterClassId)
        {
            if (!_characterClassDict.ContainsKey(characterClassId))
            {
                Debug.LogWarning($"[CharacterManager] Cannot unlock character class: not found: {characterClassId}");
                return false;
            }

            if (_unlockedCharacterClasses.Contains(characterClassId))
            {
                Debug.Log($"[CharacterManager] Character class already unlocked: {characterClassId}");
                return true;
            }

            _unlockedCharacterClasses.Add(characterClassId);

            Debug.Log($"[CharacterManager] Unlocked character class: {_characterClassDict[characterClassId].DisplayName} ({characterClassId})");

            return true;
        }

        /// <summary>
        /// Select a character class by ID.
        /// </summary>
        /// <param name="characterClassId">The character class ID</param>
        /// <returns>True if the character class was selected</returns>
        public bool SelectCharacterClass(int characterClassId)
        {
            CharacterClass characterClass = GetCharacterClass(characterClassId);
            if (characterClass == null)
            {
                return false;
            }

            return SelectCharacterClass(characterClass);
        }

        /// <summary>
        /// Select a character class.
        /// </summary>
        /// <param name="characterClass">The character class to select</param>
        /// <returns>True if the character class was selected</returns>
        public bool SelectCharacterClass(CharacterClass characterClass)
        {
            if (characterClass == null)
            {
                Debug.LogError("[CharacterManager] Cannot select null character class");
                return false;
            }

            if (!_unlockedCharacterClasses.Contains(characterClass.Id))
            {
                Debug.LogError($"[CharacterManager] Cannot select locked character class: {characterClass.DisplayName} ({characterClass.Id})");
                return false;
            }

            if (SelectedCharacterClass == characterClass)
            {
                return true; // Already selected
            }

            Debug.Log($"[CharacterManager] Selecting character class: {characterClass.DisplayName} ({characterClass.Id})");

            SelectedCharacterClass = characterClass;
            OnCharacterClassChanged?.Invoke(SelectedCharacterClass);

            return true;
        }

        /// <summary>
        /// Add a character class to the available character classes.
        /// </summary>
        /// <param name="characterClass">The character class to add</param>
        /// <returns>True if the character class was added</returns>
        public bool AddCharacterClass(CharacterClass characterClass)
        {
            if (characterClass == null)
            {
                Debug.LogError("[CharacterManager] Cannot add null character class");
                return false;
            }

            if (_characterClassDict.ContainsKey(characterClass.Id))
            {
                Debug.LogWarning($"[CharacterManager] Character class already exists: {characterClass.Id}");
                return false;
            }

            _availableCharacterClasses.Add(characterClass);
            _characterClassDict[characterClass.Id] = characterClass;

            // Add to unlocked if unlocked by default
            if (characterClass.UnlockedByDefault)
            {
                _unlockedCharacterClasses.Add(characterClass.Id);
            }

            Debug.Log($"[CharacterManager] Added character class: {characterClass.DisplayName} ({characterClass.Id})");

            return true;
        }

        /// <summary>
        /// Remove a character class from the available character classes.
        /// </summary>
        /// <param name="characterClassId">The character class ID to remove</param>
        /// <returns>True if the character class was removed</returns>
        public bool RemoveCharacterClass(int characterClassId)
        {
            if (!_characterClassDict.ContainsKey(characterClassId))
            {
                Debug.LogWarning($"[CharacterManager] Character class not found: {characterClassId}");
                return false;
            }

            CharacterClass characterClass = _characterClassDict[characterClassId];
            _availableCharacterClasses.Remove(characterClass);
            _characterClassDict.Remove(characterClassId);
            _unlockedCharacterClasses.Remove(characterClassId);

            // If we removed the selected character class, select a new one
            if (SelectedCharacterClass == characterClass)
            {
                if (_availableCharacterClasses.Count > 0)
                {
                    SelectCharacterClass(_availableCharacterClasses[0]);
                }
                else
                {
                    SelectedCharacterClass = null;
                }
            }

            Debug.Log($"[CharacterManager] Removed character class: {characterClass.DisplayName} ({characterClass.Id})");

            return true;
        }

        /// <summary>
        /// Create an ability for a player controller.
        /// </summary>
        /// <param name="characterClass">The character class</param>
        /// <param name="playerController">The player controller</param>
        /// <returns>The created ability</returns>
        public CharacterAbility CreateAbilityForPlayer(CharacterClass characterClass, PlayerController playerController)
        {
            if (characterClass == null || playerController == null)
            {
                Debug.LogError("[CharacterManager] Cannot create ability: null character class or player controller");
                return null;
            }

            var ability = CharacterAbility.CreateAbility(characterClass.PrimaryAbilityType, characterClass, playerController);

            Debug.Log($"[CharacterManager] Created ability for player: {characterClass.PrimaryAbilityType}");

            return ability;
        }
    }
}
