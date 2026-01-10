using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;

namespace Core.Shared.Events
{
    /// <summary>
    /// Centralized event bus for decoupled communication
    /// Thread-safe implementation with proper cleanup
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();


        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                GameLogger.LogWarning("Attempted to subscribe with null handler");
                return;
            }

            lock (_lock)
            {
                Type eventType = typeof(T);

                if (!_subscriptions.ContainsKey(eventType))
                {
                    _subscriptions[eventType] = new List<Delegate>();
                }

                // Prevent duplicate subscriptions
                if (!_subscriptions[eventType].Contains(handler))
                {
                    _subscriptions[eventType].Add(handler);
                }
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            lock (_lock)
            {
                Type eventType = typeof(T);

                if (_subscriptions.TryGetValue(eventType, out List<Delegate> handlers))
                {
                    handlers.Remove(handler);

                    // Clean up empty lists
                    if (handlers.Count == 0)
                    {
                        _subscriptions.Remove(eventType);
                    }
                }
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                GameLogger.LogWarning("Attempted to publish null event data");
                return;
            }

            Type eventType = typeof(T);
            List<Delegate> handlersCopy;

            lock (_lock)
            {
                if (!_subscriptions.TryGetValue(eventType, out List<Delegate> handlers))
                {
                    // No subscribers for this event
                    return;
                }

                // Create a copy to avoid modification during iteration
                handlersCopy = new List<Delegate>(handlers);
            }

            // Invoke handlers outside the lock to prevent deadlocks
            foreach (Delegate handler in handlersCopy)
            {
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"Error invoking handler for {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public void ClearSubscriptions<T>() where T : class
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_subscriptions.Remove(eventType))
                {
                    GameLogger.LogInfo($"Cleared all subscriptions for {eventType.Name}");
                }
            }
        }

        public void ClearAllSubscriptions()
        {
            lock (_lock)
            {
                int count = _subscriptions.Count;
                _subscriptions.Clear();
                GameLogger.LogInfo($"Cleared all subscriptions ({count} event types)");
            }
        }

        public int GetSubscriptionCount<T>() where T : class
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                return _subscriptions.TryGetValue(eventType, out List<Delegate> handlers) ? handlers.Count : 0;
            }
        }

        public int GetTotalSubscriptionCount()
        {
            lock (_lock)
            {
                return _subscriptions.Values.Sum(list => list.Count);
            }
        }
    }
}
