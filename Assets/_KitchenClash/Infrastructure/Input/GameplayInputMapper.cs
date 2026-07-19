using Playcenter.Services;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Pure math helpers mapping raw device state → <see cref="InputAxis2"/>.
    /// No UnityEngine dependency — safe for EditMode unit tests.
    /// </summary>
    public static class GameplayInputMapper
    {
        /// <summary>
        /// Maps WASD / arrow keys to a magnitude-clamped axis (max length 1).
        /// </summary>
        public static InputAxis2 FromKeyboard(
            bool w, bool a, bool s, bool d,
            bool up, bool left, bool down, bool right)
        {
            float x = 0f;
            float y = 0f;

            if (d || right)
            {
                x += 1f;
            }

            if (a || left)
            {
                x -= 1f;
            }

            if (w || up)
            {
                y += 1f;
            }

            if (s || down)
            {
                y -= 1f;
            }

            return ClampMagnitude(new InputAxis2(x, y), 1f);
        }

        /// <summary>
        /// Maps a virtual stick sample in [-1, 1]. Values inside deadzone become Zero.
        /// Outside deadzone the vector is magnitude-clamped to 1.
        /// </summary>
        public static InputAxis2 FromVirtualStick(float x, float y, float deadZone = 0.1f)
        {
            if (deadZone < 0f)
            {
                deadZone = 0f;
            }

            InputAxis2 raw = new InputAxis2(x, y);
            float mag = raw.Magnitude;
            if (mag <= deadZone)
            {
                return InputAxis2.Zero;
            }

            return ClampMagnitude(raw, 1f);
        }

        public static InputAxis2 ClampMagnitude(InputAxis2 value, float maxLength)
        {
            if (maxLength <= 0f)
            {
                return InputAxis2.Zero;
            }

            float mag = value.Magnitude;
            if (mag <= maxLength || mag <= 0.0001f)
            {
                return value;
            }

            float scale = maxLength / mag;
            return new InputAxis2(value.X * scale, value.Y * scale);
        }
    }
}
