using System;
using System.Threading.Tasks;
using Core.GameFramework.Time;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Gameplay.Ability.Components
{
    public abstract class BaseAbility : NetworkBehaviour
    {
        [Header("Base Ability Settings"), SerializeField]
        protected float cooldownDuration = 1f;
        [SerializeField] protected float castTime;
        [SerializeField] protected bool requiresTarget;
        [SerializeField] protected float range = 5f;
        [SerializeField] protected GameObject effectPrefab;
        protected readonly NetworkVariable<float> CooldownRemaining = new NetworkVariable<float>();

        protected readonly NetworkVariable<bool> IsOnCooldown = new NetworkVariable<bool>();
        protected Timer CooldownTimer;
        protected AbilityComponent Owner;
        protected ITimeManager TimeManager;

        public event Action<float> OnCooldownUpdate;
        public event Action OnAbilityStart;
        public event Action OnAbilityComplete;
        public event Action OnAbilityFailed;

        [Inject]
        public virtual void Construct(ITimeManager timeManager)
        {
            TimeManager = timeManager;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                CooldownTimer = TimeManager.CreateTimer(cooldownDuration, OnCooldownComplete);
            }
        }

        public virtual void Initialize(AbilityComponent owner)
        {
            Owner = owner;
        }

        public virtual async Task<bool> TryActivateAbility(Vector3 targetPosition, GameObject target = null)
        {
            if (!CanActivateAbility())
                return false;

            if (IsServer)
            {
                StartCooldown();
                NotifyAbilityStartClientRpc();

                bool success = await ExecuteAbility(targetPosition, target);
                if (success)
                {
                    NotifyAbilityCompleteClientRpc();
                }
                else
                {
                    NotifyAbilityFailedClientRpc();
                }
                return success;
            }
            TryActivateAbilityServerRpc(targetPosition, target);
            return true;
        }

        protected virtual bool CanActivateAbility()
        {
            if (IsOnCooldown.Value)
                return false;

            if (!Owner.HasResourceToCast(GetResourceCost()))
                return false;

            return true;
        }

        protected virtual void StartCooldown()
        {
            if (!IsServer) return;

            IsOnCooldown.Value = true;
            CooldownRemaining.Value = cooldownDuration;
            CooldownTimer.Reset();
        }

        protected virtual void OnCooldownComplete()
        {
            if (!IsServer) return;

            IsOnCooldown.Value = false;
            CooldownRemaining.Value = 0;
        }

        protected abstract Task<bool> ExecuteAbility(Vector3 targetPosition, GameObject target);

        protected abstract int GetResourceCost();

        [ServerRpc(RequireOwnership = false)]
        private void TryActivateAbilityServerRpc(Vector3 targetPosition, NetworkObjectReference targetRef)
        {
            GameObject target = null;
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                target = targetObj.gameObject;
            }

            _ = TryActivateAbility(targetPosition, target);
        }

        [ClientRpc]
        private void NotifyAbilityStartClientRpc()
        {
            OnAbilityStart?.Invoke();
        }

        [ClientRpc]
        private void NotifyAbilityCompleteClientRpc()
        {
            OnAbilityComplete?.Invoke();
        }

        [ClientRpc]
        private void NotifyAbilityFailedClientRpc()
        {
            OnAbilityFailed?.Invoke();
        }

        public virtual void UpdateCooldown()
        {
            if (IsOnCooldown.Value)
            {
                float remainingTime = CooldownRemaining.Value;
                OnCooldownUpdate?.Invoke(remainingTime / cooldownDuration);
            }
        }
    }
}