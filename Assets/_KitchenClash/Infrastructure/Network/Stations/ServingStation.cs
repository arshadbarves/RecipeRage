using System.Collections.Generic;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Cooking;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Network;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network.Stations
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

        private NetworkScoreManager _scoreManager;

        [Inject] private IEventBus _eventBus;
        [Inject] private IMatchContext _matchContext;
        [Inject] private IOrderService _orderService;
        private IDishValidator _validator;

        /// <summary>
        /// Initialize the serving station.
        /// </summary>
        [SerializeField] private int _teamId;
        public int TeamId => _teamId;
        
        /// <summary>
        /// Reference to the linked sink station for this team.
        /// </summary>
        private SinkStation _linkedSink;
        
        protected override void Awake()
        {
            base.Awake();

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }

            // Set station name
            _stationName = "Serving Station";

            // Create dish validator
            _validator = new StandardDishValidator();
            _orderService ??= new OrderService(null, new RecipeCatalog(null));

            ResolveRuntimeDependencies();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResolveRuntimeDependencies();
            // Find linked sink
            FindLinkedSink();
        }

        private void ResolveRuntimeDependencies()
        {
            _matchContext?.Refresh();
            _orderManager ??= _matchContext?.OrderManager;
            _scoreManager ??= _matchContext?.NetworkScoreManager;

            if (_orderManager == null)
            {
                GameLogger.LogWarning("OrderManager not available for ServingStation.");
            }

            if (_scoreManager == null)
            {
                GameLogger.LogWarning("NetworkScoreManager not available for ServingStation. Scoring will not work.");
            }
        }

        private void FindLinkedSink()
        {
            var sinks = FindObjectsByType<SinkStation>(FindObjectsSortMode.None);
            foreach (var sink in sinks)
            {
                if (sink.TeamId == _teamId)
                {
                    _linkedSink = sink;
                    break;
                }
            }
        }

        protected override void HandleInteraction(PlayerController player)
        {
            // Block if dirty dishes exist
            if (_linkedSink != null && _linkedSink.DirtyPlateCount > 0)
            {
                 // Optional: visual feedback "Wash Dishes!"
                 GameLogger.Log("Cannot serve! Sink has dirty dishes.");
                 ShowFailureVisual(); // Re-use failure visual or separate one
                 return;
            }

            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                PlateItem plate = heldObject.GetComponent<PlateItem>();

                if (plate == null || plate.IngredientCount <= 0)
                {
                    ShowFailureVisual();
                    TriggerFailureShakeClientRpc();
                    GameLogger.Log("Order failed - only assembled plates can be served");
                    return;
                }

                player.DropObject();

                bool orderCompleted = ProcessPlate(plate, player.OwnerClientId, player.TeamId, out _);

                if (orderCompleted)
                {
                    ShowSuccessVisual();
                    TriggerSuccessShakeClientRpc();
                    GameLogger.Log($"Order completed! Player {player.OwnerClientId} earned points");
                }
                else
                {
                    ShowFailureVisual();
                    TriggerFailureShakeClientRpc();
                    GameLogger.Log("Order failed - no matching order or invalid dish");
                }

                if (plate.NetworkObject != null && plate.NetworkObject.IsSpawned)
                {
                    plate.NetworkObject.Despawn();
                }
            }
        }

        /// <summary>
        /// Process a plate with assembled ingredients.
        /// </summary>
        private bool ProcessPlate(PlateItem plate, ulong playerId, int teamId, out int scoreAwarded)
        {
            scoreAwarded = 0;

            if (_orderManager == null || _validator == null || _orderService == null)
            {
                return false;
            }

            IReadOnlyList<RecipeOrderState> activeOrders = _orderManager.GetActiveOrders();
            if (activeOrders == null || activeOrders.Count == 0)
            {
                return false;
            }

            List<IngredientItem> ingredients = plate.GetIngredients();
            if (ingredients.Count == 0)
            {
                return false;
            }

            bool matchedOrder = _orderService.TryGetBestActiveOrder(
                activeOrders,
                order =>
                {
                    Recipe candidateRecipe = _orderManager.GetRecipeById(order.RecipeId);
                    return candidateRecipe != null && _validator.ValidateDish(ingredients, candidateRecipe);
                },
                out RecipeOrderState orderToComplete);

            if (!matchedOrder)
            {
                return false;
            }

            Recipe recipe = _orderManager.GetRecipeById(orderToComplete.RecipeId);
            if (recipe == null)
            {
                return false;
            }

            float timeRemaining = _orderService.GetRemainingTime(orderToComplete, Time.time);
            scoreAwarded = _validator.CalculateScore(ingredients, recipe, timeRemaining);
            DishQuality quality = _validator.GetDishQuality(ingredients, recipe);

            if (_scoreManager != null)
            {
                ScoreReason reason = quality == DishQuality.Perfect
                    ? ScoreReason.PerfectDish
                    : ScoreReason.DishCompleted;

                _scoreManager.AddScoreServerRpc(playerId, scoreAwarded, reason);
            }

            return _orderManager.CompleteOrder(orderToComplete.OrderId, teamId, scoreAwarded);
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

        [ClientRpc]
        private void TriggerSuccessShakeClientRpc()
        {
            _eventBus?.Publish(new CameraShakeEvent(0.3f, 0.3f));
        }

        [ClientRpc]
        private void TriggerFailureShakeClientRpc()
        {
            _eventBus?.Publish(new CameraShakeEvent(0.5f, 0.25f));
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
