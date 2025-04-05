using RecipeRage.Core;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station for cutting ingredients.
    /// </summary>
    public class CuttingStation : NetworkBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float _cuttingSpeed = 0.25f;
        [SerializeField] private Transform _ingredientPlacementPoint;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject _activeVisual;
        [SerializeField] private AudioSource _cuttingSound;
        
        /// <summary>
        /// Whether the station is currently active.
        /// </summary>
        private NetworkVariable<bool> _isActive = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// The current ingredient being cut.
        /// </summary>
        private NetworkVariable<NetworkObjectReference> _currentIngredient = new NetworkVariable<NetworkObjectReference>();
        
        /// <summary>
        /// The player currently using the station.
        /// </summary>
        private NetworkVariable<ulong> _currentPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);
        
        /// <summary>
        /// Initialize the cutting station.
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
        /// Place an ingredient on the cutting station.
        /// </summary>
        /// <param name="ingredient">The ingredient to place.</param>
        /// <param name="playerId">The ID of the player placing the ingredient.</param>
        /// <returns>True if the ingredient was placed successfully, false otherwise.</returns>
        public bool PlaceIngredient(NetworkObject ingredient, ulong playerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CuttingStation] Only the server can place ingredients on cutting stations.");
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
            
            // Check if the ingredient requires cutting
            Ingredient ingredientData = ingredientItem.GetIngredient();
            if (ingredientData == null || !ingredientData.RequiresCutting)
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
            
            return true;
        }
        
        /// <summary>
        /// Remove the current ingredient from the cutting station.
        /// </summary>
        /// <returns>The removed ingredient, or null if there was no ingredient.</returns>
        public NetworkObject RemoveIngredient()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CuttingStation] Only the server can remove ingredients from cutting stations.");
                return null;
            }
            
            // Check if the station has an ingredient
            if (!_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                return null;
            }
            
            // Deactivate the station
            _isActive.Value = false;
            _currentPlayerId.Value = ulong.MaxValue;
            
            // Clear the current ingredient
            _currentIngredient.Value = new NetworkObjectReference();
            
            return ingredientObject;
        }
        
        /// <summary>
        /// Start cutting the current ingredient.
        /// </summary>
        /// <param name="playerId">The ID of the player cutting the ingredient.</param>
        /// <returns>True if cutting was started successfully, false otherwise.</returns>
        public bool StartCutting(ulong playerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CuttingStation] Only the server can start cutting ingredients.");
                return false;
            }
            
            // Check if the station has an ingredient
            if (!_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                return false;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                return false;
            }
            
            // Check if the ingredient is already cut
            IngredientState state = ingredientItem.GetState();
            if (state.IsCut)
            {
                return false;
            }
            
            // Activate the station
            _isActive.Value = true;
            _currentPlayerId.Value = playerId;
            
            return true;
        }
        
        /// <summary>
        /// Stop cutting the current ingredient.
        /// </summary>
        /// <param name="playerId">The ID of the player stopping cutting.</param>
        /// <returns>True if cutting was stopped successfully, false otherwise.</returns>
        public bool StopCutting(ulong playerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CuttingStation] Only the server can stop cutting ingredients.");
                return false;
            }
            
            // Check if the station is active and the player is the one cutting
            if (!_isActive.Value || _currentPlayerId.Value != playerId)
            {
                return false;
            }
            
            // Deactivate the station
            _isActive.Value = false;
            _currentPlayerId.Value = ulong.MaxValue;
            
            return true;
        }
        
        /// <summary>
        /// Apply cutting to the current ingredient.
        /// </summary>
        public void ApplyCutting()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[CuttingStation] Only the server can apply cutting to ingredients.");
                return;
            }
            
            // Check if the station is active
            if (!_isActive.Value)
            {
                return;
            }
            
            // Check if the station has an ingredient
            if (!_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                // If the ingredient is no longer valid, deactivate the station
                _isActive.Value = false;
                _currentPlayerId.Value = ulong.MaxValue;
                return;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                // If the ingredient item component is missing, deactivate the station
                _isActive.Value = false;
                _currentPlayerId.Value = ulong.MaxValue;
                return;
            }
            
            // Apply cutting to the ingredient
            ingredientItem.ApplyCutting(_cuttingSpeed * Time.deltaTime);
            
            // Check if the ingredient is cut
            IngredientState state = ingredientItem.GetState();
            if (state.IsCut)
            {
                // If the ingredient is cut, deactivate the station
                _isActive.Value = false;
                _currentPlayerId.Value = ulong.MaxValue;
            }
        }
        
        /// <summary>
        /// Update the cutting station.
        /// </summary>
        private void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }
            
            // If the station is active, apply cutting
            if (_isActive.Value)
            {
                ApplyCutting();
            }
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
        /// Update the visual representation of the cutting station.
        /// </summary>
        private void UpdateVisuals()
        {
            // Update the active visual
            if (_activeVisual != null)
            {
                _activeVisual.SetActive(_isActive.Value);
            }
            
            // Update the cutting sound
            if (_cuttingSound != null)
            {
                if (_isActive.Value)
                {
                    if (!_cuttingSound.isPlaying)
                    {
                        _cuttingSound.Play();
                    }
                }
                else
                {
                    if (_cuttingSound.isPlaying)
                    {
                        _cuttingSound.Stop();
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when a player interacts with this cutting station.
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
            if (_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                if (_isActive.Value)
                {
                    return "Stop Cutting";
                }
                else
                {
                    // Check if the ingredient is already cut
                    IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
                    if (ingredientItem != null && ingredientItem.GetState().IsCut)
                    {
                        return "Take Ingredient";
                    }
                    else
                    {
                        return "Start Cutting";
                    }
                }
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
            // Try to get the player controller
            var playerController = interactor.GetComponent<Core.Player.PlayerController>();
            if (playerController == null)
            {
                return false;
            }
            
            // If the station is active, only the player using it can interact with it
            if (_isActive.Value)
            {
                return _currentPlayerId.Value == playerController.OwnerClientId;
            }
            
            return true;
        }
        
        /// <summary>
        /// Request to interact with the cutting station.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestInteractionServerRpc(ulong playerId)
        {
            // Check if the station has an ingredient
            if (_currentIngredient.Value.TryGet(out NetworkObject ingredientObject))
            {
                // Get the ingredient item component
                IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
                if (ingredientItem != null)
                {
                    // Check if the ingredient is already cut
                    IngredientState state = ingredientItem.GetState();
                    if (state.IsCut)
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
                                        Debug.Log($"[CuttingStation] Player {playerId} took ingredient from cutting station.");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"[CuttingStation] Player {playerId} failed to take ingredient from cutting station.");
                                    }
                                }
                            }
                        }
                    }
                    else if (_isActive.Value)
                    {
                        // Stop cutting
                        if (StopCutting(playerId))
                        {
                            Debug.Log($"[CuttingStation] Player {playerId} stopped cutting.");
                        }
                        else
                        {
                            Debug.LogWarning($"[CuttingStation] Player {playerId} failed to stop cutting.");
                        }
                    }
                    else
                    {
                        // Start cutting
                        if (StartCutting(playerId))
                        {
                            Debug.Log($"[CuttingStation] Player {playerId} started cutting.");
                        }
                        else
                        {
                            Debug.LogWarning($"[CuttingStation] Player {playerId} failed to start cutting.");
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
                            if (PlaceIngredient(heldItem, playerId))
                            {
                                // Remove the item from the player
                                playerController.DropItem();
                                
                                Debug.Log($"[CuttingStation] Player {playerId} placed ingredient on cutting station.");
                            }
                            else
                            {
                                Debug.LogWarning($"[CuttingStation] Player {playerId} failed to place ingredient on cutting station.");
                            }
                        }
                    }
                }
            }
        }
    }
}
