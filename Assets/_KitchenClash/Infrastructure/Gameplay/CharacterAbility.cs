using System;
using KitchenClash.Domain;
using KitchenClash.Application.Models;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public abstract class CharacterAbility
    {
        public float Cooldown { get; protected set; } = 30f;
        public float Duration { get; protected set; } = 0f;
        public float CooldownTimer { get; protected set; }
        public float DurationTimer { get; protected set; }
        public bool IsOnCooldown => CooldownTimer > 0f;
        public bool IsActive => DurationTimer > 0f;

        public event Action<CharacterAbility> OnAbilityActivated;
        public event Action<CharacterAbility> OnAbilityDeactivated;

        public static CharacterAbility CreateAbility(AbilityType type, CharacterClass characterClass, object owner)
        {
            return type switch
            {
                AbilityType.SpeedBoost => new SpeedBoostAbility(),
                AbilityType.InstantCook => new InstantCookAbility(),
                AbilityType.DoubleIngredients => new DoubleIngredientsAbility(),
                AbilityType.Dash => new DashCharacterAbility(),
                AbilityType.FlavorBoost => new FlavorBoostCharacterAbility(),
                AbilityType.PerfectSlice => new PerfectSliceCharacterAbility(),
                AbilityType.KitchenWisdom => new KitchenWisdomCharacterAbility(),
                AbilityType.IngredientSwap => new IngredientSwapCharacterAbility(),
                AbilityType.SpiceRush => new SpiceRushCharacterAbility(),
                _ => new DefaultAbility()
            };
        }

        public virtual void Update(float deltaTime)
        {
            if (CooldownTimer > 0f) CooldownTimer -= deltaTime;
            if (DurationTimer > 0f)
            {
                DurationTimer -= deltaTime;
                if (DurationTimer <= 0f) Deactivate();
            }
        }

        public virtual bool Activate()
        {
            if (IsOnCooldown) return false;
            CooldownTimer = Cooldown;
            if (Duration > 0f) DurationTimer = Duration;
            OnAbilityActivated?.Invoke(this);
            return true;
        }

        public virtual void Deactivate()
        {
            if (!IsActive) return;
            DurationTimer = 0f;
            OnAbilityDeactivated?.Invoke(this);
        }

        public virtual void Reset()
        {
            CooldownTimer = 0f;
            DurationTimer = 0f;
        }
    }

    public class DefaultAbility : CharacterAbility { }
    public class SpeedBoostAbility : CharacterAbility { public SpeedBoostAbility() { Cooldown = 20f; Duration = 5f; } }
    public class InstantCookAbility : CharacterAbility { public InstantCookAbility() { Cooldown = 30f; Duration = 8f; } }
    public class DoubleIngredientsAbility : CharacterAbility { public DoubleIngredientsAbility() { Cooldown = 25f; Duration = 10f; } }
    public class DashCharacterAbility : CharacterAbility { public DashCharacterAbility() { Cooldown = 10f; Duration = 0f; } }
    public class FlavorBoostCharacterAbility : CharacterAbility { public FlavorBoostCharacterAbility() { Cooldown = 15f; Duration = 5f; } }
    public class PerfectSliceCharacterAbility : CharacterAbility { public PerfectSliceCharacterAbility() { Cooldown = 20f; Duration = 0f; } }
    public class KitchenWisdomCharacterAbility : CharacterAbility { public KitchenWisdomCharacterAbility() { Cooldown = 30f; Duration = 8f; } }
    public class IngredientSwapCharacterAbility : CharacterAbility { public IngredientSwapCharacterAbility() { Cooldown = 25f; Duration = 0f; } }
    public class SpiceRushCharacterAbility : CharacterAbility { public SpiceRushCharacterAbility() { Cooldown = 20f; Duration = 5f; } }
}
