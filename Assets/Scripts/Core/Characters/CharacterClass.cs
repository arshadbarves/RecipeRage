using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Core.Characters
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
        
        [Tooltip("Character model prefab")]
        [SerializeField] private GameObject _characterPrefab;
        
        [Header("Stat Modifiers")]
        [Tooltip("Movement speed modifier")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _movementSpeedModifier = 1f;
        
        [Tooltip("Interaction speed modifier")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _interactionSpeedModifier = 1f;
        
        [Tooltip("Carrying capacity modifier")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _carryingCapacityModifier = 1f;
        
        [Header("Ability Settings")]
        [Tooltip("Primary ability type")]
        [SerializeField] private AbilityType _primaryAbilityType;
        
        [Tooltip("Primary ability cooldown in seconds")]
        [SerializeField] private float _primaryAbilityCooldown = 30f;
        
        [Tooltip("Primary ability duration in seconds (0 for instant)")]
        [SerializeField] private float _primaryAbilityDuration = 0f;
        
        [Tooltip("Primary ability icon")]
        [SerializeField] private Sprite _primaryAbilityIcon;
        
        [Tooltip("Primary ability description")]
        [TextArea(2, 4)]
        [SerializeField] private string _primaryAbilityDescription;
        
        [Tooltip("Primary ability parameters (JSON format)")]
        [SerializeField] private string _primaryAbilityParameters;
        
        [Header("Unlock Settings")]
        [Tooltip("Whether the character is unlocked by default")]
        [SerializeField] private bool _unlockedByDefault = false;
        
        [Tooltip("Coins required to unlock")]
        [SerializeField] private int _unlockCost = 1000;
        
        [Tooltip("Player level required to unlock")]
        [SerializeField] private int _unlockLevel = 1;
        
        /// <summary>
        /// Unique identifier for the character class.
        /// </summary>
        public int Id => _id;
        
        /// <summary>
        /// Display name of the character class.
        /// </summary>
        public string DisplayName => _displayName;
        
        /// <summary>
        /// Description of the character class.
        /// </summary>
        public string Description => _description;
        
        /// <summary>
        /// Icon for the character class.
        /// </summary>
        public Sprite Icon => _icon;
        
        /// <summary>
        /// Character model prefab.
        /// </summary>
        public GameObject CharacterPrefab => _characterPrefab;
        
        /// <summary>
        /// Movement speed modifier.
        /// </summary>
        public float MovementSpeedModifier => _movementSpeedModifier;
        
        /// <summary>
        /// Interaction speed modifier.
        /// </summary>
        public float InteractionSpeedModifier => _interactionSpeedModifier;
        
        /// <summary>
        /// Carrying capacity modifier.
        /// </summary>
        public float CarryingCapacityModifier => _carryingCapacityModifier;
        
        /// <summary>
        /// Primary ability type.
        /// </summary>
        public AbilityType PrimaryAbilityType => _primaryAbilityType;
        
        /// <summary>
        /// Primary ability cooldown in seconds.
        /// </summary>
        public float PrimaryAbilityCooldown => _primaryAbilityCooldown;
        
        /// <summary>
        /// Primary ability duration in seconds (0 for instant).
        /// </summary>
        public float PrimaryAbilityDuration => _primaryAbilityDuration;
        
        /// <summary>
        /// Primary ability icon.
        /// </summary>
        public Sprite PrimaryAbilityIcon => _primaryAbilityIcon;
        
        /// <summary>
        /// Primary ability description.
        /// </summary>
        public string PrimaryAbilityDescription => _primaryAbilityDescription;
        
        /// <summary>
        /// Primary ability parameters (JSON format).
        /// </summary>
        public string PrimaryAbilityParameters => _primaryAbilityParameters;
        
        /// <summary>
        /// Whether the character is unlocked by default.
        /// </summary>
        public bool UnlockedByDefault => _unlockedByDefault;
        
        /// <summary>
        /// Coins required to unlock.
        /// </summary>
        public int UnlockCost => _unlockCost;
        
        /// <summary>
        /// Player level required to unlock.
        /// </summary>
        public int UnlockLevel => _unlockLevel;
        
        /// <summary>
        /// Validate the character class settings.
        /// </summary>
        private void OnValidate()
        {
            // Ensure ID is positive
            _id = Mathf.Max(0, _id);
            
            // Ensure cooldown is positive
            _primaryAbilityCooldown = Mathf.Max(0f, _primaryAbilityCooldown);
            
            // Ensure duration is non-negative
            _primaryAbilityDuration = Mathf.Max(0f, _primaryAbilityDuration);
            
            // Ensure unlock cost is non-negative
            _unlockCost = Mathf.Max(0, _unlockCost);
            
            // Ensure unlock level is positive
            _unlockLevel = Mathf.Max(1, _unlockLevel);
        }
    }
    
    /// <summary>
    /// Enum for ability types.
    /// </summary>
    public enum AbilityType
    {
        None,
        SpeedBoost,
        FreezeTime,
        DoubleIngredients,
        InstantCook,
        InstantChop,
        TeleportToStation,
        PushOtherPlayers,
        StealIngredient,
        PreventBurning,
        AutoPlate,
        IngredientMagnet
    }
}
