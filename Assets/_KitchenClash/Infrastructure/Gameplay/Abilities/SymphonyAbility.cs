using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Bella super: team sees all orders + buff 8s
    /// </summary>
    public sealed class SymphonyAbility : ActiveAbilityBase
    {
        public SymphonyAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[Symphony] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
