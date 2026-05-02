using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Grandpa gadget: T1 to T2 upgrade
    /// </summary>
    public sealed class VintageSpiceAbility : ActiveAbilityBase
    {
        public VintageSpiceAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[VintageSpice] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
