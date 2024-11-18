using System;
using System.Collections.Generic;
using Gameplay.Ability.Effects;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Character.Stats
{
    public class CharacterStats : NetworkBehaviour
    {
        [SerializeField] private StatConfiguration statConfiguration;

        private readonly Dictionary<StatType, StatValue> _stats = new Dictionary<StatType, StatValue>();
        private readonly Dictionary<StatType, NetworkVariable<float>> _syncedStats =
            new Dictionary<StatType, NetworkVariable<float>>();
        public bool IsStunned { get; set; }

        public event Action<StatType, float> OnStatChanged;
        public event Action OnHealthDepleted;

        public void Initialize()
        {
            if (!statConfiguration)
            {
                Debug.LogError("StatConfiguration not assigned to CharacterStats!");
                return;
            }

            statConfiguration.Initialize();

            // Initialize all stats
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                StatConfiguration.StatDefinition def = statConfiguration.GetStatDefinition(statType);
                if (def != null)
                {
                    _stats[statType] = new StatValue {
                        BaseValue = def.baseValue
                    };
                    _syncedStats[statType] = new NetworkVariable<float>(def.baseValue);
                }
            }

            // Set up network variable callbacks
            if (IsServer)
            {
                foreach ((StatType statType, NetworkVariable<float> value) in _syncedStats)
                {
                    value.OnValueChanged += (prev, next) => OnStatChangedInternal(statType, next);
                }
            }
        }

        public float GetStatValue(StatType statType)
        {
            if (_stats.TryGetValue(statType, out StatValue stat))
            {
                return stat.Value;
            }
            return 0f;
        }

        public void AddModifier(StatType statType, StatModifier modifier)
        {
            if (!IsServer) return;

            if (_stats.TryGetValue(statType, out StatValue stat))
            {
                stat.AddModifier(modifier);
                SyncStatValue(statType, stat.Value);
            }
        }

        public void RemoveModifier(StatType statType, StatModifier modifier)
        {
            if (!IsServer) return;

            if (_stats.TryGetValue(statType, out StatValue stat))
            {
                if (stat.RemoveModifier(modifier))
                {
                    SyncStatValue(statType, stat.Value);
                }
            }
        }

        public void ModifyHealth(float amount)
        {
            if (!IsServer) return;

            float currentHealth = GetStatValue(StatType.Health);
            float maxHealth = GetStatValue(StatType.MaxHealth);
            float newHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

            SyncStatValue(StatType.Health, newHealth);

            if (newHealth <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }

        private void SyncStatValue(StatType statType, float value)
        {
            if (_syncedStats.TryGetValue(statType, out NetworkVariable<float> netVar))
            {
                netVar.Value = value;
            }
        }

        private void OnStatChangedInternal(StatType statType, float newValue)
        {
            OnStatChanged?.Invoke(statType, newValue);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialize();
        }
        public void ApplyDamage(DamageInfo damageInfo)
        {
            ModifyHealth(-damageInfo.Amount);
        }
        public float GetMovementSpeed()
        {
            return GetStatValue(StatType.MovementSpeed);
        }
    }
}