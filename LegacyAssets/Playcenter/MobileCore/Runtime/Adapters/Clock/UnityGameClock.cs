using System;
using UnityEngine;

namespace Playcenter.MobileCore
{
    /// <summary>Runtime clock — ticked by PlaycenterBootstrap.Update with Time.deltaTime.</summary>
    public sealed class UnityGameClock : IGameClock
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
