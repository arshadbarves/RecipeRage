using System.Collections.Generic;
using Core.Characters;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents a plate that holds ingredients for dish assembly.
    /// Follows Single Responsibility Principle - handles only plate state.
    /// </summary>
    public class PlateItem : NetworkBehaviour, IInteractable
    {
        [Header("Plate Settings")]
        [SerializeField] private int _maxIngredients = 5;
        [SerializeField] private Transform[] _ingredientSlots;
        [SerializeField] private GameObject _plateVisual;
        
        /// <summary>
        /// The list of ingredient NetworkObject IDs on this plate.
        /// </summary>
        private NetworkList<ulong> _ingredientIds;
        
        /// <summary>
        /// The target recipe ID for this plate.
        /// </summary>
        private NetworkVariable<int> _recipeId = new NetworkVariable<int>(-1);
        
        /// <summary>
        /// Whether the dish is complete.
        /// </summary>
        private NetworkVariable<bool> _isComplete = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// Whether the plate is being held by a player.
        /// </summary>
        private NetworkVariable<bool> _isHeld = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// The ID of the player holding this plate.
        /// </summary>
        private NetworkVariable<ulong> _heldByPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
        /// <summary>
        /// Get the recipe ID.
        /// </summary>
        public int RecipeId => _recipeId.Value;
        
        /// <summary>
        /// Get whether the dish is complete.
        /// </summary>
        public bool IsComplete => _isComplete.Value;
        
        /// <summary>
        /// Get the number of ingredients on the plate.
        /// </summary>
        public int IngredientCount => _ingredientIds.Count;
        
        /// <summary>
        /// Initialize the plate.
        /// </summary>
        private void Awake()
        {
            _ingredientIds = new NetworkList<ulong>();
        }
        
        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _ingredientIds.OnListChanged += OnIngredientListChanged;
            _isComplete.OnValueChanged += OnIsCompleteChanged;
            _isHeld.OnValueChanged += OnIsHeldChanged;
        }
        
        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _ingredientIds.OnListChanged -= OnIngredientListChanged;
            _isComplete.OnValueChanged -= OnIsCompleteChanged;
            _isHeld.OnValueChanged -= OnIsHeldChanged;
        }
        
        /// <summary>
        /// Set the target recipe for this plate.
        /// </summary>
        /// <param name="recipeId">The recipe ID</param>
        public void SetRecipe(int recipeId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can set the recipe");
                return;
            }
            
            _recipeId.Value = recipeId;
        }
        
        /// <summary>
        /// Add an ingredient to the plate.
        /// </summary>
        /// <param name="ingredientNetworkId">The ingredient's NetworkObject ID</param>
        /// <returns>True if the ingredient was added</returns>
        public bool AddIngredient(ulong ingredientNetworkId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can add ingredients");
                return false;
            }
            
            // Check if plate is full
            if (_ingredientIds.Count >= _maxIngredients)
            {
                Debug.LogWarning("[PlateItem] Plate is full");
                return false;
            }
            
            // Check if ingredient already on plate
            if (_ingredientIds.Contains(ingredientNetworkId))
            {
                Debug.LogWarning("[PlateItem] Ingredient already on plate");
                return false;
            }
            
            // Add ingredient
            _ingredientIds.Add(ingredientNetworkId);
            
            // Get the ingredient NetworkObject
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientNetworkId, out NetworkObject ingredientObject))
            {
                // Parent to plate
                if (_ingredientSlots != null && _ingredientIds.Count <= _ingredientSlots.Length)
                {
                    Transform slot = _ingredientSlots[_ingredientIds.Count - 1];
                    ingredientObject.transform.SetParent(slot);
                    ingredientObject.transform.localPosition = Vector3.zero;
                    ingredientObject.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Stack on plate
                    ingredientObject.transform.SetParent(transform);
                    ingredientObject.transform.localPosition = Vector3.up * 0.1f * _ingredientIds.Count;
                    ingredientObject.transform.localRotation = Quaternion.identity;
                }
            }
            
            Debug.Log($"[PlateItem] Added ingredient {ingredientNetworkId} to plate");
            return true;
        }
        
        /// <summary>
        /// Remove an ingredient from the plate.
        /// </summary>
        /// <param name="ingredientNetworkId">The ingredient's NetworkObject ID</param>
        /// <returns>True if the ingredient was removed</returns>
        public bool RemoveIngredient(ulong ingredientNetworkId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can remove ingredients");
                return false;
            }
            
            // Check if ingredient is on plate
            if (!_ingredientIds.Contains(ingredientNetworkId))
            {
                Debug.LogWarning("[PlateItem] Ingredient not on plate");
                return false;
            }
            
            // Remove ingredient
            _ingredientIds.Remove(ingredientNetworkId);
            
            // Unparent ingredient
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientNetworkId, out NetworkObject ingredientObject))
            {
                ingredientObject.transform.SetParent(null);
            }
            
            Debug.Log($"[PlateItem] Removed ingredient {ingredientNetworkId} from plate");
            return true;
        }
        
        /// <summary>
        /// Get all ingredients on the plate.
        /// </summary>
        /// <returns>A list of ingredient items</returns>
        public List<IngredientItem> GetIngredients()
        {
            List<IngredientItem> ingredients = new List<IngredientItem>();
            
            foreach (ulong ingredientId in _ingredientIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientId, out NetworkObject ingredientObject))
                {
                    IngredientItem ingredient = ingredientObject.GetComponent<IngredientItem>();
                    if (ingredient != null)
                    {
                        ingredients.Add(ingredient);
                    }
                }
            }
            
            return ingredients;
        }
        
        /// <summary>
        /// Mark the dish as complete.
        /// </summary>
        public void CompleteDish()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can complete the dish");
                return;
            }
            
            _isComplete.Value = true;
        }
        
        /// <summary>
        /// Clear all ingredients from the plate.
        /// </summary>
        public void Clear()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can clear the plate");
                return;
            }
            
            // Unparent all ingredients
            foreach (ulong ingredientId in _ingredientIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientId, out NetworkObject ingredientObject))
                {
                    ingredientObject.transform.SetParent(null);
                }
            }
            
            _ingredientIds.Clear();
            _isComplete.Value = false;
            _recipeId.Value = -1;
        }
        
        /// <summary>
        /// Pick up the plate.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        public void PickUp(ulong playerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can pick up plates");
                return;
            }
            
            _isHeld.Value = true;
            _heldByPlayerId.Value = playerId;
        }
        
        /// <summary>
        /// Drop the plate.
        /// </summary>
        public void Drop()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[PlateItem] Only the server can drop plates");
                return;
            }
            
            _isHeld.Value = false;
            _heldByPlayerId.Value = ulong.MaxValue;
        }
        
        /// <summary>
        /// Interact with the plate.
        /// </summary>
        /// <param name="player">The player interacting</param>
        public void Interact(PlayerController player)
        {
            // Request pickup or drop via RPC
            RequestPickupDropServerRpc(player.NetworkObject);
        }
        
        /// <summary>
        /// Get the interaction prompt.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public string GetInteractionPrompt()
        {
            if (_isHeld.Value)
            {
                return "Drop Plate";
            }
            else
            {
                return $"Pick Up Plate ({_ingredientIds.Count} ingredients)";
            }
        }
        
        /// <summary>
        /// Check if the plate can be interacted with.
        /// </summary>
        /// <param name="player">The player trying to interact</param>
        /// <returns>True if the plate can be interacted with</returns>
        public bool CanInteract(PlayerController player)
        {
            // If held, only the holder can interact
            if (_isHeld.Value)
            {
                return _heldByPlayerId.Value == player.OwnerClientId;
            }
            
            // Otherwise, any player can pick it up
            return true;
        }
        
        /// <summary>
        /// Request to pick up or drop the plate.
        /// </summary>
        /// <param name="playerNetworkObject">The player's network object</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupDropServerRpc(NetworkObjectReference playerNetworkObject)
        {
            if (playerNetworkObject.TryGet(out NetworkObject networkObject))
            {
                PlayerController player = networkObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    ulong playerId = player.OwnerClientId;
                    
                    if (_isHeld.Value)
                    {
                        // Only the holder can drop
                        if (_heldByPlayerId.Value == playerId)
                        {
                            Drop();
                        }
                    }
                    else
                    {
                        // Any player can pick up
                        PickUp(playerId);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle ingredient list changes.
        /// </summary>
        private void OnIngredientListChanged(NetworkListEvent<ulong> changeEvent)
        {
            // Update visuals or trigger events
            Debug.Log($"[PlateItem] Ingredient list changed, now has {_ingredientIds.Count} ingredients");
        }
        
        /// <summary>
        /// Handle completion state changes.
        /// </summary>
        private void OnIsCompleteChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                Debug.Log("[PlateItem] Dish completed!");
                // Update visuals or trigger effects
            }
        }
        
        /// <summary>
        /// Handle held state changes.
        /// </summary>
        private void OnIsHeldChanged(bool previousValue, bool newValue)
        {
            // Update collider or visuals
            if (_plateVisual != null)
            {
                // Could add visual feedback here
            }
        }
    }
}
