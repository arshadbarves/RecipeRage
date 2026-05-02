using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Bella gadget: 3 pre-prepped items
    /// </summary>
    public sealed class MiseEnPlaceAbility : ActiveAbilityBase
    {
        public MiseEnPlaceAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[MiseEnPlace] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
