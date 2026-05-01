namespace KitchenClash.Domain
{
    public sealed class AbilityResult
    {
        public AbilityEffectType EffectType { get; }
        public float Value { get; }
        public float Duration { get; }
        public bool Success { get; }

        public AbilityResult(AbilityEffectType effectType, float value = 0f, float duration = 0f, bool success = true)
        {
            EffectType = effectType;
            Value = value;
            Duration = duration;
            Success = success;
        }

        public static AbilityResult Failed => new(AbilityEffectType.None, success: false);
    }
}
