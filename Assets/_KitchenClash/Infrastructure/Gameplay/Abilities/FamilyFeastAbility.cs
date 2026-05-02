using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Grandpa super: auto-serve all T1
    /// </summary>
    public sealed class FamilyFeastAbility : ActiveAbilityBase
    {
        public FamilyFeastAbility(AbilityDefinition def) : base(def) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            Debug.Log($"[FamilyFeast] Activated (value={Def.Value}, duration={Def.Duration})");
        }
    }
}
