using System;
using Gameplay.Characters;
using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// Base class for stations that process ingredients over time (e.g., Stove, Cutting Board).
    /// </summary>
    public abstract class ProcessingStation : StationBase
    {
        [Header("Processing Settings")]
        [SerializeField] protected float _processingTime = 3f;
        [SerializeField] protected GameObject _progressBarPrefab;
        [SerializeField] protected AudioClip _startProcessingSound;
        [SerializeField] protected AudioClip _finishProcessingSound;

        /// <summary>
        /// Event triggered when processing starts.
        /// </summary>
        public event Action<ProcessingStation, IngredientItem> OnProcessingStarted;

        /// <summary>
        /// Event triggered when processing completes.
        /// </summary>
        public event Action<ProcessingStation, IngredientItem> OnProcessingCompleted;

        /// <summary>
        /// Event triggered when processing is canceled.
        /// </summary>
        public event Action<ProcessingStation> OnProcessingCanceled;

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
        /// The network variable for processing state.
        /// </summary>
        protected NetworkVariable<bool> _isProcessingNetVar = new NetworkVariable<bool>(false);

        /// <summary>
        /// The network variable for processing progress.
        /// </summary>
        protected NetworkVariable<float> _processingProgressNetVar = new NetworkVariable<float>(0f);

        /// <summary>
        /// Initialize the processing station.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

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
        /// Update the processing station.
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
        /// Handle specific processing interaction.
        /// </summary>
        protected override void HandleInteraction(PlayerController player)
        {
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

                    // Update station state
                    if (_networkController != null)
                    {
                        _networkController.SetState(StationState.Processing);
                    }
                }
                else
                {
                    // Release station if ingredient can't be processed
                    if (_networkController != null)
                    {
                        _networkController.ReleaseStationServerRpc(player.OwnerClientId);
                    }
                }
            }
            // If the station has a processed ingredient
            else if (_currentIngredient != null && !_isProcessing)
            {
                // Give the ingredient to the player
                if (player.PickUpObject(_currentIngredient.gameObject))
                {
                    _currentIngredient = null;

                    // Update station state
                    if (_networkController != null)
                    {
                        _networkController.SetState(StationState.Idle);
                    }
                }
            }
        }

        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        public override string GetInteractionPrompt()
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
                return base.GetInteractionPrompt();
            }
        }

        /// <summary>
        /// Check if the station can be interacted with.
        /// </summary>
        public override bool CanInteract(PlayerController player)
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
        protected abstract bool CanProcessIngredient(IngredientItem ingredientItem);

        /// <summary>
        /// Process the ingredient.
        /// </summary>
        protected abstract bool ProcessIngredient(IngredientItem ingredientItem);

        /// <summary>
        /// Place an ingredient on the station.
        /// </summary>
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

            // Update station state
            if (_networkController != null)
            {
                _networkController.SetState(success ? StationState.Complete : StationState.Error);
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
    }
}
