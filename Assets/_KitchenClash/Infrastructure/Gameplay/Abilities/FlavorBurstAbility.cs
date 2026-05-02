using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Marco active: boost dish tier by 1 for 5s
    /// </summary>
    public sealed class FlavorBurstAbility : ActiveAbilityBase
    {
        public FlavorBurstAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[FlavorBurst] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
