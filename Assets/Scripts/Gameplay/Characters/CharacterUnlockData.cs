using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Defines unlock requirements for a character class.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterUnlock", menuName = "RecipeRage/Character/Unlock Data")]
    public class CharacterUnlockData : ScriptableObject
    {
        [Header("Unlock Settings")]
        [Tooltip("Whether the character is unlocked by default")]
        [SerializeField] private bool _unlockedByDefault = false;

        [Tooltip("Coins required to unlock")]
        [SerializeField] private int _unlockCost = 1000;

        [Tooltip("Player level required to unlock")]
        [SerializeField] private int _unlockLevel = 1;

        public bool UnlockedByDefault => _unlockedByDefault;
        public int UnlockCost => _unlockCost;
        public int UnlockLevel => _unlockLevel;

        private void OnValidate()
        {
            _unlockCost = Mathf.Max(0, _unlockCost);
            _unlockLevel = Mathf.Max(1, _unlockLevel);
        }
    }
}
