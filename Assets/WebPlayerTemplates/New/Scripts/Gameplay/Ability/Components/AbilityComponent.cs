using System;
using System.Collections.Generic;
using Core.GameFramework.Event.Core;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Gameplay.Ability.Components
{
    public class AbilityComponent : NetworkBehaviour
    {
        [SerializeField] private List<BaseAbility> startingAbilities = new List<BaseAbility>();

        private readonly Dictionary<Type, BaseAbility> _abilities = new Dictionary<Type, BaseAbility>();
        private readonly NetworkVariable<int> _maxResource = new NetworkVariable<int>(100);
        private readonly NetworkVariable<int> _resource = new NetworkVariable<int>();

        private EventManager _eventManager;

        private void Update()
        {
            foreach (BaseAbility ability in _abilities.Values)
            {
                ability.UpdateCooldown();
            }
        }

        [Inject]
        public void Construct(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _resource.Value = _maxResource.Value;
            }

            InitializeAbilities();
        }

        private void InitializeAbilities()
        {
            foreach (BaseAbility ability in startingAbilities)
            {
                if (ability != null)
                {
                    RegisterAbility(ability);
                }
            }
        }

        public void RegisterAbility(BaseAbility ability)
        {
            Type abilityType = ability.GetType();
            if (!_abilities.ContainsKey(abilityType))
            {
                ability.Initialize(this);
                _abilities[abilityType] = ability;
            }
        }

        public T GetAbility<T>() where T : BaseAbility
        {
            return _abilities.TryGetValue(typeof(T), out BaseAbility ability) ? ability as T : null;
        }

        public bool HasResourceToCast(int cost)
        {
            return _resource.Value >= cost;
        }

        public void ConsumeResource(int amount)
        {
            if (!IsServer) return;

            _resource.Value = Mathf.Max(0, _resource.Value - amount);
        }

        public void RestoreResource(int amount)
        {
            if (!IsServer) return;

            _resource.Value = Mathf.Min(_maxResource.Value, _resource.Value + amount);
        }

        public float GetResourcePercentage()
        {
            return (float)_resource.Value / _maxResource.Value;
        }
    }
}