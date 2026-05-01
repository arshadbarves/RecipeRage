using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.Shared;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network.Stations
{
    /// <summary>
    /// A station that provides ingredients (formerly IngredientSpawner).
    /// </summary>
    public class IngredientCrate : StationBase
    {
        [Header("Crate Settings")]
        [SerializeField] private Ingredient _ingredientToProvide;
        [SerializeField] private float _provideCooldown = 3f;
        [SerializeField] private GameObject _availableVisual;
        [SerializeField] private GameObject _cooldownVisual;

        /// <summary>
        /// Whether the crate is currently on cooldown.
        /// </summary>
        private NetworkVariable<bool> _isOnCooldown = new NetworkVariable<bool>(false);

        /// <summary>
        /// The cooldown timer.
        /// </summary>
        private float _cooldownTimer;

        /// <summary>
        /// Reference to the network ingredient spawner service.
        /// </summary>
        private IngredientNetworkSpawner _ingredientNetworkSpawner;

        [Inject] private IMatchContext _matchContext;

        public Ingredient ProvidedIngredient => _ingredientToProvide;

        /// <summary>
        /// Initialize the ingredient crate.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
            else
            {
                GameLogger.LogWarning("[IngredientCrate] LifetimeScope not found. Ingredient spawner injection will be unavailable.");
            }

            // Set station name
            _stationName = $"{_ingredientToProvide?.DisplayName ?? "Ingredient"} Crate";

            ResolveIngredientSpawner(logIfMissing: false);
        }

        public void ConfigureIngredient(Ingredient ingredient)
        {
            _ingredientToProvide = ingredient;
            _stationName = $"{_ingredientToProvide?.DisplayName ?? "Ingredient"} Crate";
        }

        /// <summary>
        /// Set up network variables when the network object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResolveIngredientSpawner();

            // Subscribe to network variable changes
            _isOnCooldown.OnValueChanged += OnCooldownChanged;

            // Initialize visuals
            UpdateVisuals();
        }

        /// <summary>
        /// Clean up when the network object despawns.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Unsubscribe from network variable changes
            _isOnCooldown.OnValueChanged -= OnCooldownChanged;
        }

        private void Update()
        {
            // Only update on the server
            if (!IsServer)
            {
                return;
            }

            // Update cooldown timer
            if (_isOnCooldown.Value)
            {
                _cooldownTimer -= Time.deltaTime;

                if (_cooldownTimer <= 0f)
                {
                    // Reset cooldown
                    _isOnCooldown.Value = false;
                }
            }
        }

        /// <summary>
        /// Interact with the ingredient crate.
        /// </summary>
        protected override void HandleInteraction(PlayerController player)
        {
            // Check if the crate is on cooldown
            if (_isOnCooldown.Value)
            {
                GameLogger.Log($"Crate is on cooldown ({_cooldownTimer:F1}s remaining)");
                return;
            }

            // Check if the player is already holding an item
            if (player.IsHoldingObject())
            {
                GameLogger.Log("Player is already holding an item");
                return;
            }

            // Provide the ingredient
            ProvideIngredient(player);

            // Start cooldown
            _isOnCooldown.Value = true;
            _cooldownTimer = _provideCooldown;
        }

        /// <summary>
        /// Spawn an ingredient and give it to the player.
        /// </summary>
        private void ProvideIngredient(PlayerController player)
        {
            if (_ingredientToProvide == null)
            {
                GameLogger.LogWarning("No ingredient to provide");
                return;
            }

            // Check if we have the network spawner service
            if (_ingredientNetworkSpawner == null)
            {
                ResolveIngredientSpawner();
            }

            if (_ingredientNetworkSpawner == null)
            {
                GameLogger.LogError("IngredientNetworkSpawner service not available. Cannot spawn ingredient.");
                return;
            }

            // Use the network spawner service to spawn the ingredient
            Vector3 spawnPosition = _ingredientPlacementPoint != null
                ? _ingredientPlacementPoint.position
                : transform.position + Vector3.up;

            NetworkObject ingredientNetworkObject = _ingredientNetworkSpawner.SpawnIngredient(_ingredientToProvide, spawnPosition);

            if (ingredientNetworkObject == null)
            {
                GameLogger.LogError("Failed to spawn ingredient via network spawner");
                return;
            }

            // Get the ingredient item component
            IngredientItem ingredientItem = ingredientNetworkObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                GameLogger.LogWarning("Spawned object doesn't have IngredientItem component");
                return;
            }

            // Give the ingredient to the player
            player.PickUpObject(ingredientNetworkObject.gameObject);

            // Play sound
            if (_audioSource != null && _interactSound != null)
            {
                _audioSource.clip = _interactSound;
                _audioSource.Play();
            }

            GameLogger.Log($"Provided {_ingredientToProvide.DisplayName} to player {player.OwnerClientId}");
        }

        /// <summary>
        /// Handle cooldown changed event.
        /// </summary>
        private void OnCooldownChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Update the visuals based on the current state.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_availableVisual != null)
            {
                _availableVisual.SetActive(!_isOnCooldown.Value);
            }

            if (_cooldownVisual != null)
            {
                _cooldownVisual.SetActive(_isOnCooldown.Value);
            }
        }

        private void ResolveIngredientSpawner(bool logIfMissing = true)
        {
            _ingredientNetworkSpawner ??= _matchContext?.IngredientNetworkSpawner;

            if (logIfMissing && _ingredientNetworkSpawner == null)
            {
                GameLogger.LogWarning("[IngredientCrate] IngredientNetworkSpawner is not available from match context yet.");
            }
        }
    }
}
