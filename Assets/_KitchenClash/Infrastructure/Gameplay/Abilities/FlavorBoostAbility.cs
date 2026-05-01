using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Rosa's active: nearby dishes gain +1 quality tier for 5s.
    /// </summary>
    public sealed class FlavorBoostAbility : ActiveAbilityBase
    {
        public FlavorBoostAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will query nearby dish entities and boost quality.
            Debug.Log($"[FlavorBoost] +{Def.Value} quality tier to nearby dishes for {Def.Duration}s");
        }

        public override void Deactivate()
        {
            Debug.Log("[FlavorBoost] Quality boost expired");
            base.Deactivate();
        }
    }
}
