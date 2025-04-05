using RecipeRage.Core;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Represents an ingredient item in the game world.
    /// </summary>
    public class IngredientItem : NetworkBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider _collider;
        
        /// <summary>
        /// The current state of the ingredient.
        /// </summary>
        private NetworkVariable<IngredientState> _state = new NetworkVariable<IngredientState>();
        
        /// <summary>
        /// The ingredient data.
        /// </summary>
        private Ingredient _ingredientData;
        
        /// <summary>
        /// Whether the ingredient is being held by a player.
        /// </summary>
        private NetworkVariable<bool> _isHeld = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// The ID of the player holding this ingredient.
        /// </summary>
        private NetworkVariable<ulong> _heldByPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
        /// <summary>
        /// Initialize the ingredient item.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Subscribe to state changes
            _state.OnValueChanged += OnStateChanged;
            _isHeld.OnValueChanged += OnIsHeldChanged;
            
            // Initialize the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Clean up when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from state changes
            _state.OnValueChanged -= OnStateChanged;
            _isHeld.OnValueChanged -= OnIsHeldChanged;
        }
        
        /// <summary>
        /// Set the ingredient data.
        /// </summary>
        /// <param name="ingredient">The ingredient data.</param>
        public void SetIngredient(Ingredient ingredient)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientItem] Only the server can set the ingredient data.");
                return;
            }
            
            _ingredientData = ingredient;
            
            // Initialize the state
            IngredientState initialState = new IngredientState
            {
                IngredientId = ingredient.Id,
                IsCut = false,
                IsCooked = false,
                IsBurned = false,
                CookingProgress = 0f,
                CuttingProgress = 0f
            };
            
            _state.Value = initialState;
            
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Get the ingredient data.
        /// </summary>
        /// <returns>The ingredient data.</returns>
        public Ingredient GetIngredient()
        {
            return _ingredientData;
        }
        
        /// <summary>
        /// Get the current state of the ingredient.
        /// </summary>
        /// <returns>The ingredient state.</returns>
        public IngredientState GetState()
        {
            return _state.Value;
        }
        
        /// <summary>
        /// Update the visual representation of the ingredient.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_ingredientData == null)
            {
                return;
            }
            
            // Update the sprite
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = _ingredientData.Icon;
                
                // Apply color based on state
                Color color = _ingredientData.Color;
                
                if (_state.Value.IsBurned)
                {
                    // Burned ingredients are black
                    color = Color.black;
                }
                else if (_state.Value.IsCooked)
                {
                    // Cooked ingredients are slightly darker
                    color = Color.Lerp(_ingredientData.Color, Color.gray, 0.3f);
                }
                
                _spriteRenderer.color = color;
            }
            
            // Update the collider
            if (_collider != null)
            {
                _collider.enabled = !_isHeld.Value;
            }
        }
        
        /// <summary>
        /// Handle changes to the ingredient state.
        /// </summary>
        /// <param name="previousState">The previous state.</param>
        /// <param name="newState">The new state.</param>
        private void OnStateChanged(IngredientState previousState, IngredientState newState)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Handle changes to the held state.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnIsHeldChanged(bool previousValue, bool newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Pick up the ingredient.
        /// </summary>
        /// <param name="playerId">The ID of the player picking up the ingredient.</param>
        public void PickUp(ulong playerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientItem] Only the server can pick up ingredients.");
                return;
            }
            
            _isHeld.Value = true;
            _heldByPlayerId.Value = playerId;
        }
        
        /// <summary>
        /// Drop the ingredient.
        /// </summary>
        public void Drop()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientItem] Only the server can drop ingredients.");
                return;
            }
            
            _isHeld.Value = false;
            _heldByPlayerId.Value = ulong.MaxValue;
        }
        
        /// <summary>
        /// Apply cutting to the ingredient.
        /// </summary>
        /// <param name="amount">The amount of cutting progress to add.</param>
        public void ApplyCutting(float amount)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientItem] Only the server can apply cutting to ingredients.");
                return;
            }
            
            if (_ingredientData == null || !_ingredientData.RequiresCutting)
            {
                return;
            }
            
            IngredientState currentState = _state.Value;
            
            // If already cut, do nothing
            if (currentState.IsCut)
            {
                return;
            }
            
            // Update the cutting progress
            currentState.CuttingProgress += amount;
            
            // Check if cutting is complete
            if (currentState.CuttingProgress >= 1.0f)
            {
                currentState.IsCut = true;
                currentState.CuttingProgress = 1.0f;
            }
            
            // Update the state
            _state.Value = currentState;
        }
        
        /// <summary>
        /// Apply cooking to the ingredient.
        /// </summary>
        /// <param name="amount">The amount of cooking progress to add.</param>
        public void ApplyCooking(float amount)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientItem] Only the server can apply cooking to ingredients.");
                return;
            }
            
            if (_ingredientData == null || !_ingredientData.RequiresCooking)
            {
                return;
            }
            
            IngredientState currentState = _state.Value;
            
            // If already burned, do nothing
            if (currentState.IsBurned)
            {
                return;
            }
            
            // Update the cooking progress
            currentState.CookingProgress += amount;
            
            // Check if cooking is complete
            if (currentState.CookingProgress >= 1.0f && !currentState.IsCooked)
            {
                currentState.IsCooked = true;
                currentState.CookingProgress = 1.0f;
            }
            // Check if the ingredient is burned (cooking progress > 2.0)
            else if (currentState.CookingProgress >= 2.0f)
            {
                currentState.IsBurned = true;
                currentState.CookingProgress = 2.0f;
            }
            
            // Update the state
            _state.Value = currentState;
        }
        
        /// <summary>
        /// Called when a player interacts with this ingredient.
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
            
            // Request pickup or drop via RPC
            RequestPickupDropServerRpc(playerController.OwnerClientId);
        }
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text.</returns>
        public string GetInteractionPrompt()
        {
            if (_isHeld.Value)
            {
                return "Drop";
            }
            else
            {
                return "Pick Up";
            }
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
            
            // If the ingredient is held, only the player holding it can interact with it
            if (_isHeld.Value)
            {
                return _heldByPlayerId.Value == playerController.OwnerClientId;
            }
            
            // Otherwise, any player can interact with it
            return true;
        }
        
        /// <summary>
        /// Request to pick up or drop the ingredient.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupDropServerRpc(ulong playerId)
        {
            if (_isHeld.Value)
            {
                // Only the player holding the ingredient can drop it
                if (_heldByPlayerId.Value == playerId)
                {
                    Drop();
                }
            }
            else
            {
                // Any player can pick up the ingredient if it's not held
                PickUp(playerId);
            }
        }
    }
}
