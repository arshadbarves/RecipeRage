using Core.Logging;
using Core.Networking.Models;
using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.Shared;
using Gameplay.Stations;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network.Bot
{
    /// <summary>
    /// Server-authoritative task-driven bot controller.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        [SerializeField] private float _replanInterval = 0.4f;
        [SerializeField] private float _interactionDistance = 1.15f;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _stuckTimeout = 2.5f;
        [SerializeField] private bool _enableAI = true;

        private BotPlayer _botData;
        private PlayerController _playerController;
        private readonly BotTaskPlanner _planner = new BotTaskPlanner();
        private BotKitchenSnapshot _snapshot;
        private readonly BotClaimRegistry _claimRegistry = BotClaimRegistry.Shared;

        [Inject] private IMatchContext _matchContext;

        private BotTaskPlan _currentPlan;
        private float _nextReplanTime;
        private float _lastInteractionTime;
        private Vector3 _lastPosition;
        private float _stuckTimer;
        private bool _isInitialized;

        public void Initialize(BotPlayer botData)
        {
            _botData = botData;
            _isInitialized = botData != null;
            GameLogger.Log($"[BotController] Initialized bot: {_botData?.BotName ?? "Unknown"}");
        }

        private void Awake()
        {
            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }
            else
            {
                GameLogger.LogWarning("[BotController] LifetimeScope not found. Bot runtime injection will be unavailable.");
            }

            _playerController = GetComponent<PlayerController>();
            _snapshot = new BotKitchenSnapshot(_matchContext);
            _lastPosition = transform.position;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
            {
                enabled = false;
                return;
            }

            _nextReplanTime = Time.time;
            _lastInteractionTime = Time.time;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ReleaseClaims();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ReleaseClaims();
        }

        private void Update()
        {
            if (!IsServer || !_enableAI || !_isInitialized || _playerController == null)
            {
                return;
            }

            EnsureSnapshot();
            if (_snapshot == null)
            {
                return;
            }

            if (CheckForStuck())
            {
                RecoverFromFailure();
            }

            if (_currentPlan == null || Time.time >= _nextReplanTime)
            {
                Replan();
            }

            ExecuteCurrentPlan();
        }

        public BotPlayer GetBotData()
        {
            return _botData;
        }

        public bool IsBot()
        {
            return _isInitialized && _botData != null;
        }

        private void Replan()
        {
            if (_snapshot == null || _botData == null)
            {
                _currentPlan = BotTaskPlan.Idle();
                _nextReplanTime = Time.time + _replanInterval;
                return;
            }

            var planningSnapshot = _snapshot.Capture(_playerController, _botData.BotId, _claimRegistry);
            if (ClaimedOrderNeedsRelease(planningSnapshot))
            {
                ReleaseClaims();
                planningSnapshot = _snapshot.Capture(_playerController, _botData.BotId, _claimRegistry);
            }

            _currentPlan = _planner.Plan(planningSnapshot);
            _nextReplanTime = Time.time + _replanInterval;

            switch (_currentPlan.Type)
            {
                case BotTaskType.ClaimOrder:
                    HandleOrderClaim();
                    break;
                case BotTaskType.Recover:
                    RecoverFromFailure();
                    break;
            }
        }

        private static bool ClaimedOrderNeedsRelease(BotPlanningSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.ClaimedOrderId.HasValue)
            {
                return false;
            }

            foreach (BotOrderDescriptor order in snapshot.Orders)
            {
                if (order.OrderId != snapshot.ClaimedOrderId.Value)
                {
                    continue;
                }

                return order.IsExpired || order.IsCompleted || order.HasInvalidAssembly;
            }

            return true;
        }

        private void ExecuteCurrentPlan()
        {
            if (_currentPlan == null)
            {
                return;
            }

            switch (_currentPlan.Type)
            {
                case BotTaskType.Idle:
                    return;
                case BotTaskType.FetchIngredient:
                case BotTaskType.ProcessIngredient:
                case BotTaskType.AcquirePlate:
                case BotTaskType.AssembleDish:
                case BotTaskType.ServeDish:
                case BotTaskType.WashDishes:
                    MoveAndInteractWithTarget();
                    return;
            }
        }

        private void MoveAndInteractWithTarget()
        {
            Component target = _snapshot.ResolveStation(_currentPlan.TargetStationId);
            if (target == null)
            {
                _currentPlan = null;
                _nextReplanTime = Time.time;
                return;
            }

            Vector3 targetPosition = target.transform.position;
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance > _interactionDistance)
            {
                MoveTowards(targetPosition);
                return;
            }

            if (Time.time - _lastInteractionTime < 0.15f)
            {
                return;
            }

            if (target is StationBase station)
            {
                station.Interact(_playerController);
                _lastInteractionTime = Time.time;
                _nextReplanTime = Time.time;
            }
        }

        private void MoveTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 step = direction.normalized * _moveSpeed * Time.deltaTime;
            if (step.sqrMagnitude > direction.sqrMagnitude)
            {
                step = direction;
            }

            transform.position += step;
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        private bool CheckForStuck()
        {
            if (_currentPlan == null || string.IsNullOrWhiteSpace(_currentPlan.TargetStationId))
            {
                _lastPosition = transform.position;
                _stuckTimer = 0f;
                return false;
            }

            Component target = _snapshot.ResolveStation(_currentPlan.TargetStationId);
            if (target == null)
            {
                return true;
            }

            float movedDistance = Vector3.Distance(transform.position, _lastPosition);
            float targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (movedDistance < 0.02f && targetDistance > _interactionDistance + 0.25f)
            {
                _stuckTimer += Time.deltaTime;
            }
            else
            {
                _stuckTimer = 0f;
            }

            _lastPosition = transform.position;
            return _stuckTimer >= _stuckTimeout;
        }

        private void HandleOrderClaim()
        {
            if (_botData == null || !_currentPlan.OrderId.HasValue)
            {
                _currentPlan = null;
                return;
            }

            if (!_claimRegistry.TryClaimOrder(_currentPlan.OrderId.Value, _botData.BotId))
            {
                _currentPlan = null;
                return;
            }

            CounterStation counter = _snapshot.FindNearestAvailableCounter(transform.position, _botData.BotId, _claimRegistry);
            if (counter != null)
            {
                _claimRegistry.AssignCounter(_currentPlan.OrderId.Value, counter.NetworkObject != null && counter.NetworkObject.IsSpawned
                    ? counter.NetworkObject.NetworkObjectId.ToString()
                    : counter.GetInstanceID().ToString());
            }

            _currentPlan = null;
            _nextReplanTime = Time.time;
        }

        private void RecoverFromFailure()
        {
            ReleaseClaims();

            if (_playerController != null && _playerController.IsHoldingObject())
            {
                GameObject heldObject = _playerController.GetHeldObject();
                IngredientItem heldIngredient = heldObject != null ? heldObject.GetComponent<IngredientItem>() : null;

                _playerController.DropObject();

                if (heldIngredient != null && heldIngredient.IsBurned && heldIngredient.NetworkObject != null && heldIngredient.NetworkObject.IsSpawned)
                {
                    heldIngredient.NetworkObject.Despawn(true);
                }
            }

            _currentPlan = null;
            _nextReplanTime = Time.time + 0.1f;
            _stuckTimer = 0f;
        }

        private void ReleaseClaims()
        {
            if (_botData != null)
            {
                _claimRegistry.ReleaseOrderForBot(_botData.BotId);
            }
        }

        private void EnsureSnapshot()
        {
            if (_snapshot == null && _matchContext != null)
            {
                _snapshot = new BotKitchenSnapshot(_matchContext);
            }
        }
    }
}
