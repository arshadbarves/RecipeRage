using System;
using System.Collections;
using RecipeRage.Core.Characters;
using RecipeRage.Core.Patterns;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Stations
{
    /// <summary>
    /// Base class for all cooking stations in the game.
    /// </summary>
    public abstract class CookingStation : NetworkBehaviour, IInteractable
    {
        [Header("Station Settings")]
        [SerializeField] protected string _stationName = "Cooking Station";
        [SerializeField] protected float _processingTime = 3f;
        [SerializeField] protected Transform _ingredientPlacementPoint;
        [SerializeField] protected GameObject _progressBarPrefab;
        [SerializeField] protected AudioClip _startProcessingSound;
        [SerializeField] protected AudioClip _finishProcessingSound;
        
        /// <summary>
        /// Event triggered when processing starts.
        /// </summary>
        public event Action<CookingStation, IngredientItem> OnProcessingStarted;
        
        /// <summary>
        /// Event triggered when processing completes.
        /// </summary>
        public event Action<CookingStation, IngredientItem> OnProcessingCompleted;
        
        /// <summary>
        /// Event triggered when processing is canceled.
        /// </summary>
        public event Action<CookingStation> OnProcessingCanceled;
        
        /// <summary>
        /// The current ingredient being processed.
        /// </summary>
        protected IngredientItem _currentIngredient;
        
        /// <summary>
        /// The current processing progress (0-1).
        /// </summary>
        protected float _processingProgress;
        
        /// <summary>
        /// Whether the station is currently processing an ingredient.
        /// </summary>
        protected bool _isProcessing;
        
        /// <summary>
        /// The progress bar UI.
        /// </summary>
        protected GameObject _progressBar;
        
        /// <summary>
        /// The audio source for station sounds.
        /// </summary>
        protected AudioSource _audioSource;
        
        /// <summary>
        /// The network variable for processing state.
        /// </summary>
        protected NetworkVariable<bool> _isProcessingNetVar = new NetworkVariable<bool>(false);
        
        /// <summary>
        /// The network variable for processing progress.
        /// </summary>
        protected NetworkVariable<float> _processingProgressNetVar = new NetworkVariable<float>(0f);
        
        /// <summary>
        /// Initialize the cooking station.
        /// </summary>
        protected virtual void Awake()
        {
            // Get components
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Create progress bar if prefab is set
            if (_progressBarPrefab != null && _ingredientPlacementPoint != null)
            {
                _progressBar = Instantiate(_progressBarPrefab, _ingredientPlacementPoint.position + Vector3.up * 1.5f, Quaternion.identity);
                _progressBar.transform.SetParent(transform);
                _progressBar.SetActive(false);
            }
        }
        
        /// <summary>
        /// Set up network variables when the network object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Subscribe to network variable changes
            _isProcessingNetVar.OnValueChanged += OnIsProcessingChanged;
            _processingProgressNetVar.OnValueChanged += OnProcessingProgressChanged;
        }
        
        /// <summary>
        /// Clean up when the network object despawns.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            // Unsubscribe from network variable changes
            _isProcessingNetVar.OnValueChanged -= OnIsProcessingChanged;
            _processingProgressNetVar.OnValueChanged -= OnProcessingProgressChanged;
        }
        
        /// <summary>
        /// Update the cooking station.
        /// </summary>
        protected virtual void Update()
        {
            // Only update processing on the server
            if (!IsServer || !_isProcessing)
            {
                return;
            }
            
            // Update processing progress
            _processingProgress += Time.deltaTime / _processingTime;
            _processingProgressNetVar.Value = _processingProgress;
            
            // Check if processing is complete
            if (_processingProgress >= 1f)
            {
                CompleteProcessing();
            }
        }
        
        /// <summary>
        /// Handle interaction from a player.
        /// </summary>
        /// <param name="player">The player that is interacting</param>
        public virtual void Interact(PlayerController player)
        {
            if (!IsServer)
            {
                // Request interaction from the server
                InteractServerRpc(player.NetworkObject);
                return;
            }
            
            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();
                
                if (ingredientItem != null && CanProcessIngredient(ingredientItem))
                {
                    // Take the ingredient from the player
                    player.DropObject();
                    
                    // Place the ingredient on the station
                    PlaceIngredient(ingredientItem);
                    
                    // Start processing
                    StartProcessing(ingredientItem);
                }
            }
            // If the station has a processed ingredient
            else if (_currentIngredient != null && !_isProcessing)
            {
                // Give the ingredient to the player
                if (player.PickUpObject(_currentIngredient.gameObject))
                {
                    _currentIngredient = null;
                }
            }
        }
        
        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public virtual string GetInteractionPrompt()
        {
            if (_isProcessing)
            {
                return $"Processing... ({Mathf.FloorToInt(_processingProgress * 100)}%)";
            }
            else if (_currentIngredient != null)
            {
                return $"Take {_currentIngredient.Ingredient.DisplayName}";
            }
            else
            {
                return $"Use {_stationName}";
            }
        }
        
        /// <summary>
        /// Check if the station can be interacted with.
        /// </summary>
        /// <param name="player">The player that wants to interact</param>
        /// <returns>True if the station can be interacted with</returns>
        public virtual bool CanInteract(PlayerController player)
        {
            // Can't interact while processing
            if (_isProcessing)
            {
                return false;
            }
            
            // If the player is holding an ingredient
            if (player.IsHoldingObject())
            {
                GameObject heldObject = player.GetHeldObject();
                IngredientItem ingredientItem = heldObject.GetComponent<IngredientItem>();
                
                // Check if the ingredient can be processed
                if (ingredientItem != null)
                {
                    return CanProcessIngredient(ingredientItem);
                }
                
                return false;
            }
            
            // If the station has a processed ingredient
            return _currentIngredient != null && !_isProcessing;
        }
        
        /// <summary>
        /// Check if the ingredient can be processed by this station.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to check</param>
        /// <returns>True if the ingredient can be processed</returns>
        protected abstract bool CanProcessIngredient(IngredientItem ingredientItem);
        
        /// <summary>
        /// Process the ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        /// <returns>True if the ingredient was processed</returns>
        protected abstract bool ProcessIngredient(IngredientItem ingredientItem);
        
        /// <summary>
        /// Place an ingredient on the station.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to place</param>
        protected virtual void PlaceIngredient(IngredientItem ingredientItem)
        {
            if (_ingredientPlacementPoint == null)
            {
                return;
            }
            
            // Place the ingredient at the placement point
            ingredientItem.transform.SetParent(_ingredientPlacementPoint);
            ingredientItem.transform.localPosition = Vector3.zero;
            ingredientItem.transform.localRotation = Quaternion.identity;
            
            // Set the current ingredient
            _currentIngredient = ingredientItem;
        }
        
        /// <summary>
        /// Start processing an ingredient.
        /// </summary>
        /// <param name="ingredientItem">The ingredient to process</param>
        protected virtual void StartProcessing(IngredientItem ingredientItem)
        {
            if (_isProcessing || ingredientItem == null)
            {
                return;
            }
            
            // Set processing state
            _isProcessing = true;
            _isProcessingNetVar.Value = true;
            _processingProgress = 0f;
            _processingProgressNetVar.Value = 0f;
            
            // Show progress bar
            if (_progressBar != null)
            {
                _progressBar.SetActive(true);
            }
            
            // Play sound
            if (_audioSource != null && _startProcessingSound != null)
            {
                _audioSource.PlayOneShot(_startProcessingSound);
            }
            
            // Trigger event
            OnProcessingStarted?.Invoke(this, ingredientItem);
        }
        
        /// <summary>
        /// Complete processing an ingredient.
        /// </summary>
        protected virtual void CompleteProcessing()
        {
            if (!_isProcessing || _currentIngredient == null)
            {
                return;
            }
            
            // Process the ingredient
            bool success = ProcessIngredient(_currentIngredient);
            
            // Set processing state
            _isProcessing = false;
            _isProcessingNetVar.Value = false;
            _processingProgress = 0f;
            _processingProgressNetVar.Value = 0f;
            
            // Hide progress bar
            if (_progressBar != null)
            {
                _progressBar.SetActive(false);
            }
            
            // Play sound
            if (success && _audioSource != null && _finishProcessingSound != null)
            {
                _audioSource.PlayOneShot(_finishProcessingSound);
            }
            
            // Trigger event
            if (success)
            {
                OnProcessingCompleted?.Invoke(this, _currentIngredient);
            }
            else
            {
                OnProcessingCanceled?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Cancel processing an ingredient.
        /// </summary>
        protected virtual void CancelProcessing()
        {
            if (!_isProcessing)
            {
                return;
            }
            
            // Set processing state
            _isProcessing = false;
            _isProcessingNetVar.Value = false;
            _processingProgress = 0f;
            _processingProgressNetVar.Value = 0f;
            
            // Hide progress bar
            if (_progressBar != null)
            {
                _progressBar.SetActive(false);
            }
            
            // Trigger event
            OnProcessingCanceled?.Invoke(this);
        }
        
        /// <summary>
        /// Handle changes to the processing state network variable.
        /// </summary>
        /// <param name="previousValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        private void OnIsProcessingChanged(bool previousValue, bool newValue)
        {
            // Update local processing state
            _isProcessing = newValue;
            
            // Show/hide progress bar
            if (_progressBar != null)
            {
                _progressBar.SetActive(newValue);
            }
        }
        
        /// <summary>
        /// Handle changes to the processing progress network variable.
        /// </summary>
        /// <param name="previousValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        private void OnProcessingProgressChanged(float previousValue, float newValue)
        {
            // Update local processing progress
            _processingProgress = newValue;
            
            // Update progress bar
            if (_progressBar != null)
            {
                // TODO: Update progress bar UI
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
                    // Handle interaction
                    Interact(player);
                }
            }
        }
    }
}
