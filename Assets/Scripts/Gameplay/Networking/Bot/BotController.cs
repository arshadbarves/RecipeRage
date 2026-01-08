using Core.Characters;
using Modules.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Networking.Bot
{
    /// <summary>
    /// Controls bot behavior - simple AI for bot players
    /// Attach this to the bot prefab (same prefab as PlayerController)
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        [SerializeField] private float _actionInterval = 2f; // Time between actions
        [SerializeField] private float _moveRadius = 10f; // How far bot can move
        [SerializeField] private bool _enableAI = true; // Toggle AI on/off

        private BotPlayer _botData;
        private PlayerController _playerController;
        private float _nextActionTime;
        private Vector3 _targetPosition;
        private bool _isInitialized;

        /// <summary>
        /// Initialize bot with data
        /// </summary>
        public void Initialize(BotPlayer botData)
        {
            _botData = botData;
            _isInitialized = true;

            GameLogger.Log($"[BotController] Initialized bot: {botData.BotName}");
        }

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Only server runs bot AI
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            // Set initial target
            _targetPosition = GetRandomPosition();
            _nextActionTime = Time.time + _actionInterval;

            GameLogger.Log($"[BotController] Bot spawned on server: {gameObject.name}");
        }

        private void Update()
        {
            if (!IsServer || !_enableAI || !_isInitialized)
            {
                return;
            }

            // Simple AI: Move to random positions periodically
            if (Time.time >= _nextActionTime)
            {
                PerformAction();
                _nextActionTime = Time.time + _actionInterval;
            }

            // Move towards target
            MoveTowardsTarget();
        }

        /// <summary>
        /// Perform a bot action
        /// </summary>
        private void PerformAction()
        {
            // Choose random action
            int action = Random.Range(0, 3);

            switch (action)
            {
                case 0:
                    // Move to new position
                    _targetPosition = GetRandomPosition();
                    GameLogger.Log($"[BotController] {_botData?.BotName ?? "Bot"} moving to {_targetPosition}");
                    break;

                case 1:
                    // Try to interact (placeholder - implement when interaction system is ready)
                    // _playerController.TryInteract();
                    break;

                case 2:
                    // Idle for a moment
                    _targetPosition = transform.position;
                    break;
            }
        }

        /// <summary>
        /// Move towards target position
        /// </summary>
        private void MoveTowardsTarget()
        {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            direction.y = 0; // Keep on ground

            // Simple movement (you can improve this with proper movement controller)
            if (Vector3.Distance(transform.position, _targetPosition) > 0.5f)
            {
                transform.position += direction * 3f * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Get random position within move radius
        /// </summary>
        private Vector3 GetRandomPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _moveRadius;
            return new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        /// <summary>
        /// Get bot data
        /// </summary>
        public BotPlayer GetBotData()
        {
            return _botData;
        }

        /// <summary>
        /// Check if this is a bot
        /// </summary>
        public bool IsBot()
        {
            return _isInitialized && _botData != null;
        }
    }
}
