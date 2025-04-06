using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
{
    /// <summary>
    /// A station that spawns ingredients.
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
        /// Initialize the ingredient spawner.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = $"{_ingredientToSpawn?.DisplayName ?? "Ingredient"} Spawner";
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
        public override void Interact(RecipeRage.Core.Characters.PlayerController player)
        {
            if (!IsServer)
            {
                // Request interaction from the server
                InteractServerRpc();
                return;
            }

            // Check if the spawner is on cooldown
            if (_isOnCooldown.Value)
            {
                return;
            }

            // Check if the player is already holding an item
            if (player.IsHoldingItem())
            {
                return;
            }

            // Spawn the ingredient
            SpawnIngredient(player);

            // Start cooldown
            _isOnCooldown.Value = true;
            _cooldownTimer = _spawnCooldown;
        }

        /// <summary>
        /// Spawn an ingredient.
        /// </summary>
        /// <param name="player">The player to give the ingredient to</param>
        private void SpawnIngredient(RecipeRage.Core.Characters.PlayerController player)
        {
            if (_ingredientToSpawn == null)
            {
                Debug.LogWarning("[IngredientSpawner] No ingredient to spawn");
                return;
            }

            // Create the ingredient prefab
            GameObject ingredientPrefab = _ingredientToSpawn.Prefab;
            if (ingredientPrefab == null)
            {
                Debug.LogWarning("[IngredientSpawner] Ingredient prefab is null");
                return;
            }

            // Instantiate the ingredient
            GameObject ingredientObject = Instantiate(ingredientPrefab, _ingredientPlacementPoint.position, Quaternion.identity);

            // Get the ingredient item component
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                Debug.LogWarning("[IngredientSpawner] Ingredient item component not found");
                Destroy(ingredientObject);
                return;
            }

            // Set the ingredient
            ingredientItem.SetIngredient(_ingredientToSpawn);

            // Spawn the ingredient on the network
            NetworkObject networkObject = ingredientObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();

                // Give the ingredient to the player
                player.PickupItem(networkObject);
            }
            else
            {
                Debug.LogWarning("[IngredientSpawner] Network object component not found");
                Destroy(ingredientObject);
            }

            // Play sound
            if (_audioSource != null && _startProcessingSound != null)
            {
                _audioSource.clip = _startProcessingSound;
                _audioSource.Play();
            }
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
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc()
        {
            // Get the local player
            var player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RecipeRage.Core.Characters.PlayerController>();

            // Interact with the spawner
            Interact(player);
        }
    }
}
