using UnityEngine;
using Unity.Netcode;
using System;

namespace RecipeRage.Gameplay.Core
{
    public class CharacterCombat : NetworkBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private LayerMask attackMask;
        [SerializeField] private Transform attackPoint;

        [Header("Special Ability")]
        [SerializeField] private float specialCooldown = 5f;
        [SerializeField] private float specialRange = 3f;
        [SerializeField] private float specialDuration = 2f;

        // Network state
        private readonly NetworkVariable<float> _lastAttackTime = new();
        private readonly NetworkVariable<float> _lastSpecialTime = new();
        private readonly NetworkVariable<bool> _isSpecialActive = new();

        // Combat effects
        private readonly NetworkList<CombatEffect> _activeEffects;

        // Events
        public event Action<BaseNetworkCharacter> OnAttackLanded;
        public event Action<BaseNetworkCharacter> OnSpecialHit;
        public event Action<CombatEffect> OnEffectApplied;
        public event Action<CombatEffect> OnEffectRemoved;

        private CharacterStats _stats;

        public CharacterCombat()
        {
            _activeEffects = new NetworkList<CombatEffect>();
        }

        public override void OnNetworkSpawn()
        {
            _stats = GetComponent<CharacterStats>();
            _activeEffects.OnListChanged += HandleEffectsChanged;
        }

        public bool TryAttack()
        {
            if (!IsServer) return false;
            if (Time.time - _lastAttackTime.Value < attackCooldown) return false;

            _lastAttackTime.Value = Time.time;
            PerformAttackServerRpc();
            return true;
        }

        public bool TryUseSpecialAbility()
        {
            if (!IsServer) return false;
            if (Time.time - _lastSpecialTime.Value < specialCooldown) return false;
            if (_isSpecialActive.Value) return false;

            _lastSpecialTime.Value = Time.time;
            _isSpecialActive.Value = true;
            PerformSpecialAbilityServerRpc();
            return true;
        }

        [ServerRpc]
        private void PerformAttackServerRpc()
        {
            var hits = Physics.OverlapSphere(
                attackPoint.position,
                attackRange,
                attackMask
            );

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<BaseNetworkCharacter>(out var target) &&
                    target != this)
                {
                    float damage = CalculateDamage(target);
                    ApplyDamage(target, damage);
                    OnAttackLanded?.Invoke(target);
                    
                    // Apply combat effects
                    var effect = new CombatEffect
                    {
                        Type = CombatEffectType.Stun,
                        Duration = 1f,
                        Value = 0f
                    };
                    ApplyEffect(target, effect);
                }
            }

            PerformAttackClientRpc();
        }

        [ClientRpc]
        private void PerformAttackClientRpc()
        {
            // Play attack animation/VFX
        }

        [ServerRpc]
        private void PerformSpecialAbilityServerRpc()
        {
            var hits = Physics.OverlapSphere(
                transform.position,
                specialRange,
                attackMask
            );

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<BaseNetworkCharacter>(out var target) &&
                    target != this)
                {
                    OnSpecialHit?.Invoke(target);
                    
                    // Apply special effects
                    var effect = new CombatEffect
                    {
                        Type = CombatEffectType.Slow,
                        Duration = specialDuration,
                        Value = 50f // 50% slow
                    };
                    ApplyEffect(target, effect);
                }
            }

            PerformSpecialAbilityClientRpc();
        }

        [ClientRpc]
        private void PerformSpecialAbilityClientRpc()
        {
            // Play special ability animation/VFX
        }

        private float CalculateDamage(BaseNetworkCharacter target)
        {
            float baseDamage = _stats.GetCurrentAttackPower();
            float targetDefense = target.GetComponent<CharacterStats>().GetCurrentDefense();
            
            // Simple damage formula
            return Mathf.Max(1f, baseDamage - targetDefense);
        }

        private void ApplyDamage(BaseNetworkCharacter target, float damage)
        {
            target.GetComponent<CharacterStats>().TakeDamage(damage);
        }

        private void ApplyEffect(BaseNetworkCharacter target, CombatEffect effect)
        {
            if (!IsServer) return;

            var combat = target.GetComponent<CharacterCombat>();
            combat._activeEffects.Add(effect);
            OnEffectApplied?.Invoke(effect);
        }

        private void HandleEffectsChanged()
        {
            // Update character stats based on active effects
            foreach (var effect in _activeEffects)
            {
                switch (effect.Type)
                {
                    case CombatEffectType.Slow:
                        ApplySlowEffect(effect);
                        break;
                    case CombatEffectType.Stun:
                        ApplyStunEffect(effect);
                        break;
                }
            }
        }

        private void ApplySlowEffect(CombatEffect effect)
        {
            var modifier = new StatModifier
            {
                Stat = StatType.Speed,
                Type = StatModifierType.Percentage,
                Value = -effect.Value,
                Duration = effect.Duration
            };
            _stats.AddModifier(modifier);
        }

        private void ApplyStunEffect(CombatEffect effect)
        {
            // Implement stun logic
        }

        private void Update()
        {
            if (!IsServer) return;
            UpdateEffects();
        }

        private void UpdateEffects()
        {
            bool effectsChanged = false;
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (Time.time >= effect.StartTime + effect.Duration)
                {
                    _activeEffects.RemoveAt(i);
                    OnEffectRemoved?.Invoke(effect);
                    effectsChanged = true;
                }
            }

            if (effectsChanged)
            {
                HandleEffectsChanged();
            }
        }
    }

    public struct CombatEffect : INetworkSerializable, IEquatable<CombatEffect>
    {
        public CombatEffectType Type;
        public float Duration;
        public float Value;
        public float StartTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Duration);
            serializer.SerializeValue(ref Value);
            serializer.SerializeValue(ref StartTime);
        }

        public bool Equals(CombatEffect other)
        {
            return Type == other.Type &&
                   Duration.Equals(other.Duration) &&
                   Value.Equals(other.Value) &&
                   StartTime.Equals(other.StartTime);
        }
    }

    public enum CombatEffectType
    {
        None,
        Slow,
        Stun,
        Burn,
        Heal
    }
}
