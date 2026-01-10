using Gameplay.Characters;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Stations
{
    /// <summary>
    /// Base class for all gameplay stations.
    /// Handles network locking, interaction basics, and audio references.
    /// </summary>
    [RequireComponent(typeof(StationNetworkController))]
    public abstract class StationBase : NetworkBehaviour, IInteractable
    {
        [Header("Station Base Settings")]
        [SerializeField] protected string _stationName = "Station";
        [SerializeField] protected Transform _ingredientPlacementPoint;
        [SerializeField] protected AudioClip _interactSound;

        /// <summary>
        /// The audio source for station sounds.
        /// </summary>
        protected AudioSource _audioSource;

        /// <summary>
        /// The station network controller for managing station access.
        /// </summary>
        protected StationNetworkController _networkController;

        protected virtual void Awake()
        {
            // Get components
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Get network controller
            _networkController = GetComponent<StationNetworkController>();
            if (_networkController == null)
            {
                GameLogger.LogWarning($"{_stationName} is missing StationNetworkController component. Station locking will not work.");
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

            // Check if player can use this station (not locked by another player)
            if (_networkController != null && !_networkController.CanPlayerUse(player.OwnerClientId))
            {
                GameLogger.Log($"{_stationName} is locked by another player");
                return;
            }

            // Lock the station for this player
            if (_networkController != null)
            {
                _networkController.RequestUseStationServerRpc(player.OwnerClientId);
            }

            // Perform the specific station interaction
            HandleInteraction(player);

            // Release the station lock (unless the specific interaction needs to keep it held)
            // Note: Some complex interactions might want to manually release this later, 
            // but for safety we release here. Subclasses can override Interact if they need manual lock control.
            if (_networkController != null)
            {
                _networkController.ReleaseStationServerRpc(player.OwnerClientId);
            }
        }

        /// <summary>
        /// Specific interaction logic to be implemented by subclasses.
        /// Executed only on the Server and only if the station is not locked.
        /// </summary>
        /// <param name="player">The player interacting</param>
        protected abstract void HandleInteraction(PlayerController player);

        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        public virtual string GetInteractionPrompt()
        {
            return $"Use {_stationName}";
        }

        /// <summary>
        /// Check if the station can be interacted with.
        /// </summary>
        public virtual bool CanInteract(PlayerController player)
        {
            return true;
        }

        /// <summary>
        /// Request interaction from the server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        protected void InteractServerRpc(NetworkObjectReference playerNetworkObject)
        {
            if (playerNetworkObject.TryGet(out NetworkObject networkObject))
            {
                PlayerController player = networkObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    Interact(player);
                }
            }
        }
    }
}
