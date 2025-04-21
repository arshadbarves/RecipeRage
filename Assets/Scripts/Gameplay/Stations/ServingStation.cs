using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A station for serving completed dishes to fulfill orders.
    /// </summary>
    public class ServingStation : CookingStation
    {
        [Header("Serving Station Settings")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private GameObject _successVisual;
        [SerializeField] private GameObject _failureVisual;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

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
        }

        /// <summary>
        /// Check if the ingredient can be processed by this station.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be processed</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Only accept ingredients (in a real implementation, we would check if it's a completed dish)
            return ingredientItem != null;
        }

        /// <summary>
        /// Process the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        /// <returns>True if the ingredient was processed successfully</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!IsServer)
            {
                return false;
            }

            // We don't actually need to check the dish type here since CanProcessIngredient already does that
            // Just complete the processing

            // Try to find a matching order
            bool orderFound = false;

            // Get all active orders
            var activeOrders = _orderManager.GetActiveOrders();

            foreach (var order in activeOrders)
            {
                // Check if the ingredient matches what's needed for the order
                // In a real implementation, you would check if the ingredient matches the recipe
                // For now, we'll just assume any dish can complete any order

                // Complete the order
                _orderManager.CompleteOrder(order.OrderId);
                orderFound = true;

                // Show success visual
                ShowSuccessVisual();

                break;
            }

            // If no matching order was found
            if (!orderFound)
            {
                // Show failure visual
                ShowFailureVisual();
            }

            // Destroy the ingredient
            ingredientItem.NetworkObject.Despawn();

            return true;
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
