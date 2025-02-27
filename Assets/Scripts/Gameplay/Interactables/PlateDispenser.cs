using UnityEngine;
using System;
using RecipeRage.Core.Player;
using RecipeRage.Core.Interaction;
using RecipeRage.Gameplay.Cooking;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// Dispenses clean plates for players to use
    /// </summary>
    public class PlateDispenser : BaseStation, IInteractable
    {
        #region Properties
        public bool CanInteract => !_isBeingUsed.Value && (_maxPlates <= 0 || _platesRemaining.Value > 0);
        public InteractionType InteractionType => InteractionType.Container;
        public InteractionState CurrentState { get; private set; } = InteractionState.Idle;
        #endregion

        #region Serialized Fields
        [Header("Dispenser Settings")]
        [SerializeField] private GameObject _platePrefab;
        [SerializeField] private int _maxPlates = -1; // -1 for infinite
        [SerializeField] private float _respawnTime = 3f;
        [SerializeField] private Transform _spawnPoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _dispensePlateSound;
        [SerializeField] private AudioClip _emptySound;
        #endregion

        #region Private Fields
        private NetworkVariable<bool> _isBeingUsed = new NetworkVariable<bool>();
        private NetworkVariable<int> _platesRemaining = new NetworkVariable<int>();
        private NetworkVariable<float> _nextSpawnTime = new NetworkVariable<float>();
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            if (_maxPlates > 0)
            {
                _platesRemaining.Value = _maxPlates;
            }
            
            // Set default spawn point if not assigned
            if (_spawnPoint == null)
            {
                _spawnPoint = interactionPoint;
            }
        }

        public override void OnNetworkSpawn()
        {
            _isBeingUsed.OnValueChanged += OnBeingUsedChanged;
            if (_platesRemaining != null)
            {
                _platesRemaining.OnValueChanged += OnPlatesRemainingChanged;
            }
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isBeingUsed.OnValueChanged -= OnBeingUsedChanged;
            if (_platesRemaining != null)
            {
                _platesRemaining.OnValueChanged -= OnPlatesRemainingChanged;
            }
        }

        private void Update()
        {
            if (IsServer && _maxPlates > 0 && _platesRemaining.Value < _maxPlates)
            {
                if (Time.time >= _nextSpawnTime.Value)
                {
                    _platesRemaining.Value++;
                    _nextSpawnTime.Value = Time.time + _respawnTime;
                }
            }
        }
        #endregion

        #region IInteractable Implementation
        public bool StartInteraction(PlayerController player, Action onComplete)
        {
            if (!CanInteract || !IsServer)
                return false;

            if (player.HeldItem != null)
                return false;

            if (_maxPlates > 0 && _platesRemaining.Value <= 0)
            {
                PlayEmptySound();
                return false;
            }

            DispensePlateServerRpc(player.NetworkObjectId);
            return true;
        }

        public void CancelInteraction(PlayerController player)
        {
            // Dispenser interactions are instant, no need to cancel
        }

        public bool ContinueInteraction(PlayerController player)
        {
            // Dispenser interactions are instant, no need to continue
            return false;
        }
        #endregion

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void DispensePlateServerRpc(ulong playerId)
        {
            if (_isBeingUsed.Value)
                return;

            _isBeingUsed.Value = true;

            // Spawn the plate
            GameObject plate = Instantiate(_platePrefab, _spawnPoint.position, _spawnPoint.rotation);
            NetworkObject networkObj = plate.GetComponent<NetworkObject>();
            networkObj.Spawn();

            // Update quantity if needed
            if (_maxPlates > 0)
            {
                _platesRemaining.Value--;
                _nextSpawnTime.Value = Time.time + _respawnTime;
            }

            // Give to player
            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            var player = playerObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PickupItem(plate);
            }

            DispensePlateClientRpc();
            _isBeingUsed.Value = false;
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void DispensePlateClientRpc()
        {
            PlayDispenseSound();
            UpdateVisuals();
        }
        #endregion

        #region Private Methods
        private void OnBeingUsedChanged(bool previousValue, bool newValue)
        {
            UpdateVisuals();
        }

        private void OnPlatesRemainingChanged(int previousValue, int newValue)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            SetHighlight(CanInteract);
            if (_maxPlates > 0)
            {
                SetStationMaterial(_platesRemaining.Value > 0 ? StationMaterialState.Normal : StationMaterialState.Disabled);
            }
        }

        private void PlayDispenseSound()
        {
            PlaySound(_dispensePlateSound);
        }

        private void PlayEmptySound()
        {
            PlaySound(_emptySound);
        }
        #endregion
    }
} 