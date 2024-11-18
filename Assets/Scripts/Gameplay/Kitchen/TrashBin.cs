using UnityEngine;
using Unity.Netcode;
using System;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen
{
    public class TrashBin : NetworkBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private int pointPenalty = 10;
        [SerializeField] private ParticleSystem trashEffect;
        [SerializeField] private AudioClip trashSound;

        // Network state
        private readonly NetworkVariable<StateType> _currentState = new();
        private readonly NetworkVariable<int> _itemsTrashed = new();

        // Events
        public event Action<InventoryItem> OnItemTrashed;
        public event Action<int> OnPenaltyApplied;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentState.Value = StateType.Available;
            }
        }

        public bool CanInteract(BaseNetworkCharacter character)
        {
            if (_currentState.Value != StateType.Available) return false;

            float distance = Vector3.Distance(transform.position, character.transform.position);
            return distance <= interactionRadius;
        }

        public void StartInteraction(BaseNetworkCharacter character)
        {
            if (!IsServer) return;
            if (!CanInteract(character)) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            // Find burnt or low quality items
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (ShouldTrashItem(item))
                {
                    TrashItemServerRpc(character.NetworkObjectId, item);
                }
            }
        }

        private bool ShouldTrashItem(InventoryItem item)
        {
            // Trash items that are:
            // 1. Burnt food (quality <= 0)
            // 2. Very low quality ingredients (quality < 0.2)
            return item.Quality <= 0f || 
                   (item.Type == ItemType.Ingredient && item.Quality < 0.2f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TrashItemServerRpc(ulong characterId, InventoryItem item)
        {
            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory.TryRemoveItem(item))
            {
                _itemsTrashed.Value++;
                OnItemTrashed?.Invoke(item);

                // Apply point penalty
                var stats = character.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.AddScore(-pointPenalty);
                    OnPenaltyApplied?.Invoke(pointPenalty);
                }

                // Play effects
                PlayEffectsClientRpc();
            }
        }

        [ClientRpc]
        private void PlayEffectsClientRpc()
        {
            if (trashEffect != null)
            {
                trashEffect.Play();
            }

            if (trashSound != null)
            {
                AudioSource.PlayClipAtPoint(trashSound, transform.position);
            }
        }

        private BaseNetworkCharacter FindCharacter(ulong characterId)
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

        public void CompleteInteraction(BaseNetworkCharacter character)
        {
            // Instant interaction, no completion needed
        }

        public void CancelInteraction(BaseNetworkCharacter character)
        {
            // Instant interaction, no cancellation needed
        }
    }
}
