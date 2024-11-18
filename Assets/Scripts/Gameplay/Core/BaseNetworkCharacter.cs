using UnityEngine;
using Unity.Netcode;
using VContainer;
using System;
using RecipeRage.Gameplay.Player;
using Unity.Netcode.Components;

namespace RecipeRage.Gameplay.Core
{
    public abstract class BaseNetworkCharacter : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] protected NetworkAnimator networkAnimator;
        [SerializeField] protected NetworkTransform networkTransform;
        
        [Header("Interaction")]
        [SerializeField] protected float interactionRange = 2f;
        [SerializeField] protected LayerMask interactionMask;

        // Core components
        protected PlayerInventory Inventory { get; private set; }
        protected CharacterRecipes Recipes { get; private set; }
        protected CharacterStats Stats { get; private set; }
        protected CharacterCombat Combat { get; private set; }

        // Network state
        protected NetworkVariable<CharacterState> CurrentState;
        protected NetworkVariable<float> CurrentHealth;
        protected NetworkVariable<int> CurrentScore;

        // Events
        public event Action<float> OnHealthChanged;
        public event Action<int> OnScoreChanged;
        public event Action<CharacterState> OnStateChanged;
        public event Action<IInteractable> OnInteractionStarted;
        public event Action<IInteractable> OnInteractionCompleted;

        [Inject]
        protected virtual void Construct(
            CharacterInventory inventory,
            CharacterRecipes recipes,
            CharacterStats stats,
            CharacterCombat combat)
        {
            Inventory = inventory;
            Recipes = recipes;
            Stats = stats;
            Combat = combat;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Initialize network variables
            CurrentState = new NetworkVariable<CharacterState>(CharacterState.Idle);
            CurrentHealth = new NetworkVariable<float>(100f);
            CurrentScore = new NetworkVariable<int>(0);

            // Subscribe to network variable changes
            CurrentHealth.OnValueChanged += (prev, next) => OnHealthChanged?.Invoke(next);
            CurrentScore.OnValueChanged += (prev, next) => OnScoreChanged?.Invoke(next);
            CurrentState.OnValueChanged += (prev, next) => OnStateChanged?.Invoke(next);

            if (IsServer)
            {
                InitializeServerSide();
            }

            if (IsOwner)
            {
                InitializeClientSide();
            }
        }

        protected virtual void InitializeServerSide()
        {
            // Server-side initialization
        }

        protected virtual void InitializeClientSide()
        {
            // Client-side initialization
        }

        protected bool TryInteract(out IInteractable interactable)
        {
            interactable = null;
            if (!IsServer) return false;

            if (Physics.Raycast(
                transform.position,
                transform.forward,
                out RaycastHit hit,
                interactionRange,
                interactionMask
            ))
            {
                if (hit.collider.TryGetComponent<IInteractable>(out interactable))
                {
                    OnInteractionStarted?.Invoke(interactable);
                    return true;
                }
            }

            return false;
        }

        [ServerRpc]
        protected virtual void InteractServerRpc(NetworkObjectReference interactableRef)
        {
            if (interactableRef.TryGet(out NetworkObject networkObject) &&
                networkObject.TryGetComponent<IInteractable>(out var interactable))
            {
                if (TryStartInteraction(interactable))
                {
                    OnInteractionStarted?.Invoke(interactable);
                }
            }
        }

        protected virtual bool TryStartInteraction(IInteractable interactable)
        {
            if (!IsServer) return false;
            
            // Base interaction logic
            if (CurrentState.Value != CharacterState.Idle) return false;
            
            SetState(CharacterState.Interacting);
            return true;
        }

        protected virtual void CompleteInteraction(IInteractable interactable)
        {
            if (!IsServer) return;

            OnInteractionCompleted?.Invoke(interactable);
            SetState(CharacterState.Idle);
        }

        [ServerRpc]
        protected virtual void UpdateStateServerRpc(CharacterState newState)
        {
            SetState(newState);
        }

        protected virtual void SetState(CharacterState newState)
        {
            if (!IsServer) return;
            CurrentState.Value = newState;
        }

        [ServerRpc]
        protected virtual void TakeDamageServerRpc(float damage)
        {
            if (!IsServer) return;
            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damage);
        }

        [ServerRpc]
        protected virtual void AddScoreServerRpc(int points)
        {
            if (!IsServer) return;
            CurrentScore.Value += points;
        }
    }

    public enum CharacterState
    {
        Idle,
        Moving,
        Interacting,
        Cooking,
        Attacking,
        Stunned,
        Dead
    }

    public interface IInteractable
    {
        bool CanInteract(BaseNetworkCharacter character);
        void StartInteraction(BaseNetworkCharacter character);
        void CompleteInteraction(BaseNetworkCharacter character);
        void CancelInteraction(BaseNetworkCharacter character);
    }
}
