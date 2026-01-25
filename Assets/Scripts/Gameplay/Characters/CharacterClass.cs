using System.Collections.Generic;
using Gameplay.Skins.Data;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Scriptable object that defines a character class in RecipeRage.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterClass", menuName = "RecipeRage/Character Class")]
    public class CharacterClass : ScriptableObject
    {
        [Header("Basic Settings")]
        [Tooltip("Unique identifier for the character class")]
        [SerializeField] private int _id;

        [Tooltip("Display name of the character class")]
        [SerializeField] private string _displayName;

        [Tooltip("Description of the character class")]
        [TextArea(3, 5)]
        [SerializeField] private string _description;

        [Tooltip("Icon for the character class")]
        [SerializeField] private Sprite _icon;

        [Header("Character Data")]
        [Tooltip("Character stats configuration")]
        [SerializeField] private CharacterStats _stats;

        [Tooltip("Primary ability configuration")]
        [SerializeField] private CharacterAbilityData _primaryAbility;

        [Tooltip("Unlock requirements")]
        [SerializeField] private CharacterUnlockData _unlockData;

        [Header("Skins")]
        [Tooltip("Available skins for this character")]
        [SerializeField] private List<SkinItem> _skins = new List<SkinItem>();

        public int Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public List<SkinItem> Skins => _skins;
        public CharacterStats Stats => _stats;
        public CharacterAbilityData PrimaryAbility => _primaryAbility;
        public CharacterUnlockData UnlockData => _unlockData;

        private void OnValidate()
        {
            _id = Mathf.Max(0, _id);
        }
    }
}
