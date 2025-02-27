using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station where players can cook ingredients
    /// </summary>
    public class CookingStation : NetworkBehaviour, IInteractable
    {
        #region Events
        public event Action<float> OnCookingProgressChanged;
        public event Action<CookingResult> OnCookingCompleted;
        public event Action<CookingState> OnCookingStateChanged;
        #endregion

        #region Properties
        public bool CanInteract => !_isOccupied && !_isCooking;
        public InteractionType InteractionType => InteractionType.Cook;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        public CookingState CurrentCookingState => _currentCookingState;
        #endregion

        #region Serialized Fields
        [Header("Cooking Settings")]
        [SerializeField] private CookingMethod _cookingMethod;
        [SerializeField] private List<IngredientData> _ingredientDatabase;
        [SerializeField] private Transform _cookingPoint;
        [SerializeField] private Transform _outputPoint;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject _cookingEffectPrefab;
        [SerializeField] private ParticleSystem _burningEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _startCookingSound;
        [SerializeField] private AudioClip _cookingCompleteSound;
        [SerializeField] private AudioClip _burningSound;
        #endregion

        #region Private Fields
        private bool _isOccupied;
        private bool _isCooking;
        private float _currentCookingTime;
        private GameObject _currentIngredient;
        private Ingredient _currentIngredientComponent;
        private AudioSource _audioSource;
        private ParticleSystem _cookingEffect;
        private CookingState _currentCookingState = CookingState.Raw;
        private PlayerController _currentPlayer;
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private CookingValidator _cookingValidator;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _cookingValidator = new CookingValidator(_ingredientDatabase);
            
            if (_cookingEffectPrefab != null)
            {
                var effectObj = Instantiate(_cookingEffectPrefab, _cookingPoint);
                _cookingEffect = effectObj.GetComponent<ParticleSystem>();
                _cookingEffect.Stop();
            }
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
        }

        public override void OnNetworkDespawn()
        {
            _isBeingUsed.OnValueChanged -= OnBeingUsedChanged;
        }
        #endregion

        #region IInteractable Implementation
        public bool StartInteraction(PlayerController player, Action onComplete)
        {
            if (!CanInteract || !IsServer)
                return false;

            _currentPlayer = player;
            GameObject heldItem = player.HeldItem;
            
            if (heldItem != null)
            {
                var ingredient = heldItem.GetComponent<Ingredient>();
                if (ingredient != null && ValidateIngredient(ingredient))
                {
                    StartCookingServerRpc(player.NetworkObjectId);
                    return true;
                }
            }
            
            return false;
        }

        public void CancelInteraction(PlayerController player)
        {
            if (_currentPlayer == player)
            {
                CancelCookingServerRpc();
            }
        }

        public bool ContinueInteraction(PlayerController player)
        {
            if (!_isCooking || _currentPlayer != null)
                return false;

            _currentPlayer = player;
            return true;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void StartCookingServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;
            StartCookingClientRpc(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CancelCookingServerRpc()
        {
            if (!_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = false;
            CancelCookingClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCookingStateServerRpc(CookingState newState)
        {
            UpdateCookingStateClientRpc(newState);
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void StartCookingClientRpc(ulong playerId)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            
            if (player != null)
            {
                _currentPlayer = player;
                _currentIngredient = player.HeldItem;
                _currentIngredientComponent = _currentIngredient.GetComponent<Ingredient>();
                
                InitializeCooking();
            }
        }

        [ClientRpc]
        private void CancelCookingClientRpc()
        {
            StopCooking(false);
        }

        [ClientRpc]
        private void UpdateCookingStateClientRpc(CookingState newState)
        {
            UpdateCookingState(newState);
        }
        #endregion

        #region Private Methods
        private void InitializeCooking()
        {
            _isOccupied = true;
            _isCooking = true;
            CurrentState = InteractionState.InProgress;
            
            // Position the ingredient
            _currentIngredient.transform.SetParent(_cookingPoint);
            _currentIngredient.transform.localPosition = Vector3.zero;
            _currentIngredient.transform.localRotation = Quaternion.identity;

            // Start effects
            if (_cookingEffect != null)
            {
                _cookingEffect.Play();
            }

            if (_audioSource != null && _startCookingSound != null)
            {
                _audioSource.PlayOneShot(_startCookingSound);
            }

            // Start cooking coroutine
            StartCoroutine(CookingCoroutine());
        }

        private IEnumerator CookingCoroutine()
        {
            _currentCookingTime = 0f;
            float cookingTime = _cookingValidator.GetCookingTime(_currentIngredientComponent.IngredientId);
            float burningThreshold = _cookingValidator.GetBurningThreshold(_currentIngredientComponent.IngredientId);
            float totalTime = cookingTime + burningThreshold;
            
            while (_isCooking && _currentCookingTime < totalTime)
            {
                _currentCookingTime += Time.deltaTime;
                float progress = _currentCookingTime / cookingTime;
                OnCookingProgressChanged?.Invoke(progress);

                // Update cooking state
                CookingState newState = _cookingValidator.GetCookingState(
                    _currentIngredientComponent.IngredientId,
                    _currentCookingTime
                );
                
                if (newState != _currentCookingState)
                {
                    UpdateCookingStateServerRpc(newState);
                }
                
                yield return null;
            }

            if (_isCooking)
            {
                CompleteCooking();
            }
        }

        private void UpdateCookingState(CookingState newState)
        {
            if (_currentCookingState == newState)
                return;

            _currentCookingState = newState;
            
            // Update effects based on state
            switch (newState)
            {
                case CookingState.Burnt:
                    if (_burningEffect != null)
                        _burningEffect.Play();
                    if (_audioSource != null && _burningSound != null)
                        _audioSource.PlayOneShot(_burningSound);
                    break;
                    
                case CookingState.Cooked:
                    if (_audioSource != null && _cookingCompleteSound != null)
                        _audioSource.PlayOneShot(_cookingCompleteSound);
                    break;
            }
            
            OnCookingStateChanged?.Invoke(newState);
        }

        private void CompleteCooking()
        {
            StopCooking(true);
            
            var result = new CookingResult
            {
                FinalState = _currentCookingState,
                CookedItem = _currentIngredient,
                CookingTime = _currentCookingTime,
                WasInterrupted = false
            };

            OnCookingCompleted?.Invoke(result);
        }

        private void StopCooking(bool completed)
        {
            _isCooking = false;
            _isOccupied = false;
            CurrentState = completed ? InteractionState.Completed : InteractionState.Canceled;

            // Stop effects
            if (_cookingEffect != null)
                _cookingEffect.Stop();
            if (_burningEffect != null)
                _burningEffect.Stop();

            if (_audioSource != null && _cookingCompleteSound != null && completed)
            {
                _audioSource.PlayOneShot(_cookingCompleteSound);
            }

            // Move cooked item to output
            if (_currentIngredient != null)
            {
                _currentIngredient.transform.SetParent(_outputPoint);
                _currentIngredient.transform.localPosition = Vector3.zero;
            }

            _currentIngredient = null;
            _currentIngredientComponent = null;
            _currentPlayer = null;
            _currentCookingState = CookingState.Raw;
        }

        private bool ValidateIngredient(Ingredient ingredient)
        {
            return _cookingValidator.ValidateIngredient(ingredient.IngredientId, _cookingMethod);
        }

        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            // Update visual feedback when station's use state changes
            // TODO: Implement visual feedback
        }
        #endregion
    }

    /// <summary>
    /// Contains information about the cooking result
    /// </summary>
    public class CookingResult
    {
        public CookingState FinalState { get; set; }
        public GameObject CookedItem { get; set; }
        public float CookingTime { get; set; }
        public bool WasInterrupted { get; set; }
    }
} 