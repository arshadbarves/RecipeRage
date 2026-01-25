using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// ScriptableObject that defines ability configuration with type-safe parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityData", menuName = "RecipeRage/Character/Ability Data")]
    public class CharacterAbilityData : ScriptableObject
    {
        [Header("Basic Settings")]
        [Tooltip("Ability type")]
        [SerializeField] private AbilityType _abilityType;

        [Tooltip("Ability icon")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Ability description")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Timing")]
        [Tooltip("Ability cooldown in seconds")]
        [SerializeField] private float _cooldown = 30f;

        [Tooltip("Ability duration in seconds (0 for instant)")]
        [SerializeField] private float _duration = 0f;

        [Header("Parameters")]
        [Tooltip("Speed multiplier (for speed-based abilities)")]
        [SerializeField] private float _speedMultiplier = 1.5f;

        [Tooltip("Range in units (for range-based abilities)")]
        [SerializeField] private float _range = 5f;

        [Tooltip("Effect multiplier (for ingredient/score multipliers)")]
        [SerializeField] private float _effectMultiplier = 2f;

        [Tooltip("Push force (for push abilities)")]
        [SerializeField] private float _pushForce = 10f;

        public AbilityType AbilityType => _abilityType;
        public Sprite Icon => _icon;
        public string Description => _description;
        public float Cooldown => _cooldown;
        public float Duration => _duration;
        public float SpeedMultiplier => _speedMultiplier;
        public float Range => _range;
        public float EffectMultiplier => _effectMultiplier;
        public float PushForce => _pushForce;

        private void OnValidate()
        {
            _cooldown = Mathf.Max(0f, _cooldown);
            _duration = Mathf.Max(0f, _duration);
            _speedMultiplier = Mathf.Max(0.1f, _speedMultiplier);
            _range = Mathf.Max(0f, _range);
            _effectMultiplier = Mathf.Max(0.1f, _effectMultiplier);
            _pushForce = Mathf.Max(0f, _pushForce);
        }
    }
}
