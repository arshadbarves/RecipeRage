using UnityEngine;
using System;
using System.Collections.Generic;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A container that can store a single item
    /// </summary>
    public class ItemContainer : NetworkBehaviour, IInteractable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value;
        public InteractionType InteractionType => InteractionType.Container;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        #endregion

        #region Serialized Fields
        [Header("Container Settings")]
        [SerializeField] private Transform _itemSlot;
        [SerializeField] private bool _acceptPlatesOnly;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _highlightEffect;
        [SerializeField] private GameObject _occupiedEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip _placeItemSound;
        [SerializeField] private AudioClip _takeItemSound;
        [SerializeField] private AudioClip _invalidItemSound;
        #endregion

        #region Private Fields
        private AudioSource _audioSource;
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private GameObject _storedItem;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_itemSlot == null)
            {
                // Create default item slot if not set
                var slot = new GameObject("ItemSlot").transform;
                slot.SetParent(transform);
                slot.localPosition = Vector3.zero;
                _itemSlot = slot;
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
                // Try to place item
                if (_storedItem != null)
                {
                    PlayInvalidSound();
                    return false;
                }

                if (_acceptPlatesOnly && player.HeldItem.GetComponent<Plate>() == null)
                {
                    PlayInvalidSound();
                    return false;
                }

                PlaceItemServerRpc(player.NetworkObjectId);
                return true;
            }
            else if (_storedItem != null)
            {
                // Try to take item
                TakeItemServerRpc(player.NetworkObjectId);
                return true;
            }

            return false;
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
        private void PlaceItemServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value || _storedItem != null)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem != null)
            {
                var item = player.HeldItem;
                item.transform.SetParent(_itemSlot);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                _storedItem = item;
                player.HeldItem = null;
            }

            PlaceItemClientRpc();
            _isBeingUsed.Value = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeItemServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value || _storedItem == null)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem == null)
            {
                player.PickupItem(_storedItem);
                _storedItem = null;
            }

            TakeItemClientRpc();
            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void PlaceItemClientRpc()
        {
            PlayPlaceSound();
            UpdateVisuals();
        }

        [ClientRpc]
        private void TakeItemClientRpc()
        {
            PlayTakeSound();
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
                _highlightEffect.SetActive(CanInteract && _storedItem == null);
            }

            if (_occupiedEffect != null)
            {
                _occupiedEffect.SetActive(_storedItem != null);
            }
        }

        private void PlayPlaceSound()
        {
            if (_audioSource != null && _placeItemSound != null)
            {
                _audioSource.PlayOneShot(_placeItemSound);
            }
        }

        private void PlayTakeSound()
        {
            if (_audioSource != null && _takeItemSound != null)
            {
                _audioSource.PlayOneShot(_takeItemSound);
            }
        }

        private void PlayInvalidSound()
        {
            if (_audioSource != null && _invalidItemSound != null)
            {
                _audioSource.PlayOneShot(_invalidItemSound);
            }
        }
        #endregion
    }
} 