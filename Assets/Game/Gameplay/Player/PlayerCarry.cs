using System.Collections.Generic;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// What the player is holding: up to CarryCapacity ingredients and optionally one plate.
    /// Capacity default is 2 for ALL chefs (config-driven).
    /// </summary>
    public sealed class PlayerCarry
    {
        private readonly List<IngredientItem> _items = new List<IngredientItem>(4);
        private readonly int _capacity;
        private int _capacityBonus;

        public IReadOnlyList<IngredientItem> Items => _items;
        public Plate Plate { get; private set; }
        public bool HasPlate => Plate != null;

        public PlayerCarry(IConfigService config)
        {
            _capacity = config != null
                ? config.Get(ConfigKeys.CarryCapacity, ConfigKeys.Defaults.CarryCapacity)
                : ConfigKeys.Defaults.CarryCapacity;
        }

        public void SetCapacityBonus(int bonus)
        {
            _capacityBonus = bonus;
        }

        public bool TryAdd(IngredientItem item)
        {
            if (_items.Count >= _capacity + _capacityBonus)
            {
                return false;
            }
            _items.Add(item);
            return true;
        }

        public bool Remove(IngredientItem item)
        {
            return _items.Remove(item);
        }

        public void TakePlate(Plate plate)
        {
            Plate = plate;
        }

        public Plate ReleasePlate()
        {
            var plate = Plate;
            Plate = null;
            return plate;
        }
    }
}
