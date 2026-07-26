namespace KitchenClash.Domain
{
    public sealed class StatModifier
    {
        public float Value { get; }
        public StatModifierType Type { get; }
        public object Source { get; }

        public StatModifier(float value, StatModifierType type, object source = null)
        {
            Value = value;
            Type = type;
            Source = source;
        }
    }
}
