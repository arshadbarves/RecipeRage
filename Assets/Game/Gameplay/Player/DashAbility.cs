using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Gustavo's active ability: 3m burst in move direction, cooldown per chef
    /// level (30s → 10s at L10). Usable once per cooldown; input: double-tap
    /// the move stick direction (mobile-friendly, no extra button).
    /// </summary>
    public sealed class DashAbility
    {
        private const float DashDistance = 3f;
        private const float DashDuration = 0.15f;
        private const float DoubleTapWindow = 0.3f;

        private readonly CharacterController _characterController;
        private readonly float _cooldownSec;

        private float _cooldownRemaining;
        private float _dashRemaining;
        private Vector3 _dashDirection;
        private Vector2 _lastMoveAxis;
        private float _timeSinceLastTap = float.MaxValue;

        public bool IsReady => _cooldownRemaining <= 0f;
        public float Cooldown01 => _cooldownSec <= 0f ? 1f : 1f - _cooldownRemaining / _cooldownSec;

        public DashAbility(CharacterController characterController, float cooldownSec)
        {
            _characterController = characterController;
            _cooldownSec = cooldownSec;
        }

        public void Tick(Vector2 moveAxis, float deltaTime)
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            _timeSinceLastTap += deltaTime;

            // Double-tap detection: stick released then pushed again within window
            if (moveAxis.sqrMagnitude < 0.1f && _lastMoveAxis.sqrMagnitude > 0.5f)
            {
                if (_timeSinceLastTap < DoubleTapWindow && IsReady)
                {
                    StartDash(_lastMoveAxis);
                }
                _timeSinceLastTap = 0f;
            }
            _lastMoveAxis = moveAxis;

            if (_dashRemaining > 0f)
            {
                _dashRemaining -= deltaTime;
                _characterController.Move(_dashDirection * (DashDistance / DashDuration * deltaTime));
            }
        }

        private void StartDash(Vector2 direction)
        {
            _dashDirection = new Vector3(direction.x, 0f, direction.y).normalized;
            _dashRemaining = DashDuration;
            _cooldownRemaining = _cooldownSec;
        }
    }
}
