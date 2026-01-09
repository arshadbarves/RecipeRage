using System;
using Modules.Input;
using Gameplay.Camera;
using Unity.Netcode;
using UnityEngine;
using Modules.Logging;
using Modules.Networking.Services;
using Gameplay.Shared.Stats;
using Modules.Session;
using VContainer;
using VContainer.Unity;

namespace Gameplay.Characters
{
    /// <summary>
    /// Main player controller - orchestrates all player subsystems.
    /// Follows Single Responsibility Principle by delegating to specialized controllers.
    /// </summary>
    public class PlayerController : NetworkBehaviour
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

        #endregion

        #region Dependencies

        [Inject]
        private SessionManager _sessionManager;

        [Inject]
        private ICameraController _cameraController;

        #endregion

        #region Components

        private Rigidbody _rigidbody;
        private IInputProvider _inputProvider;

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
        public CharacterClass CharacterClass { get; private set; }
        public CharacterAbility PrimaryAbility { get; private set; }
        public ModifiableStat InteractionSpeed { get; } = new ModifiableStat(1f);
        public ModifiableStat CarryingCapacity { get; } = new ModifiableStat(1f);

        #endregion

        #region Events

        public event Action<IInteractable> OnInteraction;
        public event Action<CharacterAbility> OnAbilityUsed;
        public event Action<PlayerMovementState, PlayerMovementState> OnMovementStateChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // VContainer injection for NGO-spawned object
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
            if (!IsLocalPlayer) return;

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
            if (!IsLocalPlayer) return;

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
            // State Controller
            _stateController = new PlayerStateController();
            _stateController.OnStateChanged += (prev, curr) => OnMovementStateChanged?.Invoke(prev, curr);

            // Movement Controller
            _movementController = new PlayerMovementController(
                _rigidbody,
                transform,
                _baseMovementSpeed,
                _rotationSpeed,
                _carryingSpeedMultiplier
            );

            // Input Handler
            _inputHandler = new PlayerInputHandler(_enableInputSmoothing, _inputSmoothTime);

            // Network Controller
            _networkController = new PlayerNetworkController(
                _enableClientPrediction,
                _maxInputHistorySize,
                _reconciliationThreshold
            );

            // Interaction Controller
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

            // Register with PlayerNetworkManager
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var playerNetworkManager = sessionContainer.Resolve<IPlayerNetworkManager>();
                playerNetworkManager?.RegisterPlayer(OwnerClientId, this);
            }

            if (IsLocalPlayer)
            {
                SetupInput();
                SetupCamera();
            }

            SetupCharacterClass();
        }

        private void SetupCamera()
        {
            if (_cameraController != null && _cameraController.IsInitialized)
            {
                _cameraController.SetFollowTarget(transform);
            }
            else
            {
                GameLogger.LogWarning("Camera controller not available - camera will not follow player");
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsLocalPlayer)
            {
                if (_cameraController != null)
                {
                    _cameraController.ClearFollowTarget();
                }
            }

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var playerNetworkManager = sessionContainer.Resolve<IPlayerNetworkManager>();
                playerNetworkManager?.UnregisterPlayer(OwnerClientId);
            }

            if (IsLocalPlayer && _inputProvider != null)
            {
                _inputProvider.OnMovementInput -= HandleMoveInput;
                _inputProvider.OnInteractionInput -= HandleInteractInput;
                _inputProvider.OnSpecialAbilityInput -= HandleAbilityInput;
            }
        }

        #endregion

        #region Input Setup

        private void SetupInput()
        {
            _inputProvider = InputProviderFactory.CreateForPlatform();
            if (_inputProvider == null)
            {
                GameLogger.LogError("Input provider not found");
                return;
            }

            _inputProvider.OnMovementInput += HandleMoveInput;
            _inputProvider.OnInteractionInput += HandleInteractInput;
            _inputProvider.OnSpecialAbilityInput += HandleAbilityInput;
        }

        private void HandleMoveInput(Vector2 input)
        {
            _inputHandler?.SetRawInput(input);
        }

        private void HandleInteractInput()
        {
            if (!IsLocalPlayer) return;

            bool interacted = _interactionController?.TryInteract(_stateController, this) ?? false;
            if (interacted)
            {
                InteractServerRpc();
            }
        }

        private void HandleAbilityInput()
        {
            if (!IsLocalPlayer) return;

            bool used = _interactionController?.TryUseAbility(PrimaryAbility, _stateController) ?? false;
            if (used)
            {
                UseAbilityServerRpc();
            }
        }

        #endregion

        #region Movement Processing

        private void ProcessMovement()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null) return;
            Vector2 input = _inputHandler.GetSmoothedInput();
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);
        }

        private void ProcessMovementWithPrediction()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null || _networkController == null) return;
            Vector2 input = _inputHandler.GetSmoothedInput();

            PlayerInputData inputData = _networkController.CreateInputData(input);
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);

            PlayerStateData stateData = _networkController.CreateStateData(transform, _rigidbody, inputData.SequenceNumber);
            _networkController.StoreHistory(inputData, stateData);

            SendInputToServerRpc(inputData);
        }

        #endregion

        #region Network RPCs

        [ServerRpc]
        private void SendInputToServerRpc(PlayerInputData input)
        {
            if (_movementController == null || _stateController == null || _networkController == null) return;
            _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime);
            PlayerStateData authState = _networkController.CreateStateData(transform, _rigidbody, input.SequenceNumber);
            ReconcileStateClientRpc(authState);
        }

        [ClientRpc]
        private void ReconcileStateClientRpc(PlayerStateData serverState)
        {
            if (IsServer || _networkController == null || _movementController == null || _stateController == null) return;

            _networkController.ReconcileState(
                serverState,
                transform,
                _rigidbody,
                (input) => _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime)
            );
        }

        [ServerRpc]
        private void InteractServerRpc() => InteractClientRpc();

        [ClientRpc]
        private void InteractClientRpc()
        {
            if (IsLocalPlayer) return;
        }

        [ServerRpc]
        private void UseAbilityServerRpc() => UseAbilityClientRpc();

        [ClientRpc]
        private void UseAbilityClientRpc()
        {
            if (IsLocalPlayer) return;
        }

        #endregion

        #region Character Class Management

        private void SetupCharacterClass()
        {
            var sessionContainer = _sessionManager?.SessionContainer;
            ICharacterService characterService = null;
            if (sessionContainer != null)
            {
                characterService = sessionContainer.Resolve<ICharacterService>();
            }

            if (characterService == null)
            {
                GameLogger.LogError("Character service not available");
                return;
            }

            CharacterClass = characterService.GetCharacter(_characterClassId);
            if (CharacterClass == null)
            {
                CharacterClass = characterService.SelectedCharacter;
                _characterClassId = CharacterClass?.Id ?? 0;
            }

            if (CharacterClass != null && _movementController != null)
            {
                _movementController.MovementSpeed = _baseMovementSpeed * CharacterClass.MovementSpeedModifier;
                InteractionSpeed.BaseValue = CharacterClass.InteractionSpeedModifier;
                CarryingCapacity.BaseValue = CharacterClass.CarryingCapacityModifier;
                PrimaryAbility = CharacterAbility.CreateAbility(CharacterClass.PrimaryAbilityType, CharacterClass, this);
            }
        }

        public void SetCharacterClass(int characterClassId)
        {
            _characterClassId = characterClassId;
            SetupCharacterClass();

            if (IsLocalPlayer)
            {
                SetCharacterClassServerRpc(characterClassId);
            }
        }

        [ServerRpc]
        private void SetCharacterClassServerRpc(int characterClassId)
        {
            _characterClassId = characterClassId;
            SetCharacterClassClientRpc(characterClassId);
        }

        [ClientRpc]
        private void SetCharacterClassClientRpc(int characterClassId)
        {
            if (IsLocalPlayer) return;
            _characterClassId = characterClassId;
            SetupCharacterClass();
        }

        #endregion

        #region Object Carrying

        public bool PickUpObject(GameObject obj)
        {
            if (_heldObject != null || _holdPoint == null) return false;

            _heldObject = obj;
            obj.transform.SetParent(_holdPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            var objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null) objRigidbody.isKinematic = true;

            return true;
        }

        public GameObject DropObject()
        {
            if (_heldObject == null) return null;

            GameObject obj = _heldObject;
            _heldObject = null;
            obj.transform.SetParent(null);

            var objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null) objRigidbody.isKinematic = false;

            return obj;
        }

        public GameObject GetHeldObject() => _heldObject;
        public bool IsHoldingObject() => _heldObject != null;

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
            set { if (_movementController != null) _movementController.MovementSpeed = value; }
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

        #endregion
    }
}