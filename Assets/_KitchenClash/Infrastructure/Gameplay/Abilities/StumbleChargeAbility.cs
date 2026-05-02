using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Grandpa active: charge 4 tiles bumping opponents
    /// </summary>
    public sealed class StumbleChargeAbility : ActiveAbilityBase
    {
        public StumbleChargeAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[StumbleCharge] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
