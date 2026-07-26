using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Base class for active (cooldown-based) abilities.
    /// Concrete subclasses override ApplyEffect / RemoveEffect for specific logic.
    /// </summary>
    public abstract class ActiveAbilityBase : IAbility
    {
        protected readonly AbilityDefinition Def;

        public AbilitySlot Slot => AbilitySlot.Active;
        public float Cooldown => Def.Cooldown;
        public float Duration => Def.Duration;
        public float Value => Def.Value;

        protected ActiveAbilityBase(AbilityDefinition definition)
        {
            Def = definition;
        }

        public virtual bool CanActivate(AbilityContext ctx) => true;

        public AbilityResult Activate(AbilityContext ctx)
        {
            if (!CanActivate(ctx))
            {
                return AbilityResult.Failed;
            }

            ApplyEffect(ctx);
            Debug.Log($"[Ability] {Def.DisplayName} activated for chef {ctx.Chef}");
            return new AbilityResult(AbilityEffectType.None, Def.Value, Def.Duration, true);
        }

        /// <summary>Called when the ability is activated. Subclasses implement specific logic.</summary>
        protected abstract void ApplyEffect(AbilityContext ctx);

        /// <summary>Called when duration expires. Subclasses clean up.</summary>
        public virtual void Deactivate()
        {
            Debug.Log($"[Ability] {Def.DisplayName} deactivated");
        }
    }
}
