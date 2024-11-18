using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Core.TeamManagement;

namespace RecipeRage.Gameplay.Player
{
    public class PlayerCharacter : BaseNetworkCharacter
    {
        [Header("Input Settings")]
        [SerializeField] private float inputSmoothTime = 0.1f;
        
        // Input state
        private Vector3 _moveInput;
        private Vector3 _currentVelocity;
        private bool _isInteractPressed;
        private bool _isAttackPressed;
        private bool _isSpecialPressed;

        // Team reference
        private TeamManager _teamManager;
        private readonly NetworkVariable<int> _teamId = new(-1);

        public int TeamId => _teamId.Value;

        protected override void InitializeClientSide()
        {
            base.InitializeClientSide();
            enabled = true;

            // Find team manager
            _teamManager = FindObjectOfType<TeamManager>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer && _teamId.Value == -1)
            {
                // Auto-assign to team if not set
                _teamManager = FindObjectOfType<TeamManager>();
                if (_teamManager != null)
                {
                    _teamManager.AssignPlayerToTeamServerRpc(NetworkObjectId, 0);
                }
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            HandleInput();
            UpdateState();
            HandleActions();
        }

        private void HandleInput()
        {
            // Movement input with smoothing
            Vector3 targetInput = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                0f,
                Input.GetAxisRaw("Vertical")
            ).normalized;

            _moveInput = Vector3.SmoothDamp(
                _moveInput,
                targetInput,
                ref _currentVelocity,
                inputSmoothTime
            );

            // Action inputs
            _isInteractPressed = Input.GetButtonDown("Interact");
            _isAttackPressed = Input.GetButtonDown("Fire1");
            _isSpecialPressed = Input.GetButtonDown("Fire2");
        }

        private void UpdateState()
        {
            if (_moveInput.magnitude > 0.1f)
            {
                UpdateStateServerRpc(StateType.Moving);
            }
            else if (CurrentState.Value == StateType.Moving)
            {
                UpdateStateServerRpc(StateType.Idle);
            }
        }

        private void HandleActions()
        {
            if (_isInteractPressed)
            {
                HandleInteraction();
            }

            if (_isAttackPressed)
            {
                HandleAttack();
            }

            if (_isSpecialPressed)
            {
                HandleSpecialAbility();
            }
        }

        private void HandleInteraction()
        {
            if (Physics.Raycast(
                transform.position,
                transform.forward,
                out RaycastHit hit,
                interactionRange,
                interactionMask
            ))
            {
                if (hit.collider.TryGetComponent<NetworkObject>(out var netObj))
                {
                    InteractServerRpc(netObj);
                }
            }
        }

        private void HandleAttack()
        {
            if (CurrentState.Value != StateType.Idle &&
                CurrentState.Value != StateType.Moving)
                return;

            AttackServerRpc();
        }

        private void HandleSpecialAbility()
        {
            if (CurrentState.Value != StateType.Idle &&
                CurrentState.Value != StateType.Moving)
                return;

            UseSpecialAbilityServerRpc();
        }

        [ServerRpc]
        private void AttackServerRpc()
        {
            if (Combat.TryAttack())
            {
                UpdateStateServerRpc(StateType.Attacking);
            }
        }

        [ServerRpc]
        private void UseSpecialAbilityServerRpc()
        {
            if (Combat.TryUseSpecialAbility())
            {
                // Special ability state and effects handled by Combat component
            }
        }

        protected override bool TryStartInteraction(IInteractable interactable)
        {
            if (!base.TryStartInteraction(interactable)) return false;

            interactable.StartInteraction(this);
            return true;
        }

        [ServerRpc]
        public void SetTeamServerRpc(int teamId)
        {
            if (!IsServer) return;
            _teamId.Value = teamId;
        }

        public bool IsTeammate(PlayerCharacter other)
        {
            return _teamId.Value != -1 && _teamId.Value == other.TeamId;
        }
    }
}
