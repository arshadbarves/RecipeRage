using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Logging;
using Core.UI.Interfaces;

namespace Core.UI.Core
{
    /// <summary>
    /// Registry system for managing UI screen classes
    /// TYPE-BASED: Uses Type as key instead of UIScreenType enum
    /// </summary>
    public static class UIScreenRegistry
    {
        private static readonly Dictionary<Type, UIScreenAttribute> _screenAttributes = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the registry by scanning for screen classes
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            _screenAttributes.Clear();

            // Scan all assemblies for screen classes
            ScanForScreenClasses();

            _isInitialized = true;
            GameLogger.Log($"UIScreenRegistry initialized with {_screenAttributes.Count} screen types");
        }

        /// <summary>
        /// Scan assemblies for classes with UIScreenAttribute
        /// </summary>
        private static void ScanForScreenClasses()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    IEnumerable<Type> types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseUIScreen)))
                        .Where(t => t.GetCustomAttribute<UIScreenAttribute>() != null);

                    foreach (Type type in types)
                    {
                        UIScreenAttribute attribute = type.GetCustomAttribute<UIScreenAttribute>();
                        if (attribute != null && attribute.AutoRegister)
                        {
                            RegisterScreen(type, attribute);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    GameLogger.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Register a screen type manually
        /// </summary>
        public static void RegisterScreen(Type screenClass, UIScreenAttribute attribute = null)
        {
            if (!screenClass.IsSubclassOf(typeof(BaseUIScreen)))
            {
                GameLogger.LogError($"{screenClass.Name} must inherit from BaseUIScreen");
                return;
            }

            _screenAttributes[screenClass] = attribute ?? new UIScreenAttribute(UIScreenCategory.Screen);

            GameLogger.Log($"Registered screen: {screenClass.Name}");
        }

        /// <summary>
        /// Get screen attribute for a screen class
        /// </summary>
        public static UIScreenAttribute GetScreenAttribute(Type screenClass)
        {
            _screenAttributes.TryGetValue(screenClass, out UIScreenAttribute attribute);
            return attribute;
        }

        /// <summary>
        /// Get screen attribute for a screen class (generic)
        /// </summary>
        public static UIScreenAttribute GetScreenAttribute<T>() where T : BaseUIScreen
        {
            return GetScreenAttribute(typeof(T));
        }

        /// <summary>
        /// Get all registered screen types
        /// </summary>
        public static IEnumerable<Type> GetRegisteredScreenTypes()
        {
            return _screenAttributes.Keys;
        }

        /// <summary>
        /// Check if a screen type is registered
        /// </summary>
        public static bool IsScreenRegistered(Type screenClass)
        {
            return _screenAttributes.ContainsKey(screenClass);
        }

        /// <summary>
        /// Check if a screen type is registered (generic)
        /// </summary>
        public static bool IsScreenRegistered<T>() where T : BaseUIScreen
        {
            return IsScreenRegistered(typeof(T));
        }

        /// <summary>
        /// Unregister a screen type
        /// </summary>
        public static void UnregisterScreen(Type screenClass)
        {
            _screenAttributes.Remove(screenClass);
        }

        /// <summary>
        /// Unregister a screen type (generic)
        /// </summary>
        public static void UnregisterScreen<T>() where T : BaseUIScreen
        {
            UnregisterScreen(typeof(T));
        }

        /// <summary>
        /// Clear all registered screens
        /// </summary>
        public static void Clear()
        {
            _screenAttributes.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Get debug information about registered screens
        /// </summary>
        public static string GetDebugInfo()
        {
            string info = $"UIScreenRegistry - {_screenAttributes.Count} registered screens:\n";
            foreach (KeyValuePair<Type, UIScreenAttribute> kvp in _screenAttributes)
            {
                UIScreenAttribute attribute = kvp.Value;
                info += $"  {kvp.Key.Name} (Category: {attribute.Category}, Priority: {attribute.Priority})\n";
            }
            return info;
        }
    }
}