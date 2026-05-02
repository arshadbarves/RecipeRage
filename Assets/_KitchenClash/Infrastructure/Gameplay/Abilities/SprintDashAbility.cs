using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Rosa active: dash forward 2 tiles
    /// </summary>
    public sealed class SprintDashAbility : ActiveAbilityBase
    {
        public SprintDashAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[SprintDash] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
