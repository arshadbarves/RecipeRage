using System.Collections.Generic;
using Core.Characters;
using Core.Logging;
using Gameplay.Cooking;
using Gameplay.Scoring;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A station for serving completed dishes to fulfill orders.
    /// Integrates with NetworkScoreManager for score tracking and IDishValidator for validation.
    /// </summary>
    public class ServingStation : StationBase
    {
        [Header("Serving Station Settings")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private GameObject _successVisual;
        [SerializeField] private GameObject _failureVisual;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

        /// <summary>
        /// Network score manager for tracking scores.
        /// </summary>
        private NetworkScoreManager _scoreManager;

        /// <summary>
        /// Dish validator for validating completed dishes.
        /// </summary>
        private IDishValidator _validator;

        /// <summary>
        /// Round timer for time bonus calculation.
        /// </summary>
        private RoundTimer _roundTimer;

        /// <summary>
        /// Initialize the serving station.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = "Serving Station";

            // Find the order manager if not set
            if (_orderManager == null)
            {
                _orderManager = FindFirstObjectByType<OrderManager>();
            }

            // Find network score manager
            _scoreManager = FindFirstObjectByType<NetworkScoreManager>();
            if (_scoreManager == null)
            {
                GameLogger.LogWarning("NetworkScoreManager not found. Scoring will not work.");
            }

            // Create dish validator (using standard validation strategy)
            _validator = new StandardDishValidator();

            // Find round timer
            _roundTimer = FindFirstObjectByType<RoundTimer>();
        }

        protected override void HandleInteraction(PlayerController player)
        {
            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();

                if (ingredientItem != null)
                {
                    // Take the ingredient (it is being served)
                    player.DropObject();

                    // Process the serving logic
                    bool orderCompleted = ProcessServing(ingredientItem, player.OwnerClientId);

                    // Show visual feedback
                    if (orderCompleted)
                    {
                        ShowSuccessVisual();
                        GameLogger.Log($"Order completed! Player {player.OwnerClientId} earned points");
                    }
                    else
                    {
                        ShowFailureVisual();
                        GameLogger.Log("Order failed - no matching order or invalid dish");
                    }

                    // Despawn the served item
                    if (ingredientItem.NetworkObject != null && ingredientItem.NetworkObject.IsSpawned)
                    {
                        ingredientItem.NetworkObject.Despawn();
                    }
                }
            }
        }

        /// <summary>
        /// Process the serving of an item.
        /// </summary>
        private bool ProcessServing(IngredientItem ingredientItem, ulong playerId)
        {
            // Try to get plate component (for assembled dishes)
            PlateItem plate = ingredientItem.GetComponent<PlateItem>();
            int scoreAwarded = 0;

            if (plate != null && plate.IngredientCount > 0)
            {
                // This is an assembled dish on a plate
                return ProcessPlate(plate, playerId, out scoreAwarded);
            }
            else
            {
                // This is a single ingredient (simplified serving)
                return ProcessSingleIngredient(ingredientItem, playerId, out scoreAwarded);
            }
        }

        /// <summary>
        /// Process a plate with assembled ingredients.
        /// </summary>
        private bool ProcessPlate(PlateItem plate, ulong playerId, out int scoreAwarded)
        {
            scoreAwarded = 0;

            // Get the recipe for this plate
            Recipe recipe = _orderManager?.GetRecipeById(plate.RecipeId);
            if (recipe == null)
            {
                GameLogger.LogWarning($"Recipe not found for plate (RecipeId: {plate.RecipeId})");
                return false;
            }

            // Get ingredients on the plate
            List<IngredientItem> ingredients = plate.GetIngredients();

            // Validate the dish
            if (_validator != null && _validator.ValidateDish(ingredients, recipe))
            {
                // Calculate score with time bonus
                float timeRemaining = _roundTimer != null ? _roundTimer.TimeRemaining : 0f;
                scoreAwarded = _validator.CalculateScore(ingredients, recipe, timeRemaining);

                // Get dish quality for bonus scoring
                DishQuality quality = _validator.GetDishQuality(ingredients, recipe);

                // Add score via NetworkScoreManager
                if (_scoreManager != null)
                {
                    ScoreReason reason = quality == DishQuality.Perfect
                        ? ScoreReason.PerfectDish
                        : ScoreReason.DishCompleted;

                    _scoreManager.AddScoreServerRpc(playerId, scoreAwarded, reason);
                }

                // Complete the order
                if (_orderManager != null)
                {
                    _orderManager.CompleteOrder(plate.RecipeId);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Process a single ingredient (simplified serving).
        /// </summary>
        private bool ProcessSingleIngredient(IngredientItem ingredientItem, ulong playerId, out int scoreAwarded)
        {
            scoreAwarded = 0;

            // Get all active orders
            IReadOnlyList<RecipeOrderState> activeOrders = _orderManager?.GetActiveOrders();
            if (activeOrders == null || activeOrders.Count == 0)
            {
                return false;
            }

            // Try to match with any order (simplified logic)
            foreach (RecipeOrderState order in activeOrders)
            {
                Recipe recipe = _orderManager.GetRecipeById(order.RecipeId);
                if (recipe != null)
                {
                    // Award base points for any ingredient served
                    scoreAwarded = recipe.PointValue / 2; // Half points for incomplete dish

                    // Add score
                    if (_scoreManager != null)
                    {
                        _scoreManager.AddScoreServerRpc(playerId, scoreAwarded, ScoreReason.DishCompleted);
                    }

                    // Complete the order
                    _orderManager.CompleteOrder(order.OrderId);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Show the success visual.
        /// </summary>
        private void ShowSuccessVisual()
        {
            if (_successVisual != null)
            {
                // Show success visual
                ShowSuccessVisualClientRpc();
            }

            // Play success sound
            if (_audioSource != null && _successSound != null)
            {
                _audioSource.clip = _successSound;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Show the failure visual.
        /// </summary>
        private void ShowFailureVisual()
        {
            if (_failureVisual != null)
            {
                // Show failure visual
                ShowFailureVisualClientRpc();
            }

            // Play failure sound
            if (_audioSource != null && _failureSound != null)
            {
                _audioSource.clip = _failureSound;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Show the success visual on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowSuccessVisualClientRpc()
        {
            if (_successVisual != null)
            {
                // Show success visual
                _successVisual.SetActive(true);

                // Hide after a delay
                Invoke(nameof(HideSuccessVisual), 1.0f);
            }
        }

        /// <summary>
        /// Show the failure visual on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowFailureVisualClientRpc()
        {
            if (_failureVisual != null)
            {
                // Show failure visual
                _failureVisual.SetActive(true);

                // Hide after a delay
                Invoke(nameof(HideFailureVisual), 1.0f);
            }
        }

        /// <summary>
        /// Hide the success visual.
        /// </summary>
        private void HideSuccessVisual()
        {
            if (_successVisual != null)
            {
                _successVisual.SetActive(false);
            }
        }

        /// <summary>
        /// Hide the failure visual.
        /// </summary>
        private void HideFailureVisual()
        {
            if (_failureVisual != null)
            {
                _failureVisual.SetActive(false);
            }
        }
    }
}
