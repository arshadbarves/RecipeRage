using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.Services;
using System.Linq;
using KitchenClash.Infrastructure.Input;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Domain;
using Unity.Collections;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Main player controller - orchestrates all player subsystems.
    /// </summary>
    public class PlayerController : NetworkBehaviour, IPlayerController, IInteractable
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

        [Inject] private SessionManager _sessionManager;
        [Inject] private IEventBus _eventBus;
        [Inject] private ICharacterService _characterService;
        [Inject] private IPlayerNetworkManager _playerNetworkManager;

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
        // v2: tracks the dish data when a player collects from an AutonomousCookingStation
        private KitchenClash.Infrastructure.Network.CarriedItemData? _carriedItemData;
        // Replicated so clients can show carry state / interaction prompts.
        private readonly NetworkVariable<int> _carriedRecipeTier =
            new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<int> _carriedIngredientType =
            new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private GameObject _carriedDishVisual;
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

            _carriedRecipeTier.OnValueChanged += OnCarriedDishChanged;
            RefreshCarriedDishVisual(_carriedRecipeTier.Value);

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

            _carriedRecipeTier.OnValueChanged -= OnCarriedDishChanged;
            if (_carriedDishVisual != null)
            {
                Destroy(_carriedDishVisual);
                _carriedDishVisual = null;
            }

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
                _inputProvider.OnAttackInput -= HandleAttackInput;
            }

            CleanupSkinSystem();
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
            _inputProvider.OnAttackInput += HandleAttackInput;
        }

        private void HandleMoveInput(Vector2 input)
        {
            _inputHandler?.SetRawInput(input);
        }

        private void HandleInteractInput()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            bool interacted = _interactionController?.TryInteract(_stateController, this) ?? false;
            if (interacted)
            {
                InteractServerRpc();
            }
        }

        private void HandleAbilityInput()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            bool used = _interactionController?.TryUseAbility(PrimaryAbility, _stateController) ?? false;
            if (used)
            {
                UseAbilityServerRpc();
            }
        }

        private void HandleAttackInput()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            PlayerCombatController combat = GetComponent<PlayerCombatController>();
            combat?.RequestMeleeAttack();
        }

        #endregion

        #region Movement Processing

        private void ProcessMovement()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null)
            {
                return;
            }

            Vector2 input = _inputHandler.GetSmoothedInput();
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);
        }

        private void ProcessMovementWithPrediction()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null || _networkController == null)
            {
                return;
            }

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
            if (_movementController == null || _stateController == null || _networkController == null)
            {
                return;
            }

            _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime);
            PlayerStateData authState = _networkController.CreateStateData(transform, _rigidbody, input.SequenceNumber);
            ReconcileStateClientRpc(authState);
        }

        [ClientRpc]
        private void ReconcileStateClientRpc(PlayerStateData serverState)
        {
            if (IsServer || _networkController == null || _movementController == null || _stateController == null)
            {
                return;
            }

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
            if (IsLocalPlayer)
            {
                return;
            }
        }

        [ServerRpc]
        private void UseAbilityServerRpc() => UseAbilityClientRpc();

        [ClientRpc]
        private void UseAbilityClientRpc()
        {
            if (IsLocalPlayer)
            {
                return;
            }
        }

        #endregion

        #region Character Class Management

        private void SetupCharacterClass()
        {
            if (_characterService == null)
            {
                GameLogger.LogError("Character service not available");
                return;
            }

            // Apply GDD chef stats from the selected ChefDefinition
            ChefDefinition selectedChef = _characterService.SelectedChef;
            if (selectedChef != null && _movementController != null)
            {
                ChefStatBlock stats = selectedChef.Stats;
                _movementController.MovementSpeed = _baseMovementSpeed * stats.MoveSpeed;
                InteractionSpeed.BaseValue = stats.InteractRange;
                CarryingCapacity.BaseValue = stats.CarryCapacity;
            }

            // Legacy SO-based character class (skins, ability prefab data)
            CharacterClass = _characterService.SelectedCharacter;
            if (CharacterClass != null)
            {
                _characterClassId = CharacterClass.Id;
            }
            else
            {
                // Fallback: lookup SO by name match via Resources
                CharacterClass[] allClasses = Resources.LoadAll<CharacterClass>("ScriptableObjects/CharacterClasses");
                foreach (CharacterClass cc in allClasses)
                {
                    if (cc != null && cc.DisplayName == selectedChef?.DisplayName)
                    {
                        CharacterClass = cc;
                        _characterClassId = cc.Id;
                        break;
                    }
                }
            }

            if (CharacterClass != null)
            {
                PrimaryAbility = CharacterAbility.CreateAbility(
                    CharacterClass.PrimaryAbility != null ? CharacterClass.PrimaryAbility.AbilityType : AbilityType.None,
                    CharacterClass, this);
            }

            // Register chef abilities with AbilityService (Phase 8)
            if (selectedChef != null)
            {
                // AbilityService is resolved by MatchLifetimeScope; access via service locator pattern
                IAbilityService abilityService = FindAbilityService();
                if (abilityService != null)
                {
                    abilityService.RegisterChefAbilities(selectedChef.Id);
                    GameLogger.Log($"[AbilityService] Registered abilities for {selectedChef.DisplayName}");
                }
            }

            EnsureValidSkinForCharacter();
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
            if (IsLocalPlayer)
            {
                return;
            }

            _characterClassId = characterClassId;
            SetupCharacterClass();
        }

        #endregion

        #region Skins

        public string GetSkinId() => _skinId.Value.ToString();

        public void SetSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
            {
                GameLogger.LogWarning("Cannot set empty skin id");
                return;
            }

            if (IsServer)
            {
                SetSkinInternal(skinId);
                return;
            }

            if (IsLocalPlayer)
            {
                SetSkinServerRpc(skinId);
            }
        }

        [ServerRpc]
        private void SetSkinServerRpc(string skinId)
        {
            SetSkinInternal(skinId);
        }

        private void SetSkinInternal(string skinId)
        {
            if (!IsServer)
            {
                return;
            }

            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                GameLogger.LogWarning("No skins available for current character");
                _skinId.Value = default;
                return;
            }

            bool existsForCharacter = CharacterClass.Skins.Any(s => s != null && s.id == skinId);
            if (!existsForCharacter)
            {
                GameLogger.LogWarning($"Skin '{skinId}' does not belong to character '{CharacterClass.DisplayName}', falling back to default");
                _skinId.Value = new FixedString64Bytes(GetDefaultSkinIdForCharacter() ?? string.Empty);
                return;
            }

            _skinId.Value = new FixedString64Bytes(skinId);
        }

        private void InitializeSkinSystem()
        {
            if (IsServer)
            {
                EnsureSkinInitialized();
            }

            _skinId.OnValueChanged += OnSkinIdChanged;
            ApplySkin(_skinId.Value);
        }

        private void CleanupSkinSystem()
        {
            _skinId.OnValueChanged -= OnSkinIdChanged;

            if (_skinInstance != null)
            {
                Destroy(_skinInstance);
                _skinInstance = null;
            }

            if (_fallbackModelRenderer != null)
            {
                _fallbackModelRenderer.enabled = true;
            }
        }

        private void OnSkinIdChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            ApplySkin(newValue);
        }

        private void EnsureSkinInitialized()
        {
            if (!IsServer)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_skinId.Value.ToString()))
            {
                EnsureValidSkinForCharacter();
                return;
            }

            string initialSkinId = SelectInitialSkinId();
            if (!string.IsNullOrEmpty(initialSkinId))
            {
                _skinId.Value = new FixedString64Bytes(initialSkinId);
            }
        }

        private void EnsureValidSkinForCharacter()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return;
            }

            string currentSkinId = _skinId.Value.ToString();
            if (string.IsNullOrEmpty(currentSkinId))
            {
                if (IsServer)
                {
                    string initialSkinId = SelectInitialSkinId();
                    if (!string.IsNullOrEmpty(initialSkinId))
                    {
                        _skinId.Value = new FixedString64Bytes(initialSkinId);
                    }
                }
                return;
            }

            if (!string.IsNullOrEmpty(currentSkinId) && CharacterClass.Skins.Any(s => s != null && s.id == currentSkinId))
            {
                return;
            }

            if (IsServer)
            {
                string defaultSkinId = GetDefaultSkinIdForCharacter();
                if (!string.IsNullOrEmpty(defaultSkinId))
                {
                    _skinId.Value = new FixedString64Bytes(defaultSkinId);
                }
            }
        }

        private string SelectInitialSkinId()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            if (IsBotNetworkObject() && _randomizeBotSkin && CharacterClass.Skins.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, CharacterClass.Skins.Count);
                return CharacterClass.Skins[index]?.id;
            }

            return GetDefaultSkinIdForCharacter();
        }

        private string GetDefaultSkinIdForCharacter()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            SkinItem defaultSkin = CharacterClass.Skins.FirstOrDefault(s => s != null && s.isDefault);
            defaultSkin ??= CharacterClass.Skins.FirstOrDefault(s => s != null);
            return defaultSkin?.id;
        }

        private SkinItem GetSkinItem(string skinId)
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(skinId))
            {
                return null;
            }

            return CharacterClass.Skins.FirstOrDefault(s => s != null && s.id == skinId);
        }

        private void ApplySkin(FixedString64Bytes skinIdValue)
        {
            string skinId = skinIdValue.ToString();
            SkinItem skin = GetSkinItem(skinId) ?? GetSkinItem(GetDefaultSkinIdForCharacter());

            Transform root = GetOrFindSkinRoot();
            if (root == null)
            {
                return;
            }

            EnsureFallbackRendererCached(root);

            if (_skinInstance != null)
            {
                Destroy(_skinInstance);
                _skinInstance = null;
            }

            if (skin == null || skin.prefab == null)
            {
                SetFallbackModelVisible(true);
                return;
            }

            SetFallbackModelVisible(false);

            _skinInstance = Instantiate(skin.prefab, root);
            _skinInstance.transform.localPosition = Vector3.zero;
            _skinInstance.transform.localRotation = Quaternion.identity;
            _skinInstance.transform.localScale = Vector3.one;
        }

        private Transform GetOrFindSkinRoot()
        {
            if (_skinRoot != null)
            {
                return _skinRoot;
            }

            _skinRoot = transform.Find("Model");
            return _skinRoot;
        }

        private void EnsureFallbackRendererCached(Transform root)
        {
            if (_fallbackModelRenderer != null)
            {
                return;
            }

            _fallbackModelRenderer = root.GetComponent<MeshRenderer>();
        }

        private void SetFallbackModelVisible(bool isVisible)
        {
            if (_fallbackModelRenderer == null)
            {
                return;
            }

            _fallbackModelRenderer.enabled = isVisible;
        }

        private bool IsBotNetworkObject()
        {
            return NetworkObject != null && !NetworkObject.IsPlayerObject;
        }

        #endregion

        #region Object Carrying

        public bool PickUpObject(GameObject obj)
        {
            if (_heldObject != null || _holdPoint == null)
            {
                return false;
            }

            _heldObject = obj;
            obj.transform.SetParent(_holdPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = true;
            }

            return true;
        }

        public GameObject DropObject()
        {
            if (_heldObject == null)
            {
                return null;
            }

            GameObject obj = _heldObject;
            _heldObject = null;
            obj.transform.SetParent(null);

            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = false;
            }

            return obj;
        }

        public GameObject GetHeldObject() => _heldObject;
        public bool IsHoldingObject() => _heldObject != null;

        // ── v2 combat / carry API ─────────────────────────────────────────

        /// <summary>Maximum carried items (always 1 for the held object model).</summary>
        public bool IsCarryingMaxItems => _heldObject != null || HasCarriedDish;

        /// <summary>
        /// True when the player holds a v2 collected dish.
        /// Uses NetworkVariables so clients can evaluate interaction prompts.
        /// </summary>
        public bool HasCarriedDish =>
            IsServer
                ? _carriedItemData.HasValue
                : _carriedRecipeTier.Value >= 1;

        /// <summary>
        /// Called by AutonomousCookingStation / LootPickup when the player collects a dish.
        /// Server-authoritative; replicates tier/type for client prompts and local visual.
        /// </summary>
        public void ReceiveCollectedDish(int recipeTier, KitchenClash.Domain.IngredientType ingredientType)
        {
            if (!IsServer)
            {
                return;
            }

            int tier = Mathf.Clamp(recipeTier, 1, 3);
            _carriedItemData = new KitchenClash.Infrastructure.Network.CarriedItemData(ingredientType, tier);
            _carriedRecipeTier.Value = tier;
            _carriedIngredientType.Value = (int)ingredientType;
            GameLogger.Log($"[PlayerController] Received dish T{tier} ({ingredientType})");
        }

        /// <summary>
        /// Server-only: consume the carried v2 dish for delivery scoring.
        /// Returns false if nothing was carried.
        /// </summary>
        public bool TryConsumeCarriedDish(out CarriedItemData dish)
        {
            dish = default;
            if (!IsServer || !_carriedItemData.HasValue)
            {
                return false;
            }

            dish = _carriedItemData.Value;
            ClearCarriedDishState();
            return true;
        }

        /// <summary>
        /// Clears all carried items and returns them as data snapshots for loot drop.
        /// Called by PlayerCombatController on KO (server-only).
        /// </summary>
        public System.Collections.Generic.List<KitchenClash.Infrastructure.Network.CarriedItemData> GetAndClearCarriedItems()
        {
            var items = new System.Collections.Generic.List<KitchenClash.Infrastructure.Network.CarriedItemData>();

            // Return v2 dish data stored when ReceiveCollectedDish was called
            if (_carriedItemData.HasValue)
            {
                items.Add(_carriedItemData.Value);
                ClearCarriedDishState();
            }

            // Despawn any physical held object
            if (_heldObject != null)
            {
                var netObj = _heldObject.GetComponent<Unity.Netcode.NetworkObject>();
                netObj?.Despawn(destroy: true);
                _heldObject = null;
            }

            return items;
        }

        private void ClearCarriedDishState()
        {
            _carriedItemData = null;
            if (IsServer)
            {
                _carriedRecipeTier.Value = -1;
                _carriedIngredientType.Value = 0;
            }
        }

        private void OnCarriedDishChanged(int previousTier, int newTier)
        {
            RefreshCarriedDishVisual(newTier);
        }

        private void RefreshCarriedDishVisual(int tier)
        {
            if (_carriedDishVisual != null)
            {
                Destroy(_carriedDishVisual);
                _carriedDishVisual = null;
            }

            if (tier < 1)
            {
                return;
            }

            Transform parent = _holdPoint != null ? _holdPoint : transform;
            _carriedDishVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _carriedDishVisual.name = $"CarriedDish_T{tier}";
            _carriedDishVisual.transform.SetParent(parent, worldPositionStays: false);
            _carriedDishVisual.transform.localPosition = Vector3.zero;
            _carriedDishVisual.transform.localScale = Vector3.one * (0.25f + 0.05f * tier);

            // Visual-only — disable physics so it never blocks interaction spheres.
            Collider col = _carriedDishVisual.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            if (_carriedDishVisual.TryGetComponent(out Renderer renderer))
            {
                // Tier tint: T1 warm, T2 orange, T3 red
                Color color = tier switch
                {
                    1 => new Color(0.95f, 0.85f, 0.45f),
                    2 => new Color(0.95f, 0.55f, 0.2f),
                    _ => new Color(0.9f, 0.25f, 0.2f),
                };
                renderer.material.color = color;
            }
        }

        /// <summary>Show or hide the player's visual model (used during KO / respawn).</summary>
        public void SetVisible(bool visible)
        {
            // Enable/disable all renderers on this object and children
            foreach (var r in GetComponentsInChildren<UnityEngine.Renderer>(includeInactive: true))
                r.enabled = visible;
        }

        /// <summary>Enable or disable input processing (used during KO / respawn).</summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputHandler?.SetEnabled(enabled);
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

        /// <summary>
        /// Returns injected IPlayerNetworkManager. Never throws if null.
        /// </summary>
        private IPlayerNetworkManager ResolvePlayerNetworkManager()
        {
            return _playerNetworkManager;
        }

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
    }
}
