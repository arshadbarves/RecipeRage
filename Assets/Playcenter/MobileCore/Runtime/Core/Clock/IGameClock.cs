using System;

namespace Playcenter.MobileCore
{
    /// <summary>Single time source for all Core logic. No Time./DateTime. in Core.</summary>
    public interface IGameClock
    {
        float DeltaTime { get; }
        float Elapsed { get; }
        event Action<float> Ticked;
        void Tick(float deltaTime);
    }
}
