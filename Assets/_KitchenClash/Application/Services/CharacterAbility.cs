using System;
using KitchenClash.Infrastructure.Logging;
using UnityEngine;

namespace KitchenClash.Application.Services
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
}
