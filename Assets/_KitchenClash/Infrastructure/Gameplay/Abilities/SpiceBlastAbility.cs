using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Raj active: AoE spice stun
    /// </summary>
    public sealed class SpiceBlastAbility : ActiveAbilityBase
    {
        public SpiceBlastAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[SpiceBlast] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
