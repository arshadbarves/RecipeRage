using UnityEngine;
using System;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station that provides ingredients in specific states (raw, cooked, etc.)
    /// </summary>
    public class ContainerStation : NetworkBehaviour, IInteractable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value;
        public InteractionType InteractionType => InteractionType.Container;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        #endregion

        #region Serialized Fields
        [Header("Container Settings")]
        [SerializeField] private IngredientData _ingredientData;
        [SerializeField] private CookingState _providedState = CookingState.Raw;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private GameObject _ingredientPrefab;
        [SerializeField] private int _maxQuantity = -1; // -1 for infinite

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _highlightEffect;
        [SerializeField] private GameObject _emptyEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip _takeIngredientSound;
        [SerializeField] private AudioClip _emptySound;
        #endregion

        #region Private Fields
        private AudioSource _audioSource;
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private NetworkVariable<int> _remainingQuantity = new NetworkVariable<int>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_maxQuantity > 0)
            {
                _remainingQuantity.Value = _maxQuantity;
            }
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
            if (_remainingQuantity != null)
            {
                _remainingQuantity.OnValueChanged += OnQuantityChanged;
            }
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isBeingUsed.OnValueChanged -= OnBeingUsedChanged;
            if (_remainingQuantity != null)
            {
                _remainingQuantity.OnValueChanged -= OnQuantityChanged;
            }
        }
        #endregion

        #region IInteractable Implementation
        public bool StartInteraction(PlayerController player, Action onComplete)
        {
            if (!CanInteract || !IsServer)
                return false;

            if (_maxQuantity > 0 && _remainingQuantity.Value <= 0)
            {
                PlayEmptySound();
                return false;
            }

            if (player.HeldItem != null)
                return false;

            SpawnIngredientServerRpc(player.NetworkObjectId);
            return true;
        }

        public void CancelInteraction(PlayerController player)
        {
            // Container interactions are instant, no need to cancel
        }

        public bool ContinueInteraction(PlayerController player)
        {
            // Container interactions are instant, no need to continue
            return false;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void SpawnIngredientServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;

            // Spawn the ingredient
            GameObject ingredient = Instantiate(_ingredientPrefab, _spawnPoint.position, _spawnPoint.rotation);
            NetworkObject networkObj = ingredient.GetComponent<NetworkObject>();
            networkObj.Spawn();

            // Set the ingredient state
            Ingredient ingredientComponent = ingredient.GetComponent<Ingredient>();
            if (ingredientComponent != null)
            {
                ingredientComponent.UpdateStateServerRpc(_providedState);
            }

            // Update quantity if needed
            if (_maxQuantity > 0)
            {
                _remainingQuantity.Value--;
            }

            // Give to player
            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PickupItem(ingredient);
            }

            SpawnIngredientClientRpc();
            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void SpawnIngredientClientRpc()
        {
            PlayTakeSound();
        }
        #endregion

        #region Private Methods
        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        private void OnQuantityChanged(int previousValue, int newValue)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_highlightEffect != null)
            {
                _highlightEffect.SetActive(CanInteract);
            }

            if (_emptyEffect != null && _maxQuantity > 0)
            {
                _emptyEffect.SetActive(_remainingQuantity.Value <= 0);
            }
        }

        private void PlayTakeSound()
        {
            if (_audioSource != null && _takeIngredientSound != null)
            {
                _audioSource.PlayOneShot(_takeIngredientSound);
            }
        }

        private void PlayEmptySound()
        {
            if (_audioSource != null && _emptySound != null)
            {
                _audioSource.PlayOneShot(_emptySound);
            }
        }
        #endregion
    }
} 