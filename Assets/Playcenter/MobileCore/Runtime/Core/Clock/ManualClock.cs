using System;

namespace Playcenter.MobileCore
{
    /// <summary>Test/headless clock — caller drives ticks explicitly.</summary>
    public sealed class ManualClock : IGameClock
    {
        public float DeltaTime { get; private set; }
        public float Elapsed { get; private set; }
        public event Action<float> Ticked;

        public void Tick(float deltaTime)
        {
            DeltaTime = deltaTime;
            Elapsed += deltaTime;
            Ticked?.Invoke(deltaTime);
        }
    }
}
