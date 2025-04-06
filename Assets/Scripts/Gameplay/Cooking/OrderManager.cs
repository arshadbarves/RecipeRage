using System;
using System.Collections.Generic;
using RecipeRage.Core.Patterns;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Manages recipe orders in the game.
    /// </summary>
    public class OrderManager : NetworkBehaviour
    {
        [Header("Order Settings")]
        [SerializeField] private float _minTimeBetweenOrders = 5f;
        [SerializeField] private float _maxTimeBetweenOrders = 15f;
        [SerializeField] private int _maxActiveOrders = 5;
        [SerializeField] private float _difficultyScalingFactor = 0.8f;

        [Header("Recipes")]
        [SerializeField] private List<Recipe> _availableRecipes = new List<Recipe>();

        /// <summary>
        /// Event triggered when a new order is created.
        /// </summary>
        public event Action<RecipeOrderState> OnOrderCreated;

        /// <summary>
        /// Event triggered when an order is completed.
        /// </summary>
        public event Action<RecipeOrderState> OnOrderCompleted;

        /// <summary>
        /// Event triggered when an order expires.
        /// </summary>
        public event Action<RecipeOrderState> OnOrderExpired;

        /// <summary>
        /// The list of active orders.
        /// </summary>
        private NetworkList<RecipeOrderState> _activeOrders;

        /// <summary>
        /// Timer for generating new orders.
        /// </summary>
        private float _orderTimer;

        /// <summary>
        /// Time until the next order is generated.
        /// </summary>
        private float _timeUntilNextOrder;

        /// <summary>
        /// The next order ID to assign.
        /// </summary>
        private int _nextOrderId = 1;

        /// <summary>
        /// Initialize the order manager.
        /// </summary>
        private void Awake()
        {
            _activeOrders = new NetworkList<RecipeOrderState>();
        }

        /// <summary>
        /// Subscribe to events when the network object is spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Initialize the order timer
                _orderTimer = 0f;
                _timeUntilNextOrder = UnityEngine.Random.Range(_minTimeBetweenOrders, _maxTimeBetweenOrders);
            }

            // Subscribe to the active orders list changed event
            _activeOrders.OnListChanged += HandleActiveOrdersChanged;
        }

        /// <summary>
        /// Unsubscribe from events when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from the active orders list changed event
            _activeOrders.OnListChanged -= HandleActiveOrdersChanged;
        }

        /// <summary>
        /// Update the order manager.
        /// </summary>
        private void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }

            // Update the order timer
            _orderTimer += Time.deltaTime;

            // Check if it's time to generate a new order
            if (_orderTimer >= _timeUntilNextOrder && _activeOrders.Count < _maxActiveOrders)
            {
                GenerateNewOrder();

                // Reset the order timer
                _orderTimer = 0f;
                _timeUntilNextOrder = UnityEngine.Random.Range(_minTimeBetweenOrders, _maxTimeBetweenOrders);
            }

            // Check for expired orders
            CheckForExpiredOrders();
        }

        /// <summary>
        /// Generate a new random order.
        /// </summary>
        private void GenerateNewOrder()
        {
            if (_availableRecipes.Count == 0)
            {
                Debug.LogWarning("[OrderManager] No available recipes to generate an order from.");
                return;
            }

            // Select a random recipe
            Recipe recipe = _availableRecipes[UnityEngine.Random.Range(0, _availableRecipes.Count)];

            // Create a new order state
            RecipeOrderState orderState = new RecipeOrderState
            {
                RecipeId = recipe.Id,
                OrderId = _nextOrderId++,
                CreationTime = Time.time,
                TimeLimit = recipe.BaseTimeLimit * GetDifficultyMultiplier(recipe.Difficulty),
                IsCompleted = false,
                IsExpired = false,
                PointValue = recipe.PointValue
            };

            // Add the order to the active orders list
            _activeOrders.Add(orderState);

            Debug.Log($"[OrderManager] Generated new order: {recipe.DisplayName} (ID: {orderState.OrderId})");
        }

        /// <summary>
        /// Check for expired orders.
        /// </summary>
        private void CheckForExpiredOrders()
        {
            for (int i = 0; i < _activeOrders.Count; i++)
            {
                RecipeOrderState order = _activeOrders[i];

                // Skip completed or already expired orders
                if (order.IsCompleted || order.IsExpired)
                {
                    continue;
                }

                // Check if the order has expired
                if (Time.time - order.CreationTime >= order.TimeLimit)
                {
                    // Mark the order as expired
                    order.IsExpired = true;
                    _activeOrders[i] = order;

                    Debug.Log($"[OrderManager] Order expired: ID {order.OrderId}");
                }
            }
        }

        /// <summary>
        /// Complete an order.
        /// </summary>
        /// <param name="orderId">The ID of the order to complete.</param>
        /// <returns>True if the order was completed successfully, false otherwise.</returns>
        public bool CompleteOrder(int orderId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[OrderManager] Only the server can complete orders.");
                return false;
            }

            // Find the order with the specified ID
            for (int i = 0; i < _activeOrders.Count; i++)
            {
                RecipeOrderState order = _activeOrders[i];

                if (order.OrderId == orderId && !order.IsCompleted && !order.IsExpired)
                {
                    // Mark the order as completed
                    order.IsCompleted = true;
                    _activeOrders[i] = order;

                    Debug.Log($"[OrderManager] Order completed: ID {order.OrderId}");
                    return true;
                }
            }

            Debug.LogWarning($"[OrderManager] Failed to complete order: ID {orderId} not found or already completed/expired.");
            return false;
        }

        /// <summary>
        /// Handle changes to the active orders list.
        /// </summary>
        /// <param name="changeEvent">The list change event.</param>
        private void HandleActiveOrdersChanged(NetworkListEvent<RecipeOrderState> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<RecipeOrderState>.EventType.Add)
            {
                // A new order was added
                RecipeOrderState newOrder = _activeOrders[changeEvent.Index];
                OnOrderCreated?.Invoke(newOrder);
            }
            else if (changeEvent.Type == NetworkListEvent<RecipeOrderState>.EventType.Value)
            {
                // An order was updated
                RecipeOrderState updatedOrder = _activeOrders[changeEvent.Index];

                if (updatedOrder.IsCompleted)
                {
                    OnOrderCompleted?.Invoke(updatedOrder);
                }
                else if (updatedOrder.IsExpired)
                {
                    OnOrderExpired?.Invoke(updatedOrder);
                }
            }
        }

        /// <summary>
        /// Get the difficulty multiplier for a recipe difficulty.
        /// </summary>
        /// <param name="difficulty">The recipe difficulty.</param>
        /// <returns>The difficulty multiplier.</returns>
        private float GetDifficultyMultiplier(RecipeDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RecipeDifficulty.Easy:
                    return 1.0f;
                case RecipeDifficulty.Medium:
                    return 0.8f;
                case RecipeDifficulty.Hard:
                    return 0.6f;
                case RecipeDifficulty.Expert:
                    return 0.4f;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Get a recipe by ID.
        /// </summary>
        /// <param name="recipeId">The recipe ID.</param>
        /// <returns>The recipe, or null if not found.</returns>
        public Recipe GetRecipeById(int recipeId)
        {
            foreach (Recipe recipe in _availableRecipes)
            {
                if (recipe.Id == recipeId)
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the active orders.
        /// </summary>
        /// <returns>The list of active orders.</returns>
        public IReadOnlyList<RecipeOrderState> GetActiveOrders()
        {
            // Convert NetworkList to List to satisfy IReadOnlyList interface
            List<RecipeOrderState> ordersList = new List<RecipeOrderState>();
            foreach (var order in _activeOrders)
            {
                ordersList.Add(order);
            }
            return ordersList;
        }

        /// <summary>
        /// Try to complete an order from the client.
        /// </summary>
        /// <param name="orderId">The ID of the order to complete.</param>
        [ServerRpc(RequireOwnership = false)]
        public void CompleteOrderServerRpc(int orderId, ServerRpcParams serverRpcParams = default)
        {
            // Get the client ID that sent the RPC
            ulong clientId = serverRpcParams.Receive.SenderClientId;

            Debug.Log($"[OrderManager] Client {clientId} is trying to complete order {orderId}");

            // Try to complete the order
            bool success = CompleteOrder(orderId);

            // Notify the client of the result
            CompleteOrderResultClientRpc(orderId, success);
        }

        /// <summary>
        /// Notify clients of the result of an order completion attempt.
        /// </summary>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="success">Whether the order was completed successfully.</param>
        [ClientRpc]
        private void CompleteOrderResultClientRpc(int orderId, bool success)
        {
            Debug.Log($"[OrderManager] Order completion result: ID {orderId}, Success: {success}");
        }
    }
}
