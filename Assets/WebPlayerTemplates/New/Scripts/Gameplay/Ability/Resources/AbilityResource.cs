using System;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Ability.Resources
{
    public class AbilityResource : NetworkBehaviour
    {
        [SerializeField] private float maxResource = 100f;
        [SerializeField] private float regenRate = 5f;

        private readonly NetworkVariable<float> _currentResource = new NetworkVariable<float>();

        private void Update()
        {
            if (IsServer)
            {
                RegenerateResource();
            }
        }

        public event Action<float> OnResourceChanged;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentResource.Value = maxResource;
            }
        }

        private void RegenerateResource()
        {
            if (_currentResource.Value < maxResource)
            {
                _currentResource.Value = Mathf.Min(
                    maxResource,
                    _currentResource.Value + regenRate * Time.deltaTime
                );

                OnResourceChanged?.Invoke(_currentResource.Value);
            }
        }

        public bool ConsumeResource(float amount)
        {
            if (!IsServer) return false;

            if (_currentResource.Value >= amount)
            {
                _currentResource.Value -= amount;
                OnResourceChanged?.Invoke(_currentResource.Value);
                return true;
            }

            return false;
        }

        public float GetResourcePercentage()
        {
            return _currentResource.Value / maxResource;
        }
    }
}