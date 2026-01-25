using System;
using Core.Logging;
using Core.Shared.Events;
using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.Shared.Events;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Gameplay.Stations
{
    /// <summary>
    /// A sink station where players wash dirty dishes.
    /// Used for the "Trash Attack" mechanic in Kitchen Wars.
    /// </summary>
    public class SinkStation : StationBase
    {
        [Header("Sink Settings")]
        [SerializeField] private int _teamId;
        [SerializeField] private float _washTimePerPlate = 2.0f;
        [SerializeField] private GameObject _dirtyDishesVisual;
        [SerializeField] private GameObject _progressBarPrefab;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _washSound;
        [SerializeField] private AudioClip _cleanSound;

        private NetworkVariable<int> _dirtyPlateCount = new NetworkVariable<int>(0);
        private NetworkVariable<bool> _isWashing = new NetworkVariable<bool>(false);
        private NetworkVariable<float> _washProgress = new NetworkVariable<float>(0f);

        [Inject] private IEventBus _eventBus;

        private float _currentWashTimer;
        private GameObject _progressBar;
        
        // Expose TeamId for logic
        public int TeamId => _teamId;
        public int DirtyPlateCount => _dirtyPlateCount.Value;

        protected override void Awake()
        {
            base.Awake();
            if (_dirtyDishesVisual != null) _dirtyDishesVisual.SetActive(false);
            
            // Setup Progress Bar
            if (_progressBarPrefab != null && _ingredientPlacementPoint != null)
            {
                _progressBar = Instantiate(_progressBarPrefab, _ingredientPlacementPoint.position + Vector3.up * 1.5f, Quaternion.identity);
                _progressBar.transform.SetParent(transform);
                _progressBar.SetActive(false);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _dirtyPlateCount.OnValueChanged += OnDirtyCountChanged;
            _isWashing.OnValueChanged += OnWashingStateChanged;
            
            // Sync initial state
            OnDirtyCountChanged(0, _dirtyPlateCount.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _dirtyPlateCount.OnValueChanged -= OnDirtyCountChanged;
            _isWashing.OnValueChanged -= OnWashingStateChanged;
        }

        protected void Update()
        {
            if (!IsServer) return;

            if (_isWashing.Value)
            {
                _currentWashTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_currentWashTimer / _washTimePerPlate);
                _washProgress.Value = progress;

                if (_currentWashTimer >= _washTimePerPlate)
                {
                    CompleteWash();
                }
            }
        }

        // --- Interaction ---

        protected override void HandleInteraction(PlayerController player)
        {
            // Sink doesn't take items, it just requires interaction
            // To simulate "Holding", this simple interaction toggles washing?
            // "ProcessingStation" logic usually requires "Place Item" then "Interact".
            // Here, the "Item" is the virtual dirty dish.
            
            // Verify Team
            if (player.TeamId != _teamId)
            {
                // Can only wash own team's dishes
                // Or maybe enemies can steal? (Out of scope for now)
                return;
            }

            if (_dirtyPlateCount.Value > 0 && !_isWashing.Value)
            {
                StartWashing();
            }
            // If already washing, maybe interaction cancels it?
            // For now, let's say interacting starts it.
        }

        public override bool CanInteract(PlayerController player)
        {
             // Can interact if there are dirty dishes and we aren't already washing
             // And if player belongs to this team
             return _dirtyPlateCount.Value > 0 && !_isWashing.Value && player.TeamId == _teamId;
        }

        public override string GetInteractionPrompt()
        {
            if (_dirtyPlateCount.Value > 0)
            {
                return _isWashing.Value ? "Washing..." : $"Wash Dishes ({_dirtyPlateCount.Value})";
            }
            return "";
        }

        // --- Logic ---

        [ServerRpc(RequireOwnership = false)]
        public void AddDirtyPlatesServerRpc(int count)
        {
            _dirtyPlateCount.Value += count;
            GameLogger.Log($"Sink (Team {_teamId}) received {count} dirty dishes. Total: {_dirtyPlateCount.Value}");

            TriggerTrashAttackShakeClientRpc();
        }

        [ClientRpc]
        private void TriggerTrashAttackShakeClientRpc()
        {
            _eventBus?.Publish(new CameraShakeEvent(0.8f, 0.5f));
        }

        private void StartWashing()
        {
            _isWashing.Value = true;
            _currentWashTimer = 0f;
            if (_progressBar) _progressBar.SetActive(true);
            
            if (_audioSource && _washSound)
            {
                _audioSource.clip = _washSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        private void CompleteWash()
        {
            _dirtyPlateCount.Value--;
            _isWashing.Value = false;
            _washProgress.Value = 0f;
            
            GameLogger.Log($"Plate washed! Remaining: {_dirtyPlateCount.Value}");
            
            // Stop Sound
            if (_audioSource) 
            {
                _audioSource.Stop();
                if (_cleanSound) _audioSource.PlayOneShot(_cleanSound);
            }
            
            // Restart if more plates?
            // For now, require re-interaction to wash next (spammy/ragey)
            // Or auto-continue? Rage maps usually require spamming.
            // Let's require re-trigger for maximum annoyance, or auto if holding (requires input system change).
            // Let's stick to single plate for now.
        }
        
        private void OnDirtyCountChanged(int prev, int curr)
        {
            if (_dirtyDishesVisual != null)
            {
                _dirtyDishesVisual.SetActive(curr > 0);
            }
        }

        private void OnWashingStateChanged(bool prev, bool curr)
        {
            if (_progressBar != null) _progressBar.SetActive(curr);
            if (!curr && _audioSource) _audioSource.Stop();
        }
    }
}
