using System;

namespace Playcenter.Shell
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : class;
        void Unsubscribe<T>(Action<T> handler) where T : class;
        void Publish<T>(T eventData) where T : class;
        void ClearAllSubscriptions();
    }
}
