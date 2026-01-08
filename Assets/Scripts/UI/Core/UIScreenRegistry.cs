using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Logging;
using Core.UI;

namespace UI.Core
{
    /// <summary>
    /// Registry system for managing UI screen classes
    /// Handles auto-registration and screen creation
    /// </summary>
    public static class UIScreenRegistry
    {
        private static readonly Dictionary<UIScreenType, Type> _screenTypes = new();
        private static readonly Dictionary<UIScreenType, UIScreenAttribute> _screenAttributes = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the registry by scanning for screen classes
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            _screenTypes.Clear();
            _screenAttributes.Clear();

            // Scan all assemblies for screen classes
            ScanForScreenClasses();

            _isInitialized = true;
            GameLogger.Log($"Initialized with {_screenTypes.Count} screen types");
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
                            RegisterScreenType(attribute.ScreenType, type, attribute);
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
        public static void RegisterScreenType(UIScreenType screenType, Type screenClass, UIScreenAttribute attribute = null)
        {
            if (!screenClass.IsSubclassOf(typeof(BaseUIScreen)))
            {
                GameLogger.LogError($"{screenClass.Name} must inherit from BaseUIScreen");
                return;
            }

            _screenTypes[screenType] = screenClass;
            _screenAttributes[screenType] = attribute ?? new UIScreenAttribute(screenType, UIScreenCategory.Screen);

            GameLogger.Log($"Registered {screenType} -> {screenClass.Name}");
        }

        /// <summary>
        /// Create an instance of a screen by type
        /// </summary>
        public static BaseUIScreen CreateScreen(UIScreenType screenType)
        {
            if (!_screenTypes.TryGetValue(screenType, out Type screenClass))
            {
                GameLogger.LogError($"No screen class registered for {screenType}");
                return null;
            }

            try
            {
                var screen = (BaseUIScreen)Activator.CreateInstance(screenClass);
                return screen;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to create screen {screenType}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get screen attribute for a screen type
        /// </summary>
        public static UIScreenAttribute GetScreenAttribute(UIScreenType screenType)
        {
            _screenAttributes.TryGetValue(screenType, out UIScreenAttribute attribute);
            return attribute;
        }

        /// <summary>
        /// Get all registered screen types
        /// </summary>
        public static IEnumerable<UIScreenType> GetRegisteredScreenTypes()
        {
            return _screenTypes.Keys;
        }

        /// <summary>
        /// Check if a screen type is registered
        /// </summary>
        public static bool IsScreenRegistered(UIScreenType screenType)
        {
            return _screenTypes.ContainsKey(screenType);
        }

        /// <summary>
        /// Get the class type for a screen
        /// </summary>
        public static Type GetScreenClassType(UIScreenType screenType)
        {
            _screenTypes.TryGetValue(screenType, out Type type);
            return type;
        }

        /// <summary>
        /// Unregister a screen type
        /// </summary>
        public static void UnregisterScreenType(UIScreenType screenType)
        {
            _screenTypes.Remove(screenType);
            _screenAttributes.Remove(screenType);
        }

        /// <summary>
        /// Clear all registered screens
        /// </summary>
        public static void Clear()
        {
            _screenTypes.Clear();
            _screenAttributes.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Get debug information about registered screens
        /// </summary>
        public static string GetDebugInfo()
        {
            string info = $"UIScreenRegistry - {_screenTypes.Count} registered screens:\n";
            foreach (KeyValuePair<UIScreenType, Type> kvp in _screenTypes)
            {
                UIScreenAttribute attribute = _screenAttributes[kvp.Key];
                info += $"  {kvp.Key} -> {kvp.Value.Name} (Priority: {attribute.Priority})\n";
            }
            return info;
        }
    }
}