using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Raj's active: cook speed 2x for 5s.
    /// </summary>
    public sealed class SpiceRushAbility : ActiveAbilityBase
    {
        public SpiceRushAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will apply cook speed multiplier to player's stations.
            Debug.Log($"[SpiceRush] Cook speed x{Def.Value} for {Def.Duration}s");
        }

        public override void Deactivate()
        {
            Debug.Log("[SpiceRush] Cook speed boost expired");
            base.Deactivate();
        }
    }
}
