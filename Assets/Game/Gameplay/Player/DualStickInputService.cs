using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dual-stick touch input. Renders two virtual sticks (UI lands in Slice 5);
    /// until then reads editor keyboard (WASD + mouse buttons) so gameplay is
    /// testable immediately. Touch stick logic slots into Tick() without
    /// changing the interface.
    /// </summary>
    public sealed class DualStickInputService : IInputService
    {
        public Vector2 MoveAxis { get; private set; }
        public Vector2 AimAxis { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool ChopPressed { get; private set; }

        public void Tick()
        {
            // Editor/dev fallback; touch sticks replace this body in Slice 5.
            MoveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (MoveAxis.sqrMagnitude > 1f)
            {
                MoveAxis.Normalize();
            }

            AimAxis = Vector2.zero;
            InteractPressed = Input.GetMouseButtonDown(0);
            ChopPressed = Input.GetMouseButtonDown(1);
        }
    }
}
