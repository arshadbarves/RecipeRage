namespace Playcenter.Services
{
    /// <summary>
    /// Engine-free 2D input axis (replaces Vector2 on service ports).
    /// </summary>
    public readonly struct InputAxis2
    {
        public float X { get; }
        public float Y { get; }

        public InputAxis2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static InputAxis2 Zero => new InputAxis2(0f, 0f);

        public float SqrMagnitude => (X * X) + (Y * Y);

        public float Magnitude
        {
            get
            {
                double sq = SqrMagnitude;
                if (sq <= 0d)
                {
                    return 0f;
                }

                return (float)System.Math.Sqrt(sq);
            }
        }

        /// <summary>
        /// Returns a unit-length axis, or Zero when below epsilon.
        /// </summary>
        public InputAxis2 Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag <= 0.0001f)
                {
                    return Zero;
                }

                return new InputAxis2(X / mag, Y / mag);
            }
        }
    }
}
