using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Top-down movement + proximity interaction. Single-player for now;
    /// NetworkPlayer wraps this in Slice 2.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController _characterController;
        private IInputService _input;
        private IConfigService _config;
        private float _moveSpeed;
        private float _interactRange;

        [HideInInspector] public bool LocalSimulationEnabled = true;

        public PlayerCarry Carry { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            _input = ServiceLocator.Get<IInputService>();
            _config = ServiceLocator.Get<IConfigService>();
            _moveSpeed = _config.Get(ConfigKeys.PlayerMoveSpeed, ConfigKeys.Defaults.PlayerMoveSpeed);
            _interactRange = _config.Get(ConfigKeys.InteractRange, ConfigKeys.Defaults.InteractRange);
            Carry = new PlayerCarry(_config);
        }

        private void Update()
        {
            if (!LocalSimulationEnabled)
            {
                return;
            }

            SimulateMove(_input.MoveAxis, Time.deltaTime);

            if (_input.InteractPressed)
            {
                TryInteract();
            }
        }

        /// <summary>
        /// Called by NetworkPlayer (server) or Update (offline). Never both.
        /// </summary>
        public void SimulateMove(Vector2 moveAxis, float deltaTime)
        {
            var move = new Vector3(moveAxis.x, 0f, moveAxis.y);
            _characterController.Move(move * (_moveSpeed * deltaTime));
        }

        /// <summary>Server-side interaction entry (from NetworkPlayer RPC).</summary>
        public void InteractFromNetwork()
        {
            TryInteract();
        }

        private void TryInteract()
        {
            var nearest = FindNearestInteractable();
            if (nearest != null && nearest.CanInteract(this))
            {
                nearest.Interact(this);
            }
        }

        private IInteractable FindNearestInteractable()
        {
            var hits = Physics.OverlapSphere(transform.position, _interactRange);
            IInteractable nearest = null;
            var nearestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    continue;
                }

                var dist = (hit.transform.position - transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = interactable;
                }
            }
            return nearest;
        }
    }
}
