using Unity.Netcode;
using UnityEngine;
using Core.Logging;
using Core.Networking.Interfaces; // For IPlayerNetworkManager if needed, or IPlayerController
using Gameplay.Shared;
using VContainer;
using VContainer.Unity;

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
        private int _cachedLocalTeamId = -1;
        private bool _initialized = false;

        [Inject] private IMatchContext _matchContext;

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>();
            }

            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
        }

        private void Update()
        {
            if (!IsSpawned) return;

            _matchContext?.Refresh();

            PlayerController localPlayer = _matchContext?.LocalPlayer;
            if (localPlayer == null)
            {
                return;
            }

            int localTeamId = _matchContext?.LocalTeamId ?? localPlayer.TeamId;

            // Check if updates are needed
            if (!_initialized || _cachedTeamId != _playerController.TeamId || _cachedLocalTeamId != localTeamId)
            {
                UpdateVisuals(localPlayer, localTeamId);
            }
        }

        private void UpdateVisuals(PlayerController localPlayer, int localTeamId)
        {
            _cachedTeamId = _playerController.TeamId;
            _cachedLocalTeamId = localTeamId;
            _initialized = true;

            bool isAlly = localTeamId == _playerController.TeamId;
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
