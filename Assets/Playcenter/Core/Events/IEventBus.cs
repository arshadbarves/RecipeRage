using System;

namespace Playcenter
{
    /// <summary>
    /// Lightweight typed pub/sub. Gameplay publishes, systems subscribe. No per-frame allocs.
    /// </summary>
    public interface IEventBus
    {
        void Publish<T>(T eventData);
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Clear();
    }
}
