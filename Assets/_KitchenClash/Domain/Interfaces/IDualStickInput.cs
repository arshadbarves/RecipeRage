namespace KitchenClash.Domain
{
    /// <summary>
    /// GDD Section 8: Dual-stick input contract.
    /// Uses float pairs instead of Vector2 to stay Unity-free.
    /// </summary>
    public interface IDualStickInput
    {
        float MoveInputX { get; }
        float MoveInputY { get; }
        float AimInputX { get; }
        float AimInputY { get; }
        bool AimJustReleased { get; }
        bool AbilityPressed { get; }
        bool SuperPressed { get; }
        bool GadgetPressed { get; }

        /// <summary>True for one frame when a rapid chop tap is detected on right side.</summary>
        bool ChopTapped { get; }

        /// <summary>Running tap count within current chop session (resets after 0.5s idle).</summary>
        int ChopTapCount { get; }
    }
}
