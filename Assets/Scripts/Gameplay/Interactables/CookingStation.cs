using RecipeRage.Core;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station for cooking ingredients.
    /// </summary>
    public class CookingStation : NetworkBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float _cookingSpeed = 0.2f;
        [SerializeField] private Transform _ingredientPlacementPoint;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject _activeVisual;
        [SerializeField] private ParticleSystem _cookingParticles;
        
        /// <summary>
        /// Whether the station is currently active.
        /// </summary>
        private NetworkVariable<bool> _isActive = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// The current ingredient being cooked.
        /// </summary>
        private NetworkVariable<NetworkObjectReference> _currentIngredient = new NetworkVariable<NetworkObjectReference>();
        
        /// <summary>
        /// Initialize the cooking station.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Subscribe to state changes
            _isActive.OnValueChanged += OnIsActiveChanged;
            _currentIngredient.OnValueChanged += OnCurrentIngredientChanged;
            
            // Initialize visual state
            UpdateVisuals();
        }
        
        /// <summary>
        /// Clean up when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from state changes
            _isActive.OnValueChanged -= OnIsActiveChanged;
            _currentIngredient.OnValueChanged -= OnCurrentIngredientChanged;
        }
        
        /// <summary>
        /// Update the cooking station.
        /// </summary>
        private void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }
            
            // If the station is active, cook the ingredient
            if (_isActive.Value)
            {
                CookIngredient();
            }
        }
        
        /// <summary>
        /// Cook the current ingredient.
        /// </summary>
        private void CookIngredient()
        {
            if (!_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                // If the ingredient is no longer valid, deactivate the station
                _isActive.Value = false;
                return;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                // If the ingredient item component is missing, deactivate the station
                _isActive.Value = false;
                return;
            }
            
            // Apply cooking to the ingredient
            ingredientItem.ApplyCooking(_cookingSpeed * Time.deltaTime);
            
            // Check if the ingredient is burned
            IngredientState state = ingredientItem.GetState();
            if (state.IsBurned)
            {
                // If the ingredient is burned, deactivate the station
                _isActive.Value = false;
            }
        }
        
        /// <summary>
        /// Place an ingredient on the cooking station.
        /// </summary>
        /// <param name="ingredient">The ingredient to place.</param>
        /// <returns>True if the ingredient was placed successfully, false otherwise.</returns>
        public bool PlaceIngredient(NetworkObject ingredient)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CookingStation] Only the server can place ingredients on cooking stations.");
                return false;
            }
            
            // Check if the station already has an ingredient
            if (_currentIngredient.Value.TryGet(out _))
            {
                return false;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredient.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                return false;
            }
            
            // Check if the ingredient requires cooking
            Ingredient ingredientData = ingredientItem.GetIngredient();
            if (ingredientData == null || !ingredientData.RequiresCooking)
            {
                return false;
            }
            
            // Place the ingredient on the station
            _currentIngredient.Value = ingredient;
            
            // Position the ingredient
            if (_ingredientPlacementPoint != null)
            {
                ingredient.transform.position = _ingredientPlacementPoint.position;
                ingredient.transform.rotation = _ingredientPlacementPoint.rotation;
            }
            
            // Activate the station
            _isActive.Value = true;
            
            return true;
        }
        
        /// <summary>
        /// Remove the current ingredient from the cooking station.
        /// </summary>
        /// <returns>The removed ingredient, or null if there was no ingredient.</returns>
        public NetworkObject RemoveIngredient()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CookingStation] Only the server can remove ingredients from cooking stations.");
                return null;
            }
            
            // Check if the station has an ingredient
            if (!_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                return null;
            }
            
            // Deactivate the station
            _isActive.Value = false;
            
            // Clear the current ingredient
            _currentIngredient.Value = new NetworkObjectReference();
            
            return ingredientObject;
        }
        
        /// <summary>
        /// Handle changes to the active state.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnIsActiveChanged(bool previousValue, bool newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Handle changes to the current ingredient.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnCurrentIngredientChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update the visual representation of the cooking station.
        /// </summary>
        private void UpdateVisuals()
        {
            // Update the active visual
            if (_activeVisual != null)
            {
                _activeVisual.SetActive(_isActive.Value);
            }
            
            // Update the cooking particles
            if (_cookingParticles != null)
            {
                if (_isActive.Value)
                {
                    if (!_cookingParticles.isPlaying)
                    {
                        _cookingParticles.Play();
                    }
                }
                else
                {
                    if (_cookingParticles.isPlaying)
                    {
                        _cookingParticles.Stop();
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when a player interacts with this cooking station.
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
            
            // Request interaction via RPC
            RequestInteractionServerRpc(playerController.OwnerClientId);
        }
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text.</returns>
        public string GetInteractionPrompt()
        {
            if (_currentIngredient.Value.TryGet(out _))
            {
                return "Take Ingredient";
            }
            else
            {
                return "Place Ingredient";
            }
        }
        
        /// <summary>
        /// Check if this object can be interacted with.
        /// </summary>
        /// <param name="interactor">The GameObject that is trying to interact with this object.</param>
        /// <returns>True if the object can be interacted with.</returns>
        public bool CanInteract(GameObject interactor)
        {
            return true;
        }
        
        /// <summary>
        /// Request to interact with the cooking station.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestInteractionServerRpc(ulong playerId)
        {
            // Check if the station has an ingredient
            if (_currentIngredient.Value.TryGet(out _))
            {
                // Remove the ingredient from the station
                NetworkObject ingredient = RemoveIngredient();
                
                if (ingredient != null)
                {
                    // Get the player object
                    NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
                    if (playerObject != null)
                    {
                        // Get the player controller
                        var playerController = playerObject.GetComponent<Core.Player.PlayerController>();
                        if (playerController != null)
                        {
                            // Try to give the ingredient to the player
                            if (playerController.TryPickUpItem(ingredient))
                            {
                                Debug.Log($"[CookingStation] Player {playerId} took ingredient from cooking station.");
                            }
                            else
                            {
                                Debug.LogWarning($"[CookingStation] Player {playerId} failed to take ingredient from cooking station.");
                            }
                        }
                    }
                }
            }
            else
            {
                // Get the player object
                NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
                if (playerObject != null)
                {
                    // Get the player controller
                    var playerController = playerObject.GetComponent<Core.Player.PlayerController>();
                    if (playerController != null)
                    {
                        // Try to get the held item from the player
                        NetworkObject heldItem = playerController.GetHeldItem();
                        if (heldItem != null)
                        {
                            // Try to place the ingredient on the station
                            if (PlaceIngredient(heldItem))
                            {
                                // Remove the item from the player
                                playerController.DropItem();
                                
                                Debug.Log($"[CookingStation] Player {playerId} placed ingredient on cooking station.");
                            }
                            else
                            {
                                Debug.LogWarning($"[CookingStation] Player {playerId} failed to place ingredient on cooking station.");
                            }
                        }
                    }
                }
            }
        }
    }
}
