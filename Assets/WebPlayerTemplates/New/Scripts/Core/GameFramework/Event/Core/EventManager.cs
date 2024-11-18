using System;
using System.Collections.Generic;
using System.Reflection;
using VContainer;

namespace Core.GameFramework.Event.Core
{
    public class EventManager
    {
        private readonly EventBus _eventBus;
        private readonly Dictionary<object, List<(Type, IEventHandler)>> _subscriptions;

        [Inject]
        public EventManager(EventBus eventBus)
        {
            _eventBus = eventBus;
            _subscriptions = new Dictionary<object, List<(Type, IEventHandler)>>();
        }

        public void Subscribe<T>(object subscriber, Action<T> handler) where T : IGameEvent
        {
            DelegateEventHandler<T> eventHandler = new DelegateEventHandler<T>(handler);
            AddSubscription(subscriber, typeof(T), eventHandler);
            _eventBus.Subscribe<T>(eventHandler);
        }

        public void Unsubscribe(object subscriber)
        {
            if (_subscriptions.TryGetValue(subscriber, out List<(Type, IEventHandler)> subscriberEvents))
            {
                foreach ((Type eventType, IEventHandler handler) in subscriberEvents)
                {
                    MethodInfo method = typeof(EventBus).GetMethod("Unsubscribe");
                    if (method != null)
                    {
                        MethodInfo genericMethod = method.MakeGenericMethod(eventType);
                        genericMethod.Invoke(_eventBus, new object[] {
                            handler
                        });
                    }
                }
                _subscriptions.Remove(subscriber);
            }
        }

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            _eventBus.Publish(gameEvent);
        }

        private void AddSubscription(object subscriber, Type eventType, IEventHandler handler)
        {
            if (!_subscriptions.ContainsKey(subscriber))
            {
                _subscriptions[subscriber] = new List<(Type, IEventHandler)>();
            }
            _subscriptions[subscriber].Add((eventType, handler));
        }

        private class DelegateEventHandler<T> : IEventHandler where T : IGameEvent
        {
            private readonly Action<T> _handler;

            public DelegateEventHandler(Action<T> handler)
            {
                _handler = handler;
            }

            public void HandleEvent(IGameEvent gameEvent)
            {
                if (gameEvent is T typedEvent)
                {
                    _handler(typedEvent);
                }
            }
        }
    }
}