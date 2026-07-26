namespace Playcenter
{
    public interface ITimeService
    {
        float Time { get; }
        float DeltaTime { get; }
        float UnscaledTime { get; }
        float UnscaledDeltaTime { get; }
    }
}
