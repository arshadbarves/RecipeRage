using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Logging;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Registry system for managing UI screen classes
    /// TYPE-BASED: Uses Type as key instead of UIScreenType enum
    /// </summary>
    public static class UIScreenRegistry
    {
        private static readonly Dictionary<Type, UIScreenAttribute> _screenAttributes = new();
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            _screenAttributes.Clear();
            ScanForScreenClasses();

            _isInitialized = true;
            GameLogger.Log($"UIScreenRegistry initialized with {_screenAttributes.Count} screen types");
        }

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

        public static UIScreenAttribute GetScreenAttribute(Type screenClass)
        {
            _screenAttributes.TryGetValue(screenClass, out UIScreenAttribute attribute);
            return attribute;
        }

        public static UIScreenAttribute GetScreenAttribute<T>() where T : BaseUIScreen
        {
            return GetScreenAttribute(typeof(T));
        }

        public static IEnumerable<Type> GetRegisteredScreenTypes()
        {
            return _screenAttributes.Keys;
        }

        public static bool IsScreenRegistered(Type screenClass)
        {
            return _screenAttributes.ContainsKey(screenClass);
        }

        public static bool IsScreenRegistered<T>() where T : BaseUIScreen
        {
            return IsScreenRegistered(typeof(T));
        }

        public static void UnregisterScreen(Type screenClass)
        {
            _screenAttributes.Remove(screenClass);
        }

        public static void UnregisterScreen<T>() where T : BaseUIScreen
        {
            UnregisterScreen(typeof(T));
        }

        public static void Clear()
        {
            _screenAttributes.Clear();
            _isInitialized = false;
        }

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
