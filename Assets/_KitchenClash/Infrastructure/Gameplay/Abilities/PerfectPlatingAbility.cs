using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Yuki super: 2x speed bonus x3 dishes
    /// </summary>
    public sealed class PerfectPlatingAbility : ActiveAbilityBase
    {
        public PerfectPlatingAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[PerfectPlating] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
