using Core.Characters;
using Core.Logging;
using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// A station that spawns ingredients.
    /// Uses IngredientNetworkSpawner for efficient network object pooling.
    /// </summary>
    public class IngredientSpawner : CookingStation
    {
        [Header("Spawner Settings")]
        [SerializeField] private Ingredient _ingredientToSpawn;
        [SerializeField] private float _spawnCooldown = 3f;
        [SerializeField] private GameObject _availableVisual;
        [SerializeField] private GameObject _cooldownVisual;

        /// <summary>
        /// Whether the spawner is currently on cooldown.
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

        /// <summary>
        /// Initialize the ingredient spawner.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = $"{_ingredientToSpawn?.DisplayName ?? "Ingredient"} Spawner";

            // Find the network ingredient spawner service
            _ingredientNetworkSpawner = FindObjectOfType<IngredientNetworkSpawner>();
            if (_ingredientNetworkSpawner == null)
            {
                GameLogger.LogWarning("IngredientNetworkSpawner not found in scene. Ingredient spawning may not work properly.");
            }
        }

        /// <summary>
        /// Set up network variables when the network object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

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

        /// <summary>
        /// Update the ingredient spawner.
        /// </summary>
        protected override void Update()
        {
            base.Update();

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
        /// Process the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        /// <returns>True if the ingredient was processed successfully</returns>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            // Ingredient spawners don't process ingredients
            return false;
        }

        /// <summary>
        /// Check if the ingredient can be processed by this station.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be processed</returns>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Ingredient spawners don't process ingredients
            return false;
        }

        /// <summary>
        /// Interact with the ingredient spawner.
        /// </summary>
        /// <param name="player">The player that is interacting</param>
        public override void Interact(PlayerController player)
        {
            if (!IsServer)
            {
                // Request interaction from the server
                InteractServerRpc(player.NetworkObject);
                return;
            }

            // Check if the spawner is on cooldown
            if (_isOnCooldown.Value)
            {
                GameLogger.Log($"Spawner is on cooldown ({_cooldownTimer:F1}s remaining)");
                return;
            }

            // Check if the player is already holding an item
            if (player.IsHoldingObject())
            {
                GameLogger.Log("Player is already holding an item");
                return;
            }

            // Check if player can use this station (network controller check)
            if (_networkController != null && !_networkController.CanPlayerUse(player.OwnerClientId))
            {
                GameLogger.Log("Station is locked by another player");
                return;
            }

            // Spawn the ingredient
            SpawnIngredient(player);

            // Start cooldown
            _isOnCooldown.Value = true;
            _cooldownTimer = _spawnCooldown;
        }

        /// <summary>
        /// Spawn an ingredient using the network spawner service.
        /// </summary>
        /// <param name="player">The player to give the ingredient to</param>
        private void SpawnIngredient(PlayerController player)
        {
            if (_ingredientToSpawn == null)
            {
                GameLogger.LogWarning("No ingredient to spawn");
                return;
            }

            // Check if we have the network spawner service
            if (_ingredientNetworkSpawner == null)
            {
                GameLogger.LogError("IngredientNetworkSpawner service not available. Cannot spawn ingredient.");
                return;
            }

            // Use the network spawner service to spawn the ingredient
            // This uses object pooling for better performance
            Vector3 spawnPosition = _ingredientPlacementPoint != null
                ? _ingredientPlacementPoint.position
                : transform.position + Vector3.up;

            NetworkObject ingredientNetworkObject = _ingredientNetworkSpawner.SpawnIngredient(_ingredientToSpawn, spawnPosition);

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
            if (_audioSource != null && _startProcessingSound != null)
            {
                _audioSource.clip = _startProcessingSound;
                _audioSource.Play();
            }

            GameLogger.Log($"Spawned {_ingredientToSpawn.DisplayName} for player {player.OwnerClientId}");
        }

        /// <summary>
        /// Handle cooldown changed event.
        /// </summary>
        /// <param name="previousValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        private void OnCooldownChanged(bool previousValue, bool newValue)
        {
            // Update visuals
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

        /// <summary>
        /// Request interaction from the server.
        /// </summary>
        /// <param name="playerNetworkObject">The player's network object</param>
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc(NetworkObjectReference playerNetworkObject)
        {
            // Get the player controller
            if (playerNetworkObject.TryGet(out NetworkObject networkObject))
            {
                PlayerController player = networkObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Interact with the spawner
                    Interact(player);
                }
                else
                {
                    GameLogger.LogWarning("Player controller not found on network object");
                }
            }
            else
            {
                GameLogger.LogWarning("Failed to get network object from reference");
            }
        }
    }
}
