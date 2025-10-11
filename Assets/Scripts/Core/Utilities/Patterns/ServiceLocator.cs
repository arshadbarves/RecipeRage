using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utilities.Patterns
{
    /// <summary>
    /// Service locator pattern implementation for managing game services.
    /// Provides a central registry for services and their access.
    /// </summary>
    public class ServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static ServiceLocator _instance;

        /// <summary>
        /// Gets the singleton instance of the service locator.
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Registers a service with the service locator.
        /// </summary>
        /// <typeparam name="T">Type of service to register</typeparam>
        /// <param name="service">Service instance to register</param>
        public void Register<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (!_services.TryAdd(type, service))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} already registered. Overwriting.");
                _services[type] = service;
            }
            else
            {
                Debug.Log($"[ServiceLocator] Service of type {type.Name} registered.");
            }
        }

        /// <summary>
        /// Unregisters a service from the service locator.
        /// </summary>
        /// <typeparam name="T">Type of service to unregister</typeparam>
        public void Unregister<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"[ServiceLocator] Service of type {type.Name} unregistered.");
            }
            else
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} not registered, cannot unregister.");
            }
        }

        /// <summary>
        /// Gets a service from the service locator.
        /// </summary>
        /// <typeparam name="T">Type of service to get</typeparam>
        /// <returns>The service instance</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            Debug.LogError($"[ServiceLocator] Service of type {type.Name} not registered.");
            throw new InvalidOperationException($"Service of type {type.Name} not registered.");
        }

        /// <summary>
        /// Tries to get a service from the service locator.
        /// </summary>
        /// <typeparam name="T">Type of service to get</typeparam>
        /// <param name="service">Output parameter for the service instance</param>
        /// <returns>True if the service was found, false otherwise</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object serviceObj))
            {
                service = (T)serviceObj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Checks if a service is registered with the service locator.
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <returns>True if the service is registered, false otherwise</returns>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            Debug.Log("[ServiceLocator] All services unregistered.");
        }
    }
}
