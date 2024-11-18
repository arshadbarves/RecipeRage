namespace Gameplay.Character.Stats
{
    public class StatModifier
    {
        public readonly int Order;
        public readonly object Source;
        public readonly ModifierType Type;
        public readonly float Value;

        public StatModifier(float value, ModifierType type, int order = 0, object source = null)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }
    }

    public enum ModifierType
    {
        Flat = 100,
        PercentAdd = 200,
        PercentMult = 300
    }
}