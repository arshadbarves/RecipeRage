using RecipeRage.Core;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A spawner for ingredients.
    /// </summary>
    public class IngredientSpawner : NetworkBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private Ingredient _ingredientToSpawn;
        [SerializeField] private float _spawnCooldown = 3f;
        [SerializeField] private Transform _spawnPoint;
        
        [Header("Visual Feedback")]
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
        public override void OnNetworkSpawn()
        {
            // Subscribe to state changes
            _isOnCooldown.OnValueChanged += OnIsOnCooldownChanged;
            
            // Initialize visual state
            UpdateVisuals();
        }
        
        /// <summary>
        /// Clean up when the network object is despawned.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribe from state changes
            _isOnCooldown.OnValueChanged -= OnIsOnCooldownChanged;
        }
        
        /// <summary>
        /// Update the ingredient spawner.
        /// </summary>
        private void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }
            
            // Update the cooldown timer
            if (_isOnCooldown.Value)
            {
                _cooldownTimer -= Time.deltaTime;
                
                if (_cooldownTimer <= 0f)
                {
                    // Reset the cooldown
                    _isOnCooldown.Value = false;
                }
            }
        }
        
        /// <summary>
        /// Spawn an ingredient.
        /// </summary>
        /// <returns>True if the ingredient was spawned successfully, false otherwise.</returns>
        public bool SpawnIngredient()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[IngredientSpawner] Only the server can spawn ingredients.");
                return false;
            }
            
            // Check if the spawner is on cooldown
            if (_isOnCooldown.Value)
            {
                return false;
            }
            
            // Check if the ingredient to spawn is set
            if (_ingredientToSpawn == null)
            {
                Debug.LogWarning("[IngredientSpawner] No ingredient to spawn.");
                return false;
            }
            
            // Instantiate the ingredient prefab
            GameObject ingredientObject = Instantiate(_ingredientToSpawn.Prefab);
            
            // Get the NetworkObject component
            NetworkObject networkObject = ingredientObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogWarning("[IngredientSpawner] Ingredient prefab does not have a NetworkObject component.");
                Destroy(ingredientObject);
                return false;
            }
            
            // Get the IngredientItem component
            IngredientItem ingredientItem = ingredientObject.GetComponent<IngredientItem>();
            if (ingredientItem == null)
            {
                Debug.LogWarning("[IngredientSpawner] Ingredient prefab does not have an IngredientItem component.");
                Destroy(ingredientObject);
                return false;
            }
            
            // Position the ingredient
            if (_spawnPoint != null)
            {
                ingredientObject.transform.position = _spawnPoint.position;
                ingredientObject.transform.rotation = _spawnPoint.rotation;
            }
            else
            {
                ingredientObject.transform.position = transform.position + Vector3.up * 0.5f;
                ingredientObject.transform.rotation = Quaternion.identity;
            }
            
            // Spawn the ingredient
            networkObject.Spawn();
            
            // Set the ingredient data
            ingredientItem.SetIngredient(_ingredientToSpawn);
            
            // Start the cooldown
            _isOnCooldown.Value = true;
            _cooldownTimer = _spawnCooldown;
            
            Debug.Log($"[IngredientSpawner] Spawned ingredient: {_ingredientToSpawn.DisplayName}");
            
            return true;
        }
        
        /// <summary>
        /// Handle changes to the cooldown state.
        /// </summary>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnIsOnCooldownChanged(bool previousValue, bool newValue)
        {
            // Update the visual representation
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update the visual representation of the ingredient spawner.
        /// </summary>
        private void UpdateVisuals()
        {
            // Update the available visual
            if (_availableVisual != null)
            {
                _availableVisual.SetActive(!_isOnCooldown.Value);
            }
            
            // Update the cooldown visual
            if (_cooldownVisual != null)
            {
                _cooldownVisual.SetActive(_isOnCooldown.Value);
            }
        }
        
        /// <summary>
        /// Called when a player interacts with this ingredient spawner.
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
            
            // Request to spawn an ingredient via RPC
            RequestSpawnIngredientServerRpc(playerController.OwnerClientId);
        }
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text.</returns>
        public string GetInteractionPrompt()
        {
            if (_isOnCooldown.Value)
            {
                return "Wait...";
            }
            else
            {
                return $"Take {_ingredientToSpawn.DisplayName}";
            }
        }
        
        /// <summary>
        /// Check if this object can be interacted with.
        /// </summary>
        /// <param name="interactor">The GameObject that is trying to interact with this object.</param>
        /// <returns>True if the object can be interacted with.</returns>
        public bool CanInteract(GameObject interactor)
        {
            // Check if the interactor is a player
            var playerController = interactor.GetComponent<Core.Player.PlayerController>();
            if (playerController == null)
            {
                return false;
            }
            
            // Check if the player is already holding an item
            if (playerController.IsHoldingItem())
            {
                return false;
            }
            
            // Check if the spawner is on cooldown
            if (_isOnCooldown.Value)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Request to spawn an ingredient.
        /// </summary>
        /// <param name="playerId">The ID of the player making the request.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnIngredientServerRpc(ulong playerId)
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
            
            // Check if the player is already holding an item
            if (playerController.IsHoldingItem())
            {
                return;
            }
            
            // Try to spawn an ingredient
            if (SpawnIngredient())
            {
                Debug.Log($"[IngredientSpawner] Player {playerId} spawned an ingredient.");
            }
            else
            {
                Debug.LogWarning($"[IngredientSpawner] Player {playerId} failed to spawn an ingredient.");
            }
        }
    }
}
