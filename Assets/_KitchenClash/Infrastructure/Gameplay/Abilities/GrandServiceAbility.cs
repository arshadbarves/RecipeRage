using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Marco super: completes longest order
    /// </summary>
    public sealed class GrandServiceAbility : ActiveAbilityBase
    {
        public GrandServiceAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[GrandService] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
