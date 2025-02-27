using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// A plate that can hold multiple ingredients for serving
    /// </summary>
    public class Plate : NetworkBehaviour, IInteractable, IPickupable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value;
        public InteractionType InteractionType => InteractionType.Plate;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        public int IngredientCount => _ingredients.Count;
        #endregion

        #region Serialized Fields
        [Header("Plate Settings")]
        [SerializeField] private int _maxIngredients = 4;
        [SerializeField] private Transform _ingredientsParent;
        [SerializeField] private List<Transform> _ingredientSlots;
        [SerializeField] private bool _allowSameIngredients = true;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _highlightEffect;
        [SerializeField] private GameObject _fullEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip _addIngredientSound;
        [SerializeField] private AudioClip _removeIngredientSound;
        [SerializeField] private AudioClip _fullSound;
        #endregion

        #region Private Fields
        private AudioSource _audioSource;
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private readonly List<GameObject> _ingredients = new();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_ingredientsParent != null && _ingredientSlots.Count == 0)
            {
                // Auto-generate slots if not manually set
                for (int i = 0; i < _maxIngredients; i++)
                {
                    var slot = new GameObject($"Slot_{i}").transform;
                    slot.SetParent(_ingredientsParent);
                    slot.localPosition = new Vector3(i * 0.3f, 0.1f, 0); // Adjust spacing as needed
                    _ingredientSlots.Add(slot);
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
            UpdateVisuals();
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

            if (player.HeldItem != null)
            {
                // Try to add ingredient
                if (_ingredients.Count >= _maxIngredients)
                {
                    PlayFullSound();
                    return false;
                }

                var ingredient = player.HeldItem.GetComponent<Ingredient>();
                if (ingredient == null)
                {
                    return false;
                }

                if (!_allowSameIngredients && _ingredients.Any(item => 
                    item.GetComponent<Ingredient>()?.IngredientId == ingredient.IngredientId))
                {
                    return false;
                }

                AddIngredientServerRpc(player.NetworkObjectId);
                return true;
            }
            else if (_ingredients.Count > 0)
            {
                // Try to remove last ingredient
                RemoveIngredientServerRpc(player.NetworkObjectId);
                return true;
            }

            return false;
        }

        public void CancelInteraction(PlayerController player)
        {
            // Plate interactions are instant, no need to cancel
        }

        public bool ContinueInteraction(PlayerController player)
        {
            // Plate interactions are instant, no need to continue
            return false;
        }
        #endregion

        #region IPickupable Implementation
        public GameObject GetGameObject()
        {
            return gameObject;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void AddIngredientServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem != null)
            {
                var item = player.HeldItem;
                int slotIndex = _ingredients.Count;
                if (slotIndex < _ingredientSlots.Count)
                {
                    item.transform.SetParent(_ingredientSlots[slotIndex]);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    _ingredients.Add(item);
                    player.HeldItem = null;
                }
            }

            AddIngredientClientRpc();
            _isBeingUsed.Value = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveIngredientServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value || _ingredients.Count == 0)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem == null)
            {
                var item = _ingredients[^1]; // Take last ingredient
                _ingredients.RemoveAt(_ingredients.Count - 1);
                player.PickupItem(item);
            }

            RemoveIngredientClientRpc();
            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void AddIngredientClientRpc()
        {
            PlayAddSound();
            UpdateVisuals();
        }

        [ClientRpc]
        private void RemoveIngredientClientRpc()
        {
            PlayRemoveSound();
            UpdateVisuals();
        }
        #endregion

        #region Private Methods
        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_highlightEffect != null)
            {
                _highlightEffect.SetActive(CanInteract && _ingredients.Count < _maxIngredients);
            }

            if (_fullEffect != null)
            {
                _fullEffect.SetActive(_ingredients.Count >= _maxIngredients);
            }
        }

        private void PlayAddSound()
        {
            if (_audioSource != null && _addIngredientSound != null)
            {
                _audioSource.PlayOneShot(_addIngredientSound);
            }
        }

        private void PlayRemoveSound()
        {
            if (_audioSource != null && _removeIngredientSound != null)
            {
                _audioSource.PlayOneShot(_removeIngredientSound);
            }
        }

        private void PlayFullSound()
        {
            if (_audioSource != null && _fullSound != null)
            {
                _audioSource.PlayOneShot(_fullSound);
            }
        }
        #endregion
    }
} 