using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Raj super: all stations instant 6s
    /// </summary>
    public sealed class CurryOverdriveAbility : ActiveAbilityBase
    {
        public CurryOverdriveAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[CurryOverdrive] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
