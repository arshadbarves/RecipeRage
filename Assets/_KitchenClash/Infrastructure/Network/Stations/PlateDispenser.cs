using KitchenClash.Domain;
using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network.Stations
{
    /// <summary>
    /// Dispenses clean plates to empty-handed players.
    /// </summary>
    public class PlateDispenser : StationBase
    {
        [Header("Plate Dispenser Settings")]
        [SerializeField] private GameObject _platePrefab;
        [SerializeField] private int _teamId = -1;
        [SerializeField] private bool _sharedAcrossTeams = true;
        [SerializeField] private int _maxStock = 3;
        [SerializeField] private float _restockDelay = 2f;

        private readonly NetworkVariable<int> _availableStock = new NetworkVariable<int>(0);
        private float _restockTimer;

        public int TeamId => _teamId;
        public bool IsShared => _sharedAcrossTeams || _teamId < 0;
        public int AvailableStock => _availableStock.Value;

        protected override void Awake()
        {
            base.Awake();
            _stationName = "Plate Dispenser";
            _availableStock.Value = Mathf.Max(1, _maxStock);
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (_availableStock.Value >= _maxStock)
            {
                _restockTimer = 0f;
                return;
            }

            _restockTimer += Time.deltaTime;
            if (_restockTimer >= _restockDelay)
            {
                _restockTimer = 0f;
                _availableStock.Value = Mathf.Min(_maxStock, _availableStock.Value + 1);
            }
        }

        protected override void HandleInteraction(PlayerController player)
        {
            if (player == null)
            {
                return;
            }

            if (!IsShared && player.TeamId != _teamId)
            {
                return;
            }

            if (player.IsHoldingObject())
            {
                return;
            }

            if (_availableStock.Value <= 0)
            {
                GameLogger.Log("[PlateDispenser] No plates available.");
                return;
            }

            if (_platePrefab == null)
            {
                GameLogger.LogError("[PlateDispenser] Plate prefab is missing.");
                return;
            }

            Vector3 spawnPosition = _ingredientPlacementPoint != null
                ? _ingredientPlacementPoint.position
                : transform.position + Vector3.up;

            GameObject plateObject = Instantiate(_platePrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = plateObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                GameLogger.LogError("[PlateDispenser] Plate prefab is missing NetworkObject.");
                Destroy(plateObject);
                return;
            }

            networkObject.Spawn(true);
            player.PickUpObject(plateObject);
            _availableStock.Value = Mathf.Max(0, _availableStock.Value - 1);
            _restockTimer = 0f;
        }

        public override bool CanInteract(object playerObj)
        {
            var player = playerObj as PlayerController;
            if (player == null || player.IsHoldingObject())
            {
                return false;
            }

            return IsShared || player.TeamId == _teamId;
        }

        public override string GetInteractionPrompt()
        {
            return _availableStock.Value > 0 ? "Take Plate" : "Restocking Plates";
        }
    }
}
