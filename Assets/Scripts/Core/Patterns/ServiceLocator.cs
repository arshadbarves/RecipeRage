using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Core.Patterns
{
    /// <summary>
    /// Service Locator pattern implementation for managing game services.
    /// This provides a centralized registry for services and helps with dependency injection.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        /// <summary>
        /// Dictionary to store registered services
        /// </summary>
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service with the service locator
        /// </summary>
        /// <typeparam name="T">Type of service to register</typeparam>
        /// <param name="service">Service instance</param>
        public void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting previous registration.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"Service of type {type.Name} has been registered.");
            }
        }

        /// <summary>
        /// Unregister a service from the service locator
        /// </summary>
        /// <typeparam name="T">Type of service to unregister</typeparam>
        public void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"Service of type {type.Name} has been unregistered.");
            }
            else
            {
                Debug.LogWarning($"Attempted to unregister service of type {type.Name}, but no such service was registered.");
            }
        }

        /// <summary>
        /// Get a service from the service locator
        /// </summary>
        /// <typeparam name="T">Type of service to retrieve</typeparam>
        /// <returns>Instance of requested service</returns>
        /// <exception cref="InvalidOperationException">Thrown when requested service is not registered</exception>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
        }

        /// <summary>
        /// Try to get a service from the service locator
        /// </summary>
        /// <typeparam name="T">Type of service to retrieve</typeparam>
        /// <param name="service">Output parameter to receive service instance if found</param>
        /// <returns>True if service was found, false otherwise</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object registeredService))
            {
                service = (T)registeredService;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <returns>True if service is registered, false otherwise</returns>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// Clear all registered services
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            Debug.Log("All services have been unregistered.");
        }
    }
} 