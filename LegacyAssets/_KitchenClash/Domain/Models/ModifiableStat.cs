namespace KitchenClash.Domain
{
    public sealed class ModifiableStat
    {
        public float BaseValue { get; set; }
        public float CurrentValue { get; set; }
        
        public ModifiableStat(float baseValue)
        {
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }

        public void Reset() => CurrentValue = BaseValue;
        public void Apply(StatModifier modifier)
        {
            switch (modifier.Type)
            {
                case StatModifierType.Flat:
                    CurrentValue += modifier.Value;
                    break;
                case StatModifierType.Percent:
                    CurrentValue *= (1f + modifier.Value);
                    break;
            }
        }
    }
}
