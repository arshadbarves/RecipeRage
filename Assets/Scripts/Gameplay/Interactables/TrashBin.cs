using RecipeRage.Core;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A trash bin for disposing of unwanted ingredients.
    /// </summary>
    public class TrashBin : NetworkBehaviour, IInteractable
    {
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem _trashParticles;
        [SerializeField] private AudioSource _trashSound;
        
        /// <summary>
        /// Dispose of an ingredient.
        /// </summary>
        /// <param name="ingredient">The ingredient to dispose of.</param>
        /// <returns>True if the ingredient was disposed of successfully, false otherwise.</returns>
        public bool DisposeIngredient(NetworkObject ingredient)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[TrashBin] Only the server can dispose of ingredients.");
                return false;
            }
            
            // Check if the ingredient is valid
            if (ingredient == null)
            {
                return false;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredient.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                return false;
            }
            
            // Destroy the ingredient
            ingredient.Despawn(true);
            
            // Play visual feedback
            PlayTrashEffects();
            
            return true;
        }
        
        /// <summary>
        /// Play trash effects.
        /// </summary>
        private void PlayTrashEffects()
        {
            // Play trash particles
            if (_trashParticles != null)
            {
                _trashParticles.Play();
            }
            
            // Play trash sound
            if (_trashSound != null)
            {
                _trashSound.Play();
            }
            
            // Notify clients to play effects
            PlayTrashEffectsClientRpc();
        }
        
        /// <summary>
        /// Called when a player interacts with this trash bin.
        /// </summary>
        /// <param name="interactor">The GameObject that is interacting with this object.</param>
        public void Interact(GameObject interactor)
        {
            // Try to get the player controller
            var playerController = interactor.GetComponent<Core.Player.PlayerController>();
            if (playerController == null)
            {
                return;
            }
            
            // Request to dispose of the held item via RPC
            RequestDisposeItemServerRpc(playerController.OwnerClientId);
        }
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text.</returns>
        public string GetInteractionPrompt()
        {
            return "Trash Item";
        }
        
        /// <summary>
        /// Check if this object can be interacted with.
        /// </summary>
        /// <param name="interactor">The GameObject that is trying to interact with this object.</param>
        /// <returns>True if the object can be interacted with.</returns>
        public bool CanInteract(GameObject interactor)
        {
            // Check if the interactor is a player
            var playerController = interactor.GetComponent<Core.Player.PlayerController>();
            if (playerController == null)
            {
                return false;
            }
            
            // Check if the player is holding an item
            return playerController.IsHoldingItem();
        }
        
        /// <summary>
        /// Request to dispose of the held item.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestDisposeItemServerRpc(ulong playerId)
        {
            // Get the player object
            NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
            if (playerObject == null)
            {
                return;
            }
            
            // Get the player controller
            var playerController = playerObject.GetComponent<Core.Player.PlayerController>();
            if (playerController == null)
            {
                return;
            }
            
            // Check if the player is holding an item
            NetworkObject heldItem = playerController.GetHeldItem();
            if (heldItem == null)
            {
                return;
            }
            
            // Drop the item from the player
            playerController.DropItem();
            
            // Dispose of the item
            if (DisposeIngredient(heldItem))
            {
                Debug.Log($"[TrashBin] Player {playerId} disposed of an item.");
            }
            else
            {
                Debug.LogWarning($"[TrashBin] Player {playerId} failed to dispose of an item.");
            }
        }
        
        /// <summary>
        /// Notify clients to play trash effects.
        /// </summary>
        [ClientRpc]
        private void PlayTrashEffectsClientRpc()
        {
            // Skip if this is the server
            if (IsServer)
            {
                return;
            }
            
            // Play trash particles
            if (_trashParticles != null)
            {
                _trashParticles.Play();
            }
            
            // Play trash sound
            if (_trashSound != null)
            {
                _trashSound.Play();
            }
        }
    }
}
