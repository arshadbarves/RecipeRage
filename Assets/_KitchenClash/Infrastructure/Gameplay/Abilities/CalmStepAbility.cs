using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Yuki active: slow-motion for 3s
    /// </summary>
    public sealed class CalmStepAbility : ActiveAbilityBase
    {
        public CalmStepAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[CalmStep] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
