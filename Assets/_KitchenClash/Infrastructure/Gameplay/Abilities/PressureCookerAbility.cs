using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Raj gadget: 3x cook speed 15s
    /// </summary>
    public sealed class PressureCookerAbility : ActiveAbilityBase
    {
        public PressureCookerAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[PressureCooker] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
