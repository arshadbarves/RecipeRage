using System;
using Core.Core.Shared.Interfaces;

namespace Core.Core.Shared.Events
{
    /// <summary>
    /// Event bus interface for decoupled communication between services
    /// Follows Observer Pattern and Dependency Inversion Principle
    /// </summary>
    public interface IEventBus : IInitializable
    {
        /// <summary>
        /// Subscribe to an event type
        /// </summary>
        void Subscribe<T>(Action<T> handler) where T : class;

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : class;

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        void Publish<T>(T eventData) where T : class;

        /// <summary>
        /// Clear all subscriptions for a specific event type
        /// </summary>
        void ClearSubscriptions<T>() where T : class;

        /// <summary>
        /// Clear all subscriptions
        /// </summary>
        void ClearAllSubscriptions();
    }
}
