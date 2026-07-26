namespace RecipeRage
{
    /// <summary>
    /// Runtime state of one ingredient instance moving through the kitchen.
    /// </summary>
    public sealed class IngredientItem
    {
        public IngredientDefinition Definition { get; }
        public bool IsChopped { get; private set; }
        public bool IsCooked { get; private set; }
        public bool IsBurnt { get; private set; }

        public IngredientItem(IngredientDefinition definition)
        {
            Definition = definition;
        }

        public void Chop()
        {
            if (!IsChopped)
            {
                IsChopped = true;
            }
        }

        public void Cook()
        {
            if (!IsCooked && !IsBurnt)
            {
                IsCooked = true;
            }
        }

        public void Burn()
        {
            IsBurnt = true;
        }
    }
}
