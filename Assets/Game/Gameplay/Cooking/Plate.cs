using System.Collections.Generic;

namespace RecipeRage
{
    /// <summary>
    /// A physical plate. Holds up to capacity arranged ingredients, consumed on serve.
    /// </summary>
    public sealed class Plate
    {
        private readonly List<IngredientItem> _contents = new List<IngredientItem>(4);
        private readonly int _capacity;

        public IReadOnlyList<IngredientItem> Contents => _contents;
        public bool IsFull => _contents.Count >= _capacity;

        public Plate(int capacity)
        {
            _capacity = capacity;
        }

        public bool TryArrange(IngredientItem item)
        {
            if (IsFull)
            {
                return false;
            }
            _contents.Add(item);
            return true;
        }
    }
}
