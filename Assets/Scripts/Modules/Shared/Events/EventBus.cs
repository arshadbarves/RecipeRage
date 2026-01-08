using System;
using System.Collections.Generic;
using System.Linq;
using Core.Bootstrap;
using Modules.Logging;
using UnityEngine;
using VContainer;

namespace Modules.Shared.Events
{
    /// <summary>
    /// Centralized event bus for decoupled communication
    /// Thread-safe implementation with proper cleanup
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();
        private readonly ILoggingService _logger;

        [Inject]
        public EventBus(ILoggingService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called after all services are constructed.
        /// </summary>
        public void Initialize()
        {
            // EventBus doesn't need cross-service setup
        }

        /// <summary>
        /// Subscribe to an event type
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null)
            {
                _logger.LogWarning("Attempted to subscribe with null handler", "EventBus");
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
                    // Reduced verbosity for standard subscriptions to avoid spam
                    // _logger.LogInfo($"Subscribed to {eventType.Name} (Total: {_subscriptions[eventType].Count})", "EventBus");
                }
            }
        }

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
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

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                _logger.LogWarning("Attempted to publish null event data", "EventBus");
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
                    _logger.LogError($"Error invoking handler for {eventType.Name}: {ex.Message}\n{ex.StackTrace}", "EventBus");
                }
            }

            // Optional: Log publication if needed for debugging
            // _logger.LogInfo($"Published {eventType.Name} to {handlersCopy.Count} subscribers", "EventBus");
        }

        /// <summary>
        /// Clear all subscriptions for a specific event type
        /// </summary>
        public void ClearSubscriptions<T>() where T : class
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_subscriptions.Remove(eventType))
                {
                    _logger.LogInfo($"Cleared all subscriptions for {eventType.Name}", "EventBus");
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions
        /// </summary>
        public void ClearAllSubscriptions()
        {
            lock (_lock)
            {
                int count = _subscriptions.Count;
                _subscriptions.Clear();
                _logger.LogInfo($"Cleared all subscriptions ({count} event types)", "EventBus");
            }
        }

        /// <summary>
        /// Get subscription count for debugging
        /// </summary>
        public int GetSubscriptionCount<T>() where T : class
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                return _subscriptions.TryGetValue(eventType, out List<Delegate> handlers) ? handlers.Count : 0;
            }
        }

        /// <summary>
        /// Get total subscription count across all event types
        /// </summary>
        public int GetTotalSubscriptionCount()
        {
            lock (_lock)
            {
                return _subscriptions.Values.Sum(list => list.Count);
            }
        }
    }
}
