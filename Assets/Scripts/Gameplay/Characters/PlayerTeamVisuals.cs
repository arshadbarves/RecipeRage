using Unity.Netcode;
using UnityEngine;
using Core.Logging;
using Core.Networking.Interfaces; // For IPlayerNetworkManager if needed, or IPlayerController

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles visual representation of team affiliation (Ally vs Enemy).
    /// </summary>
    public class PlayerTeamVisuals : NetworkBehaviour
    {
        [Header("Team Visual Settings")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Renderer _targetRenderer;
        
        [Header("Materials")]
        [SerializeField] private Material _allyMaterial;
        [SerializeField] private Material _enemyMaterial;
        
        [Header("Highlights (Optional)")]
        [SerializeField] private GameObject _allyHighlight;
        [SerializeField] private GameObject _enemyHighlight;

        private int _cachedTeamId = -1;
        private bool _initialized = false;

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            if (!IsSpawned) return;

            // wait for local player to exist
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null)
            {
                return;
            }

            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
            if (localPlayer == null) return;

            // Check if updates are needed
            if (!_initialized || _cachedTeamId != _playerController.TeamId)
            {
                UpdateVisuals(localPlayer);
            }
        }

        private void UpdateVisuals(PlayerController localPlayer)
        {
            _cachedTeamId = _playerController.TeamId;
            _initialized = true;

            bool isAlly = (localPlayer.TeamId == _playerController.TeamId);
            bool isMe = (localPlayer == _playerController);

            if (_allyHighlight != null) _allyHighlight.SetActive(isAlly || isMe);
            if (_enemyHighlight != null) _enemyHighlight.SetActive(!isAlly && !isMe);

            if (_targetRenderer != null)
            {
                if (isMe)
                {
                    // Maybe distinct color for self? Or same as Ally (Green/Blue)
                    if (_allyMaterial != null) _targetRenderer.material = _allyMaterial;
                }
                else if (isAlly)
                {
                     if (_allyMaterial != null) _targetRenderer.material = _allyMaterial;
                }
                else
                {
                     if (_enemyMaterial != null) _targetRenderer.material = _enemyMaterial;
                }
            }
        }
    }
}
