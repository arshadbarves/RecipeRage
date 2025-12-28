using System;
using Core.Events;

namespace Tests.Editor.Mocks
{
    public class MockEventBus : IEventBus
    {
        public object LastPublishedEvent;

        public void Initialize() { }
        public void Publish<T>(T eventMessage) where T : class { LastPublishedEvent = eventMessage; }
        public void Subscribe<T>(Action<T> action) where T : class { }
        public void Unsubscribe<T>(Action<T> action) where T : class { }
        public void ClearSubscriptions<T>() where T : class { }
        public void ClearAllSubscriptions() { }
    }
}
