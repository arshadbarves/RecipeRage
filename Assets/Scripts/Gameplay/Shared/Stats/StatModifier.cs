namespace Gameplay.Shared.Stats
{
    public enum StatModifierType
    {
        Flat,
        PercentAdd,
        PercentMult
    }

    public class StatModifier
    {
        public readonly float Value;
        public readonly StatModifierType Type;
        public readonly object Source; // For removing specific modifiers

        public StatModifier(float value, StatModifierType type, object source = null)
        {
            Value = value;
            Type = type;
            Source = source;
        }
    }
}