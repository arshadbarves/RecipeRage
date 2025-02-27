using UnityEngine;
using System;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// A station where players can dispose of unwanted items
    /// </summary>
    public class TrashStation : BaseStation, IInteractable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value;
        public InteractionType InteractionType => InteractionType.Trash;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        #endregion

        #region Serialized Fields
        [Header("Audio")]
        [SerializeField] private AudioClip _trashSound;
        #endregion

        #region Private Fields
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
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

            if (player.HeldItem == null)
                return false;

            TrashItemServerRpc(player.NetworkObjectId);
            return true;
        }

        public void CancelInteraction(PlayerController player)
        {
            // Trash interactions are instant, no need to cancel
        }

        public bool ContinueInteraction(PlayerController player)
        {
            // Trash interactions are instant, no need to continue
            return false;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void TrashItemServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;

            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null && player.HeldItem != null)
            {
                var item = player.HeldItem;
                player.HeldItem = null;
                
                // Destroy the item
                if (item.TryGetComponent<NetworkObject>(out var networkObj))
                {
                    networkObj.Despawn();
                }
                Destroy(item);
            }

            TrashItemClientRpc();
            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void TrashItemClientRpc()
        {
            PlayTrashEffect();
            PlayTrashSound();
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
            SetHighlight(CanInteract);
        }

        private void PlayTrashEffect()
        {
            PlayParticles();
        }

        private void PlayTrashSound()
        {
            PlaySound(_trashSound);
        }
        #endregion
    }
} 