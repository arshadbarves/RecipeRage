using UnityEngine;
using Unity.Netcode;
using System;

namespace RecipeRage.Gameplay.Core
{
    public class CharacterStats : NetworkBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float baseHealth = 100f;
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float baseAttackPower = 10f;
        [SerializeField] private float baseDefense = 5f;

        // Network variables for stats
        private readonly NetworkVariable<StatData> _currentStats = new();
        private readonly NetworkVariable<float> _currentStamina = new();
        private readonly NetworkVariable<float> _cookingSkill = new();

        // Stat modifiers
        private readonly NetworkList<StatModifier> _statModifiers;

        // Events
        public event Action<StatData> OnStatsChanged;
        public event Action<float> OnStaminaChanged;
        public event Action<float> OnCookingSkillChanged;

        public CharacterStats()
        {
            _statModifiers = new NetworkList<StatModifier>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Initialize base stats
                _currentStats.Value = new StatData
                {
                    Health = baseHealth,
                    Speed = baseSpeed,
                    AttackPower = baseAttackPower,
                    Defense = baseDefense
                };

                _currentStamina.Value = 100f;
                _cookingSkill.Value = 1f;
            }

            // Subscribe to network variable changes
            _currentStats.OnValueChanged += (_, newValue) => OnStatsChanged?.Invoke(newValue);
            _currentStamina.OnValueChanged += (_, newValue) => OnStaminaChanged?.Invoke(newValue);
            _cookingSkill.OnValueChanged += (_, newValue) => OnCookingSkillChanged?.Invoke(newValue);
            _statModifiers.OnListChanged += RecalculateStats;
        }

        public void AddModifier(StatModifier modifier)
        {
            if (!IsServer) return;
            _statModifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            if (!IsServer) return;
            _statModifiers.Remove(modifier);
        }

        private void RecalculateStats()
        {
            if (!IsServer) return;

            var newStats = new StatData
            {
                Health = baseHealth,
                Speed = baseSpeed,
                AttackPower = baseAttackPower,
                Defense = baseDefense
            };

            // Apply modifiers
            foreach (var modifier in _statModifiers)
            {
                switch (modifier.Type)
                {
                    case StatModifierType.Flat:
                        ApplyFlatModifier(ref newStats, modifier);
                        break;
                    case StatModifierType.Percentage:
                        ApplyPercentageModifier(ref newStats, modifier);
                        break;
                }
            }

            _currentStats.Value = newStats;
        }

        private void ApplyFlatModifier(ref StatData stats, StatModifier modifier)
        {
            switch (modifier.Stat)
            {
                case StatType.Health:
                    stats.Health += modifier.Value;
                    break;
                case StatType.Speed:
                    stats.Speed += modifier.Value;
                    break;
                case StatType.AttackPower:
                    stats.AttackPower += modifier.Value;
                    break;
                case StatType.Defense:
                    stats.Defense += modifier.Value;
                    break;
            }
        }

        private void ApplyPercentageModifier(ref StatData stats, StatModifier modifier)
        {
            float multiplier = 1f + (modifier.Value / 100f);
            switch (modifier.Stat)
            {
                case StatType.Health:
                    stats.Health *= multiplier;
                    break;
                case StatType.Speed:
                    stats.Speed *= multiplier;
                    break;
                case StatType.AttackPower:
                    stats.AttackPower *= multiplier;
                    break;
                case StatType.Defense:
                    stats.Defense *= multiplier;
                    break;
            }
        }

        public float GetCurrentHealth() => _currentStats.Value.Health;
        public float GetCurrentSpeed() => _currentStats.Value.Speed;
        public float GetCurrentAttackPower() => _currentStats.Value.AttackPower;
        public float GetCurrentDefense() => _currentStats.Value.Defense;
        public float GetCurrentStamina() => _currentStamina.Value;
        public float GetCookingSkill() => _cookingSkill.Value;
    }

    public struct StatData : INetworkSerializable, IEquatable<StatData>
    {
        public float Health;
        public float Speed;
        public float AttackPower;
        public float Defense;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Speed);
            serializer.SerializeValue(ref AttackPower);
            serializer.SerializeValue(ref Defense);
        }

        public bool Equals(StatData other)
        {
            return Health.Equals(other.Health) &&
                   Speed.Equals(other.Speed) &&
                   AttackPower.Equals(other.AttackPower) &&
                   Defense.Equals(other.Defense);
        }
    }

    public struct StatModifier : INetworkSerializable, IEquatable<StatModifier>
    {
        public StatType Stat;
        public StatModifierType Type;
        public float Value;
        public float Duration;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Stat);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Value);
            serializer.SerializeValue(ref Duration);
        }

        public bool Equals(StatModifier other)
        {
            return Stat == other.Stat &&
                   Type == other.Type &&
                   Value.Equals(other.Value) &&
                   Duration.Equals(other.Duration);
        }
    }

    public enum StatType
    {
        Health,
        Speed,
        AttackPower,
        Defense
    }

    public enum StatModifierType
    {
        Flat,
        Percentage
    }
}
