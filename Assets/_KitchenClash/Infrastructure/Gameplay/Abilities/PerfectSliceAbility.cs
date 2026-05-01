using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Yuki's active: current prep station completes instantly.
    /// </summary>
    public sealed class PerfectSliceAbility : ActiveAbilityBase
    {
        public PerfectSliceAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            // Stub: full integration will find the player's current prep station and complete it.
            Debug.Log("[PerfectSlice] Current prep completed instantly");
        }
    }
}
