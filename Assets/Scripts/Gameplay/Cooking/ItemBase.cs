using Core.Characters;
using Modules.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Base class for all interactive items (ingredients, plates, tools).
    /// Handles physical presence, network ownership (Pickup/Drop), and basic interaction.
    /// </summary>
    public abstract class ItemBase : NetworkBehaviour, IInteractable
    {
        [Header("Item Visuals")]
        [SerializeField] protected Collider _collider;
        [SerializeField] protected Renderer _renderer;

        /// <summary>
        /// Whether the item is being held by a player.
        /// </summary>
        protected NetworkVariable<bool> _isHeld = new NetworkVariable<bool>(false);

        /// <summary>
        /// The ID of the player holding this item.
        /// </summary>
        protected NetworkVariable<ulong> _heldByPlayerId = new NetworkVariable<ulong>(ulong.MaxValue);

        public override void OnNetworkSpawn()
        {
            _isHeld.OnValueChanged += OnIsHeldChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isHeld.OnValueChanged -= OnIsHeldChanged;
        }

        /// <summary>
        /// Called when a player interacts with this item.
        /// Default behavior is to request Pickup or Drop.
        /// </summary>
        public virtual void Interact(PlayerController player)
        {
            RequestPickupDropServerRpc(player.NetworkObject);
        }

        /// <summary>
        /// Pick up the item. Server only.
        /// </summary>
        public virtual void PickUp(ulong playerId)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can pick up items.");
                return;
            }

            _isHeld.Value = true;
            _heldByPlayerId.Value = playerId;
        }

        /// <summary>
        /// Drop the item. Server only.
        /// </summary>
        public virtual void Drop()
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only the server can drop items.");
                return;
            }

            _isHeld.Value = false;
            _heldByPlayerId.Value = ulong.MaxValue;
        }

        public virtual string GetInteractionPrompt()
        {
            return _isHeld.Value ? "Drop" : "Pick Up";
        }

        public virtual bool CanInteract(PlayerController player)
        {
            // If held, only the owner can interact (to drop)
            if (_isHeld.Value)
            {
                return _heldByPlayerId.Value == player.OwnerClientId;
            }
            // If not held, anyone can interact (to pick up)
            return true;
        }

        protected virtual void OnIsHeldChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Update collider/visuals based on held state.
        /// </summary>
        protected virtual void UpdateVisuals()
        {
            if (_collider != null)
            {
                _collider.enabled = !_isHeld.Value;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void RequestPickupDropServerRpc(NetworkObjectReference playerNetworkObject)
        {
            if (playerNetworkObject.TryGet(out NetworkObject networkObject))
            {
                PlayerController player = networkObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    ulong playerId = player.OwnerClientId;
                    if (_isHeld.Value)
                    {
                        if (_heldByPlayerId.Value == playerId) Drop();
                    }
                    else
                    {
                        PickUp(playerId);
                    }
                }
            }
        }
    }
}
