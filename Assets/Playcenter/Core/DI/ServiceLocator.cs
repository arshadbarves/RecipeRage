using System;
using System.Collections.Generic;

namespace Playcenter
{
    /// <summary>
    /// Static service registry. Registered by composition roots, consumed everywhere.
    /// Not a container — no construction, no lifetimes, just lookup.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>(64);

        public static void Register<T>(T instance) where T : class
        {
            Services[typeof(T)] = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }
            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
