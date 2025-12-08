using System;
using Core.Bootstrap;
using Core.Input;
using Gameplay;
using Unity.Netcode;
using UnityEngine;
using Core.Logging;

namespace Core.Characters
{
    /// <summary>
    /// Main player controller - orchestrates all player subsystems.
    /// Follows Single Responsibility Principle by delegating to specialized controllers.
    /// 
    /// Responsibilities:
    /// - Unity lifecycle management
    /// - Component orchestration
    /// - Network synchronization
    /// - Character class management
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

        #region Components
        
        private Rigidbody _rigidbody;
        private IInputProvider _inputProvider;
        
        #endregion

        #region Controllers (SOLID - Separated Responsibilities)
        
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
        public float InteractionSpeedModifier { get; set; } = 1f;
        public int CarryingCapacity { get; set; } = 1;
        
        #endregion

        #region Events
        
        public event Action<IInteractable> OnInteraction;
        public event Action<CharacterAbility> OnAbilityUsed;
        public event Action<PlayerMovementState, PlayerMovementState> OnMovementStateChanged;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            InitializeControllers();
        }

        private void Update()
        {
            if (!IsLocalPlayer) return;

            _inputProvider?.Update();
            _inputHandler.UpdateSmoothing();
            _stateController.UpdateState(_inputHandler.GetSmoothedInput(), IsHoldingObject());

            if (PrimaryAbility != null)
            {
                PrimaryAbility.Update(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!IsLocalPlayer) return;

            if (_enableClientPrediction && _networkController.IsPredictionEnabled)
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
            
            _interactionController.OnInteraction += (interactable) => OnInteraction?.Invoke(interactable);
            _interactionController.OnAbilityUsed += (ability) => OnAbilityUsed?.Invoke(ability);
        }
        
        #endregion

        #region Network Lifecycle
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Register with PlayerNetworkManager
            var services = GameBootstrap.Services;
            if (services?.Session?.PlayerNetworkManager != null)
            {
                services.Session.PlayerNetworkManager.RegisterPlayer(OwnerClientId, this);
                GameLogger.Log($"Registered player {OwnerClientId} with PlayerNetworkManager");
            }

            if (IsLocalPlayer)
            {
                SetupInput();
                SetupCamera();
            }

            SetupCharacterClass();
        }

        /// <summary>
        /// Setup camera to follow this player (local player only)
        /// </summary>
        private void SetupCamera()
        {
            var cameraController = GameplayContext.CameraController;
            if (cameraController != null && cameraController.IsInitialized)
            {
                cameraController.SetFollowTarget(transform);
                GameLogger.Log($"Camera set to follow local player: {gameObject.name}");
            }
            else
            {
                GameLogger.LogWarning("Camera controller not available - camera will not follow player");
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Clear camera target if this is the local player
            if (IsLocalPlayer)
            {
                var cameraController = GameplayContext.CameraController;
                if (cameraController != null)
                {
                    cameraController.ClearFollowTarget();
                    GameLogger.Log("Camera follow target cleared");
                }
            }

            // Unregister from PlayerNetworkManager
            var services = GameBootstrap.Services;
            if (services?.Session?.PlayerNetworkManager != null)
            {
                services.Session.PlayerNetworkManager.UnregisterPlayer(OwnerClientId);
                GameLogger.Log($"Unregistered player {OwnerClientId} from PlayerNetworkManager");
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

            GameLogger.Log("Local player initialized");
        }

        private void HandleMoveInput(Vector2 input)
        {
            _inputHandler.SetRawInput(input);
        }

        private void HandleInteractInput()
        {
            if (!IsLocalPlayer) return;
            
            bool interacted = _interactionController.TryInteract(_stateController, this);
            if (interacted)
            {
                InteractServerRpc();
            }
        }

        private void HandleAbilityInput()
        {
            if (!IsLocalPlayer) return;
            
            bool used = _interactionController.TryUseAbility(PrimaryAbility, _stateController);
            if (used)
            {
                UseAbilityServerRpc();
            }
        }
        
        #endregion

        #region Movement Processing
        
        private void ProcessMovement()
        {
            Vector2 input = _inputHandler.GetSmoothedInput();
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);
        }

        private void ProcessMovementWithPrediction()
        {
            Vector2 input = _inputHandler.GetSmoothedInput();
            
            // Create input data
            PlayerInputData inputData = _networkController.CreateInputData(input);
            
            // Apply movement locally (prediction)
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);
            
            // Store state
            PlayerStateData stateData = _networkController.CreateStateData(
                transform,
                _rigidbody,
                inputData.SequenceNumber
            );
            
            _networkController.StoreHistory(inputData, stateData);
            
            // Send to server
            SendInputToServerRpc(inputData);
        }
        
        #endregion

        #region Network RPCs
        
        [ServerRpc]
        private void SendInputToServerRpc(PlayerInputData input)
        {
            // Server applies input
            _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime);
            
            // Send authoritative state back
            PlayerStateData authState = _networkController.CreateStateData(transform, _rigidbody, input.SequenceNumber);
            ReconcileStateClientRpc(authState);
        }

        [ClientRpc]
        private void ReconcileStateClientRpc(PlayerStateData serverState)
        {
            if (IsServer) return;
            
            _networkController.ReconcileState(
                serverState,
                transform,
                _rigidbody,
                (input) => _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime)
            );
        }

        [ServerRpc]
        private void InteractServerRpc()
        {
            InteractClientRpc();
        }

        [ClientRpc]
        private void InteractClientRpc()
        {
            if (IsLocalPlayer) return;
            // Play interaction effects
        }

        [ServerRpc]
        private void UseAbilityServerRpc()
        {
            UseAbilityClientRpc();
        }

        [ClientRpc]
        private void UseAbilityClientRpc()
        {
            if (IsLocalPlayer) return;
            // Play ability effects
        }
        
        #endregion

        #region Character Class Management
        
        private void SetupCharacterClass()
        {
            var services = GameBootstrap.Services;
            if (GameBootstrap.Services?.Session?.CharacterService == null)
            {
                GameLogger.LogError("Character service not available");
                return;
            }

            CharacterClass = services.Session.CharacterService.GetCharacter(_characterClassId);
            if (CharacterClass == null)
            {
                CharacterClass = services.Session.CharacterService.SelectedCharacter;
                _characterClassId = CharacterClass?.Id ?? 0;
            }

            if (CharacterClass != null)
            {
                _movementController.MovementSpeed = _baseMovementSpeed * CharacterClass.MovementSpeedModifier;
                InteractionSpeedModifier = CharacterClass.InteractionSpeedModifier;
                CarryingCapacity = Mathf.RoundToInt(CharacterClass.CarryingCapacityModifier);
                PrimaryAbility = CharacterAbility.CreateAbility(CharacterClass.PrimaryAbilityType, CharacterClass, this);
                
                GameLogger.Log($"Character: {CharacterClass.DisplayName}");
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
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = true;
            }

            return true;
        }

        public GameObject DropObject()
        {
            if (_heldObject == null) return null;

            GameObject obj = _heldObject;
            _heldObject = null;
            obj.transform.SetParent(null);

            var objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = false;
            }

            return obj;
        }

        public GameObject GetHeldObject() => _heldObject;
        public bool IsHoldingObject() => _heldObject != null;
        
        #endregion

        #region Public API
        
        // Movement State
        public PlayerMovementState GetMovementState() => _stateController.CurrentState;
        public void SetMovementState(PlayerMovementState state) => _stateController.SetState(state);
        public bool IsMoving() => _stateController.IsMoving();
        
        // Movement Speed
        public float GetCurrentSpeed() => _movementController.GetCurrentSpeed();
        public Vector3 GetVelocity() => _movementController.GetVelocity();
        public float MovementSpeed
        {
            get => _movementController.MovementSpeed;
            set => _movementController.MovementSpeed = value;
        }
        
        public void Stun(float duration)
        {
            _stateController.SetState(PlayerMovementState.Stunned);
            Invoke(nameof(ClearStun), duration);
        }

        private void ClearStun()
        {
            if (_stateController.CurrentState == PlayerMovementState.Stunned)
            {
                _stateController.SetState(PlayerMovementState.Idle);
            }
        }

        public void SetPredictionEnabled(bool enabled)
        {
            _enableClientPrediction = enabled;
        }

        public void SetInputSmoothingEnabled(bool enabled)
        {
            _enableInputSmoothing = enabled;
        }

        public string GetDebugInfo()
        {
            return $"State: {_stateController.CurrentState}\n" +
                   $"Speed: {GetCurrentSpeed():F2} m/s\n" +
                   $"Input: {_inputHandler.GetSmoothedInput()}\n" +
                   _networkController.GetDebugInfo();
        }
        
        #endregion
    }
}
