using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using System.Collections.Generic;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Input;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Domain;
using Unity.Collections;
using VContainer;
using VContainer.Unity;
using Playcenter.Shell;


namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Network player facade — orchestrates SOLID collaborators and partial regions.
    /// Partials: InputMovement (input/RPCs), Character (class/abilities), Skins, Carrying.
    /// Collaborators: PlayerStateController, PlayerMovementController, PlayerInputHandler,
    /// PlayerNetworkController, PlayerInteractionController.
    /// </summary>
    public partial class PlayerController : NetworkBehaviour, IPlayerController, IInteractable
    {
        #region Inspector Settings

        [Header("Movement Settings")]
        [SerializeField] private float _baseMovementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _carryingSpeedMultiplier = 0.7f;

        [Header("Input Smoothing")]
        [SerializeField] private bool _enableInputSmoothing = true;
        [SerializeField] private float _inputSmoothTime = 0.1f;

        [Header("Network Prediction")]
        [SerializeField] private bool _enableClientPrediction = true;
        [SerializeField] private int _maxInputHistorySize = 60;
        [SerializeField] private float _reconciliationThreshold = 0.1f;

        [Header("Interaction Settings")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private Transform _holdPoint;

        [Header("Character Settings")]
        [SerializeField] private int _characterClassId;

        [Header("Skin Settings")]
        [SerializeField] private Transform _skinRoot;
        [SerializeField] private bool _randomizeBotSkin = true;

        #endregion

        #region Dependencies

        [Inject] private IEventBus _eventBus;
        [Inject] private ICharacterService _characterService;
        [Inject] private IPlayerNetworkManager _playerNetworkManager;
        [Inject] private IInputProvider _inputProvider;

        #endregion

        #region Components

        private Rigidbody _rigidbody;

        #endregion

        #region Controllers (SOLID)

        private PlayerStateController _stateController;
        private PlayerMovementController _movementController;
        private PlayerInputHandler _inputHandler;
        private PlayerNetworkController _networkController;
        private PlayerInteractionController _interactionController;

        #endregion

        #region Character Data

        private GameObject _heldObject;
        /// <summary>v2 dish carry list (logical items; visual hold still uses _heldObject).</summary>
        private readonly List<CarriedItemData> _carriedDishes = new List<CarriedItemData>(4);
        private bool _inputEnabled = true;
        private bool _isVisible = true;

        public CharacterClass CharacterClass { get; private set; }
        public CharacterAbility PrimaryAbility { get; private set; }
        public ModifiableStat InteractionSpeed { get; } = new ModifiableStat(1f);
        public ModifiableStat CarryingCapacity { get; } = new ModifiableStat(1f);

        private NetworkVariable<int> _teamId = new NetworkVariable<int>(0);
        public int TeamId => _teamId.Value;

        private readonly NetworkVariable<FixedString64Bytes> _skinId = new NetworkVariable<FixedString64Bytes>(default);
        private GameObject _skinInstance;
        private MeshRenderer _fallbackModelRenderer;

        public void SetTeam(int teamId)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only server can set team ID");
                return;
            }
            _teamId.Value = teamId;
        }

        #endregion

        #region Events

        public event Action<IInteractable> OnInteraction;
        public event Action<CharacterAbility> OnAbilityUsed;
        public event Action<PlayerMovementState, PlayerMovementState> OnMovementStateChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            var scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
            else
            {
                GameLogger.LogError("LifetimeScope not found! PlayerController dependencies will fail.");
            }

            _rigidbody = GetComponent<Rigidbody>();
            InitializeControllers();
        }

        private void Update()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            if (!_inputEnabled)
            {
                _inputHandler?.SetRawInput(Vector2.zero);
                _inputHandler?.UpdateSmoothing();
                _stateController?.UpdateState(Vector2.zero, IsHoldingObject());
                if (PrimaryAbility != null)
                {
                    PrimaryAbility.Update(Time.deltaTime);
                }
                return;
            }

            _inputProvider?.Update();
            _inputHandler?.UpdateSmoothing();
            _stateController?.UpdateState(_inputHandler?.GetSmoothedInput() ?? Vector2.zero, IsHoldingObject());

            if (PrimaryAbility != null)
            {
                PrimaryAbility.Update(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            if (_enableClientPrediction && _networkController != null && _networkController.IsPredictionEnabled)
            {
                ProcessMovementWithPrediction();
            }
            else
            {
                ProcessMovement();
            }
        }

        #endregion

        #region Initialization

        private void InitializeControllers()
        {
            _stateController = new PlayerStateController();
            _stateController.OnStateChanged += (prev, curr) => OnMovementStateChanged?.Invoke(prev, curr);

            _movementController = new PlayerMovementController(
                _rigidbody,
                transform,
                _baseMovementSpeed,
                _rotationSpeed,
                _carryingSpeedMultiplier
            );

            _inputHandler = new PlayerInputHandler(_enableInputSmoothing, _inputSmoothTime);

            _networkController = new PlayerNetworkController(
                _enableClientPrediction,
                _maxInputHistorySize,
                _reconciliationThreshold
            );

            _interactionController = new PlayerInteractionController(
                transform,
                _interactionRadius,
                _interactionLayer
            );

            if (_interactionController != null)
            {
                _interactionController.OnInteraction += (interactable) => OnInteraction?.Invoke(interactable);
                _interactionController.OnAbilityUsed += (ability) => OnAbilityUsed?.Invoke(ability);
            }
        }

        #endregion

        #region Network Lifecycle

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkObject != null && NetworkObject.IsPlayerObject)
            {
                IPlayerNetworkManager playerNetworkManager = ResolvePlayerNetworkManager();
                playerNetworkManager?.RegisterPlayer(OwnerClientId, this);
            }

            if (IsLocalPlayer)
            {
                SetupInput();
                _eventBus?.Publish(new LocalPlayerSpawnedEvent
                {
                    PlayerTransform = (object)transform,
                    PlayerObject = (object)gameObject
                });
            }

            SetupCharacterClass();
            InitializeSkinSystem();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsLocalPlayer)
            {
                _eventBus?.Publish(new LocalPlayerDespawnedEvent());
            }

            if (NetworkObject != null && NetworkObject.IsPlayerObject)
            {
                IPlayerNetworkManager playerNetworkManager = ResolvePlayerNetworkManager();
                playerNetworkManager?.UnregisterPlayer(OwnerClientId);
            }

            if (IsLocalPlayer && _inputProvider != null)
            {
                _inputProvider.OnMovementInput -= HandleMoveInput;
                _inputProvider.OnInteractionInput -= HandleInteractInput;
                _inputProvider.OnSpecialAbilityInput -= HandleAbilityInput;
            }

            CleanupSkinSystem();
        }

        #endregion

        #region Public API

        public PlayerMovementState GetMovementState() => _stateController?.CurrentState ?? PlayerMovementState.Idle;
        public void SetMovementState(PlayerMovementState state) => _stateController?.SetState(state);
        public bool IsMoving() => _stateController?.IsMoving() ?? false;

        public float GetCurrentSpeed() => _movementController?.GetCurrentSpeed() ?? 0f;
        public Vector3 GetVelocity() => _movementController?.GetVelocity() ?? Vector3.zero;
        public float MovementSpeed
        {
            get => _movementController?.MovementSpeed ?? 0f;
            set { if (_movementController != null)
                {
                    _movementController.MovementSpeed = value;
                }
            }
        }

        public void Stun(float duration)
        {
            _stateController?.SetState(PlayerMovementState.Stunned);
            Invoke(nameof(ClearStun), duration);
        }

        private void ClearStun()
        {
            if (_stateController?.CurrentState == PlayerMovementState.Stunned)
            {
                _stateController.SetState(PlayerMovementState.Idle);
            }
        }

        /// <summary>
        /// Show/hide player visuals (used on KO / respawn). Affects skin instance and fallback mesh.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;

            if (_skinInstance != null)
            {
                foreach (Renderer renderer in _skinInstance.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = visible;
                }
            }

            SetFallbackModelVisible(visible && _skinInstance == null);
        }

        /// <summary>
        /// Enable/disable local movement input processing (KO lockout / countdown).
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled)
            {
                _inputHandler?.SetRawInput(Vector2.zero);
            }
        }

        public bool IsInputEnabled => _inputEnabled;
        public bool IsVisible => _isVisible;

        #endregion

        #region IInteractable Implementation

        public void Interact(object playerObj)
        {
            var player = (PlayerController)playerObj;
            if (!IsServer)
            {
                return;
            }

            if (player.TeamId != TeamId)
            {
                Stun(2.0f);

                if (IsHoldingObject())
                {
                    DropObject();
                }

                GameLogger.Log($"Player {player.OwnerClientId} slapped Player {OwnerClientId}!");
            }
        }

        public string GetInteractionPrompt()
        {
            return "Slap!";
        }

        public bool CanInteract(object playerObj)
        {
            var player = (PlayerController)playerObj;
            return player.TeamId != TeamId;
        }

        #endregion

private IAbilityService FindAbilityService()
        {
            // Resolve from nearest VContainer scope
            var scope = LifetimeScope.Find<LifetimeScope>(gameObject.scene);
            if (scope != null)
            {
                try { return scope.Container.Resolve<IAbilityService>(); }
                catch { /* Not registered in this scope */ }
            }
            return null;
        }

        private IPlayerNetworkManager ResolvePlayerNetworkManager()
        {
            return _playerNetworkManager;
        }

    }
}
