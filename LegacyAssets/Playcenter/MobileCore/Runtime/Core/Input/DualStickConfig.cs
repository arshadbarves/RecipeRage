namespace Playcenter.MobileCore
{
    /// <summary>Tuning for dual-stick behavior. Fill from IConfigService (mc_input_* keys).</summary>
    public readonly struct DualStickConfig
    {
        public float Deadzone { get; }
        public float TapWindowSeconds { get; }
        public float TapIdleResetSeconds { get; }

        public DualStickConfig(
            float deadzone = 0.15f,
            float tapWindowSeconds = 0.3f,
            float tapIdleResetSeconds = 0.5f)
        {
            Deadzone = deadzone;
            TapWindowSeconds = tapWindowSeconds;
            TapIdleResetSeconds = tapIdleResetSeconds;
        }
    }
}
