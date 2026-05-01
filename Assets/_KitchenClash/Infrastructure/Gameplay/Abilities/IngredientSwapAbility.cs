using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Bella's active: swap target opponent's held item with a random ingredient.
    /// </summary>
    public sealed class IngredientSwapAbility : ActiveAbilityBase
    {
        public IngredientSwapAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will raycast for opponent, swap their held item.
            Debug.Log("[IngredientSwap] Target opponent's held item swapped with random ingredient");
        }
    }
}
