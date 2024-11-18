using UnityEngine;
using Unity.Netcode;
using System;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public abstract class BaseStation : NetworkBehaviour, IInteractable
    {
        [Header("Base Settings")]
        [SerializeField] protected float interactionRadius = 1.5f;
        [SerializeField] protected ParticleSystem useEffect;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip useSound;
        [SerializeField] protected AudioClip completeSound;
        [SerializeField] protected AudioClip failSound;

        // Network state
        protected readonly NetworkVariable<StateType> CurrentState = new();
        protected readonly NetworkVariable<float> NextUseTime = new();
        protected readonly NetworkVariable<int> UsageCount = new();

        // Events
        public event Action<StateType> OnStateChanged;
        public event Action<BaseNetworkCharacter> OnStationUsed;
        public event Action<BaseNetworkCharacter> OnStationCompleted;
        public event Action<BaseNetworkCharacter> OnStationFailed;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InitializeServerState();
            }

            CurrentState.OnValueChanged += (_, newState) => OnStateChanged?.Invoke(newState);
        }

        protected virtual void InitializeServerState()
        {
            CurrentState.Value = StateType.Available;
            NextUseTime.Value = 0f;
            UsageCount.Value = 0;
        }

        public virtual bool CanInteract(BaseNetworkCharacter character)
        {
            if (CurrentState.Value != StateType.Available) return false;
            if (Time.time < NextUseTime.Value) return false;

            float distance = Vector3.Distance(transform.position, character.transform.position);
            return distance <= interactionRadius;
        }

        public virtual void StartInteraction(BaseNetworkCharacter character)
        {
            if (!IsServer) return;
            if (!CanInteract(character)) return;

            HandleInteractionStartServerRpc(character.NetworkObjectId);
        }

        public virtual void CompleteInteraction(BaseNetworkCharacter character)
        {
            if (!IsServer) return;
            HandleInteractionCompleteServerRpc(character.NetworkObjectId);
        }

        public virtual void CancelInteraction(BaseNetworkCharacter character)
        {
            if (!IsServer) return;
            HandleInteractionCancelServerRpc(character.NetworkObjectId);
        }

        [ServerRpc(RequireOwnership = false)]
        protected virtual void HandleInteractionStartServerRpc(ulong characterId)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            if (OnStationUsageStart(character))
            {
                CurrentState.Value = StateType.InUse;
                UsageCount.Value++;
                OnStationUsed?.Invoke(character);
                PlayEffectClientRpc(StationEffectType.Start);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected virtual void HandleInteractionCompleteServerRpc(ulong characterId)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            if (OnStationUsageComplete(character))
            {
                CurrentState.Value = StateType.Available;
                OnStationCompleted?.Invoke(character);
                PlayEffectClientRpc(StationEffectType.Complete);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected virtual void HandleInteractionCancelServerRpc(ulong characterId)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            if (OnStationUsageCancel(character))
            {
                CurrentState.Value = StateType.Available;
                OnStationFailed?.Invoke(character);
                PlayEffectClientRpc(StationEffectType.Fail);
            }
        }

        protected abstract bool OnStationUsageStart(BaseNetworkCharacter character);
        protected abstract bool OnStationUsageComplete(BaseNetworkCharacter character);
        protected abstract bool OnStationUsageCancel(BaseNetworkCharacter character);

        [ClientRpc]
        protected virtual void PlayEffectClientRpc(StationEffectType effectType)
        {
            if (useEffect != null)
            {
                useEffect.Play();
            }

            if (audioSource != null)
            {
                switch (effectType)
                {
                    case StationEffectType.Start:
                        if (useSound != null) audioSource.PlayOneShot(useSound);
                        break;
                    case StationEffectType.Complete:
                        if (completeSound != null) audioSource.PlayOneShot(completeSound);
                        break;
                    case StationEffectType.Fail:
                        if (failSound != null) audioSource.PlayOneShot(failSound);
                        break;
                }
            }
        }

        protected BaseNetworkCharacter FindCharacter(ulong characterId)
        {
            var characters = FindObjectsOfType<BaseNetworkCharacter>();
            foreach (var character in characters)
            {
                if (character.NetworkObjectId == characterId)
                {
                    return character;
                }
            }
            return null;
        }
    }

    public enum StationEffectType
    {
        Start,
        Complete,
        Fail
    }
}
