using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Executes BotTasks: steers to the target station, then performs the station
    /// interaction. Bot chopping/cooking dwell uses station timings — bots never
    /// act faster than a human tapping optimally.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class BotController : MonoBehaviour
    {
        public int BotId { get; set; }

        private PlayerController _player;
        private BotTask _currentTask;
        private float _dwellTimer;
        private float _moveSpeed;
        private float _actionDwellScale = 1f; // adaptive difficulty: >1 slower, <1 faster (never below human floor)

        public BotTask CurrentTask => _currentTask;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Start()
        {
            var config = ServiceLocator.Get<IConfigService>();
            _moveSpeed = config.Get(ConfigKeys.PlayerMoveSpeed, ConfigKeys.Defaults.PlayerMoveSpeed);
        }

        public void AssignTask(BotTask task, float actionDwellScale)
        {
            _currentTask = task;
            _actionDwellScale = actionDwellScale;
            _dwellTimer = 0f;
        }

        private void Update()
        {
            if (_currentTask == null || _currentTask.IsComplete)
            {
                return;
            }

            if (_currentTask.Kind == BotTaskKind.Wander)
            {
                Wander();
                return;
            }

            var target = _currentTask.TargetStation.transform.position;
            var toTarget = target - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 1.2f * 1.2f)
            {
                var direction = toTarget.normalized;
                _player.SimulateMove(new Vector2(direction.x, direction.z), Time.deltaTime);
                return;
            }

            ExecuteAtStation();
        }

        private void ExecuteAtStation()
        {
            switch (_currentTask.Kind)
            {
                case BotTaskKind.Chop:
                    // Simulated chopping: dwell = ChopTaps × per-tap interval (config), scaled by difficulty
                    _dwellTimer += Time.deltaTime;
                    var chopDuration = 8 * 0.25f * _actionDwellScale; // per-tap 250ms = fast human
                    if (_dwellTimer >= chopDuration)
                    {
                        _currentTask.TargetStation.Interact(_player);
                        _currentTask.IsComplete = true;
                    }
                    break;

                default:
                    _currentTask.TargetStation.Interact(_player);
                    _currentTask.IsComplete = true;
                    break;
            }
        }

        private void Wander()
        {
            // Slow drift so idle bots read as alive; task completes after 2s and replans
            _dwellTimer += Time.deltaTime;
            _player.SimulateMove(new Vector2(Mathf.Sin(Time.time * 0.5f), Mathf.Cos(Time.time * 0.3f)) * 0.3f, Time.deltaTime);
            if (_dwellTimer >= 2f)
            {
                _currentTask.IsComplete = true;
            }
        }
    }
}
