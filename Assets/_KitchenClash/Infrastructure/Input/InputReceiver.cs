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

        private void Update()
        {
            AimJustReleased = false;
            AbilityPressed = false;
            SuperPressed = false;
            GadgetPressed = false;

            foreach (var t in UnityEngine.Input.touches)
            {
                bool left = t.position.x < Screen.width * 0.5f;
                if (left)
                {
                    MoveInput = Clamp(t.position - LB);
                }
                else
                {
                    var v = Clamp(t.position - RB);
                    AimJustReleased = t.phase == TouchPhase.Ended && AimInput.magnitude > 0.3f;
                    AimInput = v;
                }
            }
        }

        private Vector2 Clamp(Vector2 v) =>
            v.magnitude > R ? v.normalized : v / R;
    }
}
