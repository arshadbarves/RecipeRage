using System;
using Core.GameFramework.Event.Core;
using Gameplay.Ability.Components;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Ability.Events
{
    public class AbilityEvents
    {
        public class AbilityActivatedEvent : INetworkedGameEvent
        {

            public AbilityActivatedEvent(GameObject caster, BaseAbility ability, Vector3 targetPosition, GameObject target = null)
            {
                Caster = caster;
                Ability = ability;
                TargetPosition = targetPosition;
                Target = target;
            }
            public GameObject Caster { get; }
            public BaseAbility Ability { get; }
            public Vector3 TargetPosition { get; }
            public GameObject Target { get; }

            public bool IsNetworked => true;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                throw new NotImplementedException();
            }
        }

        public class AbilityCompletedEvent : INetworkedGameEvent
        {

            public AbilityCompletedEvent(GameObject caster, BaseAbility ability, bool success)
            {
                Caster = caster;
                Ability = ability;
                Success = success;
            }
            public GameObject Caster { get; }
            public BaseAbility Ability { get; }
            public bool Success { get; }

            public bool IsNetworked => true;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                throw new NotImplementedException();
            }
        }

        public class ResourceChangedEvent : INetworkedGameEvent
        {

            public ResourceChangedEvent(GameObject owner, int currentValue, int maxValue)
            {
                Owner = owner;
                CurrentValue = currentValue;
                MaxValue = maxValue;
            }
            public GameObject Owner { get; }
            public int CurrentValue { get; }
            public int MaxValue { get; }

            public bool IsNetworked => true;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                throw new NotImplementedException();
            }
        }

        public class CooldownUpdatedEvent : IGameEvent
        {

            public CooldownUpdatedEvent(GameObject owner, BaseAbility ability, float remainingTime, float totalDuration)
            {
                Owner = owner;
                Ability = ability;
                RemainingTime = remainingTime;
                TotalDuration = totalDuration;
            }
            public GameObject Owner { get; }
            public BaseAbility Ability { get; }
            public float RemainingTime { get; }
            public float TotalDuration { get; }

            public bool IsNetworked => false; // Handle locally for smoother UI updates
        }
    }
}