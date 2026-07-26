using System;

namespace KitchenClash.Infrastructure.Persistence
{
    [Serializable]
    public class PlayerInputData
    {
        public float Sensitivity = 1.0f;
        public bool InvertY;
        public bool UseGyroscope;
        public int ControlScheme;
    }
}
