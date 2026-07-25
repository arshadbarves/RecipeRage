namespace Playcenter.MobileCore
{
    /// <summary>Per-tick CPU budget for bot planning. Never plan unbounded.</summary>
    public interface IBotBudget
    {
        bool TryConsume(float milliseconds);
        void ResetTick();
    }
}
