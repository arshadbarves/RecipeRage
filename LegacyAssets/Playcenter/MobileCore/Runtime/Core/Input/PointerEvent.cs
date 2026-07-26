namespace Playcenter.MobileCore
{
    public enum PointerPhase : byte
    {
        Began,
        Moved,
        Ended,
        Cancelled,
    }

    /// <summary>
    /// Raw pointer sample in screen pixels, normalized into stick space by DualStickModel.
    /// HalfWidth/HalfHeight carry the screen half-extents so the model stays resolution-aware
    /// without touching UnityEngine.Screen.
    /// </summary>
    public readonly struct PointerEvent
    {
        public int PointerId { get; }
        public float X { get; }
        public float Y { get; }
        public PointerPhase Phase { get; }
        public float HalfWidth { get; }
        public float HalfHeight { get; }

        public PointerEvent(int pointerId, float x, float y, PointerPhase phase, float halfWidth, float halfHeight)
        {
            PointerId = pointerId;
            X = x;
            Y = y;
            Phase = phase;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }
    }
}
