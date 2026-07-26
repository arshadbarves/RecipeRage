using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Marco's active: instant dash forward 3 units in aim direction.
    /// </summary>
    public sealed class DashAbility : ActiveAbilityBase
    {
        public DashAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will apply impulse to character controller.
            float dirX = ctx.AimDirX;
            float dirY = ctx.AimDirY;
            Debug.Log($"[Dash] Dash {Def.Value} units in direction ({dirX:F1},{dirY:F1})");
        }
    }
}
