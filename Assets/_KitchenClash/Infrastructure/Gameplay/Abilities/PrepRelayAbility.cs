using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Bella active: relay prep to nearest teammate
    /// </summary>
    public sealed class PrepRelayAbility : ActiveAbilityBase
    {
        public PrepRelayAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[PrepRelay] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
