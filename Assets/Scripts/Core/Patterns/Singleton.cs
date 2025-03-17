using System;
using UnityEngine;

namespace RecipeRage.Core.Patterns
{
    /// <summary>
    /// Generic singleton implementation that doesn't require MonoBehaviour.
    /// This class provides a thread-safe, lazy-initialized singleton pattern.
    /// </summary>
    /// <typeparam name="T"> The type of the singleton class </typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        // Thread-safe singleton instance with lazy initialization
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

        /// <summary>
        /// Protected constructor to prevent external instantiation.
        /// </summary>
        protected Singleton()
        {
            if (_instance.IsValueCreated)
            {
                Debug.LogWarning($"An instance of {typeof(T).Name} already exists. Use {typeof(T).Name}.Instance instead of creating a new instance.");
            }
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static T Instance => _instance.Value;
    }
}