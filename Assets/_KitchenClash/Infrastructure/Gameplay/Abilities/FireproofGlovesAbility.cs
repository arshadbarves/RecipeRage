using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Yuki gadget: fire immunity 10s
    /// </summary>
    public sealed class FireproofGlovesAbility : ActiveAbilityBase
    {
        public FireproofGlovesAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[FireproofGloves] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
