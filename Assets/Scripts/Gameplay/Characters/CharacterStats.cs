using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Defines stat modifiers for a character class.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "RecipeRage/Character/Stats")]
    public class CharacterStats : ScriptableObject
    {
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

        public float MovementSpeedModifier => _movementSpeedModifier;
        public float InteractionSpeedModifier => _interactionSpeedModifier;
        public float CarryingCapacityModifier => _carryingCapacityModifier;

        private void OnValidate()
        {
            _movementSpeedModifier = Mathf.Clamp(_movementSpeedModifier, 0.5f, 2f);
            _interactionSpeedModifier = Mathf.Clamp(_interactionSpeedModifier, 0.5f, 2f);
            _carryingCapacityModifier = Mathf.Clamp(_carryingCapacityModifier, 0.5f, 2f);
        }
    }
}
