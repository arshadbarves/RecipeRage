using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            Type type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<Delegate>();
            }

            _handlers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out List<Delegate> list))
            {
                list.Remove(handler);
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            Type type = typeof(T);
            if (!_handlers.TryGetValue(type, out List<Delegate> list))
            {
                return;
            }

            foreach (Delegate handler in list.ToArray())
            {
                ((Action<T>)handler)(eventData);
            }
        }

        public void ClearAllSubscriptions() => _handlers.Clear();
    }
}
