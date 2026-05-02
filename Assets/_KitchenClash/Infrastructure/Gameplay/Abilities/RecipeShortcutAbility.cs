using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Marco gadget: skip one ingredient
    /// </summary>
    public sealed class RecipeShortcutAbility : ActiveAbilityBase
    {
        public RecipeShortcutAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[RecipeShortcut] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
