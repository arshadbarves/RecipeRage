using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Rosa gadget: slow zone 5s
    /// </summary>
    public sealed class StickyMatAbility : ActiveAbilityBase
    {
        public StickyMatAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[StickyMat] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
