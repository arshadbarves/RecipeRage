using UnityEngine;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// GDD Section 8: Brawl Stars fixed dual-joystick.
    /// Only MonoBehaviour that handles input.
    /// Implements Domain.IDualStickInput via float pairs.
    /// </summary>
    public sealed class InputReceiver : MonoBehaviour, KitchenClash.Domain.IDualStickInput
    {
        private const float R = 60f;
        private const float ChopTapMaxDuration = 0.15f;
        private const float ChopTapMaxDrag = 20f;
        private const float ChopSessionTimeout = 0.5f;

        private static readonly Vector2 LB = new(150, 150);
        private static readonly Vector2 RB = new(-150, 150);

        public Vector2 MoveInput { get; private set; }
        public Vector2 AimInput { get; private set; }

        // IDualStickInput (pure C# floats)
        public float MoveInputX => MoveInput.x;
        public float MoveInputY => MoveInput.y;
        public float AimInputX => AimInput.x;
        public float AimInputY => AimInput.y;
        public bool AimJustReleased { get; private set; }
        public bool AbilityPressed { get; private set; }
        public bool SuperPressed { get; private set; }
        public bool GadgetPressed { get; private set; }

        // Chop tap detection
        public bool ChopTapped { get; private set; }
        public int ChopTapCount { get; private set; }

        // Right-side tap tracking per finger
        private int _rightTouchId = -1;
        private float _rightTouchStartTime;
        private Vector2 _rightTouchStartPos;
        private float _lastChopTapTime = -1f;

        private void Update()
        {
            AimJustReleased = false;
            AbilityPressed = false;
            SuperPressed = false;
            GadgetPressed = false;
            ChopTapped = false;

            // Reset chop session if idle too long
            if (_lastChopTapTime >= 0f && Time.time - _lastChopTapTime > ChopSessionTimeout)
            {
                ChopTapCount = 0;
                _lastChopTapTime = -1f;
            }

            foreach (var t in UnityEngine.Input.touches)
            {
                bool left = t.position.x < Screen.width * 0.5f;
                if (left)
                {
                    MoveInput = Clamp(t.position - LB);
                }
                else
                {
                    // Track right-side touch for chop tap detection
                    if (t.phase == TouchPhase.Began)
                    {
                        _rightTouchId = t.fingerId;
                        _rightTouchStartTime = Time.time;
                        _rightTouchStartPos = t.position;
                    }

                    var v = Clamp(t.position - RB);
                    AimJustReleased = t.phase == TouchPhase.Ended && AimInput.magnitude > 0.3f;

                    // Detect chop tap: short duration, minimal drag
                    if (t.phase == TouchPhase.Ended && t.fingerId == _rightTouchId)
                    {
                        float duration = Time.time - _rightTouchStartTime;
                        float drag = (t.position - _rightTouchStartPos).magnitude;

                        if (duration <= ChopTapMaxDuration && drag <= ChopTapMaxDrag)
                        {
                            ChopTapped = true;
                            ChopTapCount++;
                            _lastChopTapTime = Time.time;
                        }

                        _rightTouchId = -1;
                    }

                    AimInput = v;
                }
            }
        }

        private Vector2 Clamp(Vector2 v) =>
            v.magnitude > R ? v.normalized : v / R;
    }
}
