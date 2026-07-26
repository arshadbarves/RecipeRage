using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Grandpa's active: reveal all active orders to team for 8s.
    /// </summary>
    public sealed class KitchenWisdomAbility : ActiveAbilityBase
    {
        public KitchenWisdomAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will broadcast order visibility to team UI.
            Debug.Log($"[KitchenWisdom] All active orders revealed to team for {Def.Duration}s");
        }

        public override void Deactivate()
        {
            Debug.Log("[KitchenWisdom] Order reveal expired");
            base.Deactivate();
        }
    }
}
