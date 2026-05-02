using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Rosa super: all stations instant 6s
    /// </summary>
    public sealed class KitchenRushAbility : ActiveAbilityBase
    {
        public KitchenRushAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[KitchenRush] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
