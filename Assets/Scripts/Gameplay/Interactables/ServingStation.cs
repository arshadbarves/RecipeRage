using System.Collections.Generic;
using RecipeRage.Core;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station for serving completed dishes.
    /// </summary>
    public class ServingStation : NetworkBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private Transform _ingredientPlacementArea;
        [SerializeField] private OrderManager _orderManager;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject _successVisual;
        [SerializeField] private GameObject _failureVisual;
        [SerializeField] private float _feedbackDuration = 1.5f;
        
        /// <summary>
        /// The current ingredients on the serving station.
        /// </summary>
        private NetworkList<NetworkObjectReference> _currentIngredients;
        
        /// <summary>
        /// Timer for visual feedback.
        /// </summary>
        private float _feedbackTimer;
        
        /// <summary>
        /// Whether the station is showing success feedback.
        /// </summary>
        private NetworkVariable<bool> _showingSuccessFeedback = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// Whether the station is showing failure feedback.
        /// </summary>
        private NetworkVariable<bool> _showingFailureFeedback = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// Initialize the serving station.
        /// </summary>
        private void Awake()
        {
            _currentIngredients = new NetworkList<NetworkObjectReference>();
        }
        
        /// <summary>
        /// Initialize the serving station.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // Subscribe to state changes
            _showingSuccessFeedback.OnValueChanged += OnShowingSuccessFeedbackChanged;
            _showingFailureFeedback.OnValueChanged += OnShowingFailureFeedbackChanged;
            
            // Initialize visual state
            UpdateVisuals();
        }
        
        /// <summary>
        /// Clean up when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from state changes
            _showingSuccessFeedback.OnValueChanged -= OnShowingSuccessFeedbackChanged;
            _showingFailureFeedback.OnValueChanged -= OnShowingFailureFeedbackChanged;
        }
        
        /// <summary>
        /// Update the serving station.
        /// </summary>
        private void Update()
        {
            // Update the feedback timer
            if (_feedbackTimer > 0)
            {
                _feedbackTimer -= Time.deltaTime;
                
                if (_feedbackTimer <= 0)
                {
                    // Reset the feedback
                    if (IsServer)
                    {
                        _showingSuccessFeedback.Value = false;
                        _showingFailureFeedback.Value = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Place an ingredient on the serving station.
        /// </summary>
        /// <param name="ingredient">The ingredient to place.</param>
        /// <returns>True if the ingredient was placed successfully, false otherwise.</returns>
        public bool PlaceIngredient(NetworkObject ingredient)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ServingStation] Only the server can place ingredients on serving stations.");
                return false;
            }
            
            // Get the ingredient item component
            IngredientItem ingredientItem = ingredient.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                return false;
            }
            
            // Add the ingredient to the list
            _currentIngredients.Add(ingredient);
            
            // Position the ingredient
            if (_ingredientPlacementArea != null)
            {
                // Calculate a position within the placement area
                float x = Random.Range(-0.4f, 0.4f);
                float z = Random.Range(-0.4f, 0.4f);
                
                ingredient.transform.position = _ingredientPlacementArea.position + new Vector3(x, 0.1f, z);
                ingredient.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
            
            return true;
        }
        
        /// <summary>
        /// Clear all ingredients from the serving station.
        /// </summary>
        public void ClearIngredients()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ServingStation] Only the server can clear ingredients from serving stations.");
                return;
            }
            
            // Destroy all ingredients
            for (int i = 0; i < _currentIngredients.Count; i++)
            {
                if (_currentIngredients[i].TryGet(out NetworkObject ingredientObject))
                {
                    ingredientObject.Despawn(true);
                }
            }
            
            // Clear the list
            _currentIngredients.Clear();
        }
        
        /// <summary>
        /// Try to serve the current ingredients as a dish.
        /// </summary>
        /// <returns>True if the dish was served successfully, false otherwise.</returns>
        public bool TryServe()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ServingStation] Only the server can serve dishes.");
                return false;
            }
            
            // Check if there are any ingredients
            if (_currentIngredients.Count == 0)
            {
                return false;
            }
            
            // Check if the order manager is available
            if (_orderManager == null)
            {
                Debug.LogWarning("[ServingStation] Order manager is not set.");
                return false;
            }
            
            // Get the active orders
            IReadOnlyList<RecipeOrderState> activeOrders = _orderManager.GetActiveOrders();
            
            // Check if there are any active orders
            if (activeOrders.Count == 0)
            {
                // Show failure feedback
                _showingFailureFeedback.Value = true;
                _feedbackTimer = _feedbackDuration;
                
                return false;
            }
            
            // Get the ingredients on the serving station
            List<IngredientState> ingredientStates = new List<IngredientState>();
            foreach (NetworkObjectReference ingredientRef in _currentIngredients)
            {
                if (ingredientRef.TryGet(out NetworkObject ingredientObject))
                {
                    IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
                    if (ingredientItem != null)
                    {
                        ingredientStates.Add(ingredientItem.GetState());
                    }
                }
            }
            
            // Check if the ingredients match any active order
            foreach (RecipeOrderState order in activeOrders)
            {
                // Skip completed or expired orders
                if (order.IsCompleted || order.IsExpired)
                {
                    continue;
                }
                
                // Get the recipe for this order
                Recipe recipe = _orderManager.GetRecipeById(order.RecipeId);
                if (recipe == null)
                {
                    continue;
                }
                
                // Check if the ingredients match the recipe
                if (IngredientsMatchRecipe(ingredientStates, recipe))
                {
                    // Complete the order
                    _orderManager.CompleteOrder(order.OrderId);
                    
                    // Clear the ingredients
                    ClearIngredients();
                    
                    // Show success feedback
                    _showingSuccessFeedback.Value = true;
                    _feedbackTimer = _feedbackDuration;
                    
                    return true;
                }
            }
            
            // No matching order found
            // Show failure feedback
            _showingFailureFeedback.Value = true;
            _feedbackTimer = _feedbackDuration;
            
            return false;
        }
        
        /// <summary>
        /// Check if the ingredients match a recipe.
        /// </summary>
        /// <param name="ingredientStates">The ingredient states.</param>
        /// <param name="recipe">The recipe to check against.</param>
        /// <returns>True if the ingredients match the recipe, false otherwise.</returns>
        private bool IngredientsMatchRecipe(List<IngredientState> ingredientStates, Recipe recipe)
        {
            // Check if the number of ingredients matches
            if (ingredientStates.Count != recipe.Ingredients.Count)
            {
                return false;
            }
            
            // Create a copy of the ingredient states list
            List<IngredientState> remainingIngredients = new List<IngredientState>(ingredientStates);
            
            // Check each recipe ingredient
            foreach (RecipeIngredient recipeIngredient in recipe.Ingredients)
            {
                bool found = false;
                
                // Look for a matching ingredient
                for (int i = 0; i < remainingIngredients.Count; i++)
                {
                    IngredientState state = remainingIngredients[i];
                    
                    // Check if the ingredient ID matches
                    if (state.IngredientId == recipeIngredient.Ingredient.Id)
                    {
                        // Check if the preparation matches
                        if ((!recipeIngredient.RequireCut || state.IsCut) &&
                            (!recipeIngredient.RequireCooked || state.IsCooked) &&
                            !state.IsBurned)
                        {
                            // Found a match
                            found = true;
                            
                            // Remove the ingredient from the list
                            remainingIngredients.RemoveAt(i);
                            
                            break;
                        }
                    }
                }
                
                // If no match was found for this recipe ingredient, the recipe doesn't match
                if (!found)
                {
                    return false;
                }
            }
            
            // All recipe ingredients were found
            return true;
        }
        
        /// <summary>
        /// Handle changes to the success feedback state.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnShowingSuccessFeedbackChanged(bool previousValue, bool newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Handle changes to the failure feedback state.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnShowingFailureFeedbackChanged(bool previousValue, bool newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update the visual representation of the serving station.
        /// </summary>
        private void UpdateVisuals()
        {
            // Update the success visual
            if (_successVisual != null)
            {
                _successVisual.SetActive(_showingSuccessFeedback.Value);
            }
            
            // Update the failure visual
            if (_failureVisual != null)
            {
                _failureVisual.SetActive(_showingFailureFeedback.Value);
            }
        }
        
        /// <summary>
        /// Called when a player interacts with this serving station.
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
            if (_currentIngredients.Count > 0)
            {
                return "Serve Dish";
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
        /// Request to interact with the serving station.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestInteractionServerRpc(ulong playerId)
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
            
            // Check if the player is holding an ingredient
            NetworkObject heldItem = playerController.GetHeldItem();
            if (heldItem != null)
            {
                // Try to place the ingredient on the station
                if (PlaceIngredient(heldItem))
                {
                    // Remove the item from the player
                    playerController.DropItem();
                    
                    Debug.Log($"[ServingStation] Player {playerId} placed ingredient on serving station.");
                }
                else
                {
                    Debug.LogWarning($"[ServingStation] Player {playerId} failed to place ingredient on serving station.");
                }
            }
            else if (_currentIngredients.Count > 0)
            {
                // Try to serve the dish
                if (TryServe())
                {
                    Debug.Log($"[ServingStation] Player {playerId} served a dish successfully.");
                }
                else
                {
                    Debug.LogWarning($"[ServingStation] Player {playerId} failed to serve a dish.");
                }
            }
        }
    }
}
