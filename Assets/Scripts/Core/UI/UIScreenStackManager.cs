using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using Core.UI.Interfaces;

namespace Core.UI
{
    /// <summary>
    /// AAA-grade stack-based UI management system
    /// TYPE-BASED: Uses Type instead of UIScreenType enum
    /// Each category has its own stack, preventing priority conflicts
    /// </summary>
    public class UIScreenStackManager : IUIScreenStackManager
    {
        // Category-based stacks (Type-based)
        private readonly Dictionary<UIScreenCategory, Stack<Type>> _categoryStacks = new();

        // Track all visible screens across categories
        private readonly HashSet<Type> _visibleScreens = new();

        // Category configuration
        private readonly Dictionary<UIScreenCategory, CategoryConfig> _categoryConfigs = new();

        public UIScreenStackManager()
        {
            InitializeCategoryStacks();
            ConfigureCategories();
        }

        private void InitializeCategoryStacks()
        {
            foreach (UIScreenCategory category in Enum.GetValues(typeof(UIScreenCategory)))
            {
                _categoryStacks[category] = new Stack<Type>();
            }
        }

        private void ConfigureCategories()
        {
            // System: Only one at a time, no stacking
            _categoryConfigs[UIScreenCategory.System] = new CategoryConfig
            {
                AllowStacking = false,
                AllowMultiple = false,
                TrackHistory = false,
                BlocksLowerCategories = true
            };

            // Overlay: Can stack, blocks lower categories
            _categoryConfigs[UIScreenCategory.Overlay] = new CategoryConfig
            {
                AllowStacking = true,
                AllowMultiple = true,
                TrackHistory = true,
                BlocksLowerCategories = true
            };

            // Modal: Can stack, blocks interaction
            _categoryConfigs[UIScreenCategory.Modal] = new CategoryConfig
            {
                AllowStacking = true,
                AllowMultiple = true,
                TrackHistory = true,
                BlocksLowerCategories = true
            };

            // Popup: Can stack, doesn't block
            _categoryConfigs[UIScreenCategory.Popup] = new CategoryConfig
            {
                AllowStacking = true,
                AllowMultiple = true,
                TrackHistory = true,
                BlocksLowerCategories = false
            };

            // Screen: Only one at a time, full history
            _categoryConfigs[UIScreenCategory.Screen] = new CategoryConfig
            {
                AllowStacking = false,
                AllowMultiple = false,
                TrackHistory = true,
                BlocksLowerCategories = false
            };

            // Persistent: Multiple allowed, no history
            _categoryConfigs[UIScreenCategory.Persistent] = new CategoryConfig
            {
                AllowStacking = false,
                AllowMultiple = true,
                TrackHistory = false,
                BlocksLowerCategories = false
            };
        }

        /// <summary>
        /// Push a screen onto its category stack
        /// </summary>
        public bool Push(Type screenType, UIScreenCategory category)
        {
            CategoryConfig config = _categoryConfigs[category];
            Stack<Type> stack = _categoryStacks[category];

            // Check if we can add to this category
            if (!config.AllowMultiple && stack.Count > 0)
            {
                // Replace the current screen
                if (config.TrackHistory)
                {
                    // Keep in stack for history
                    stack.Push(screenType);
                }
                else
                {
                    // Replace without history
                    stack.Clear();
                    stack.Push(screenType);
                }
            }
            else if (!config.AllowStacking && _visibleScreens.Contains(screenType))
            {
                // Screen already visible, don't add again
                GameLogger.LogWarning($"Screen {screenType.Name} already visible in category {category}");
                return false;
            }
            else
            {
                // Add to stack
                stack.Push(screenType);
            }

            _visibleScreens.Add(screenType);

            GameLogger.Log($"Pushed {screenType.Name} to {category} stack (depth: {stack.Count})");
            return true;
        }

        /// <summary>
        /// Pop the top screen from a category stack
        /// </summary>
        public Type Pop(UIScreenCategory category)
        {
            Stack<Type> stack = _categoryStacks[category];

            if (stack.Count == 0)
                return null;

            Type screenType = stack.Pop();
            _visibleScreens.Remove(screenType);

            GameLogger.Log($"Popped {screenType.Name} from {category} stack (remaining: {stack.Count})");
            return screenType;
        }

        /// <summary>
        /// Pop a specific screen from its category
        /// </summary>
        public bool PopSpecific(Type screenType, UIScreenCategory category)
        {
            Stack<Type> stack = _categoryStacks[category];

            if (stack.Count == 0)
                return false;

            // If it's the top screen, just pop
            if (stack.Peek() == screenType)
            {
                Pop(category);
                return true;
            }

            // Otherwise, we need to rebuild the stack without this screen
            var temp = new List<Type>();
            bool found = false;

            while (stack.Count > 0)
            {
                Type current = stack.Pop();
                if (current == screenType)
                {
                    found = true;
                    break;
                }
                temp.Add(current);
            }

            // Rebuild stack
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                stack.Push(temp[i]);
            }

            if (found)
            {
                _visibleScreens.Remove(screenType);
                GameLogger.Log($"Removed {screenType.Name} from {category} stack");
            }

            return found;
        }

        /// <summary>
        /// Peek at the top screen in a category without removing it
        /// </summary>
        public Type Peek(UIScreenCategory category)
        {
            Stack<Type> stack = _categoryStacks[category];
            return stack.Count > 0 ? stack.Peek() : null;
        }

        /// <summary>
        /// Clear all screens from a category
        /// </summary>
        public void ClearCategory(UIScreenCategory category)
        {
            Stack<Type> stack = _categoryStacks[category];

            foreach (Type screenType in stack)
            {
                _visibleScreens.Remove(screenType);
            }

            stack.Clear();
            GameLogger.Log($"Cleared {category} stack");
        }

        /// <summary>
        /// Clear all screens from all categories
        /// </summary>
        public void ClearAll()
        {
            foreach (UIScreenCategory category in _categoryStacks.Keys)
            {
                ClearCategory(category);
            }

            _visibleScreens.Clear();
            GameLogger.Log("Cleared all category stacks");
        }

        /// <summary>
        /// Get all visible screens in a category
        /// </summary>
        public IReadOnlyList<Type> GetVisibleInCategory(UIScreenCategory category)
        {
            return _categoryStacks[category].ToList().AsReadOnly();
        }

        /// <summary>
        /// Get all visible screens across all categories
        /// </summary>
        public IReadOnlyList<Type> GetAllVisible()
        {
            return _visibleScreens.ToList().AsReadOnly();
        }

        /// <summary>
        /// Check if a screen is visible
        /// </summary>
        public bool IsVisible(Type screenType)
        {
            return _visibleScreens.Contains(screenType);
        }

        /// <summary>
        /// Get stack depth for a category
        /// </summary>
        public int GetStackDepth(UIScreenCategory category)
        {
            return _categoryStacks[category].Count;
        }

        /// <summary>
        /// Check if any higher priority category is blocking
        /// </summary>
        public bool IsBlockedByHigherCategory(UIScreenCategory category)
        {
            // Check all higher priority categories
            for (int i = 0; i < (int)category; i++)
            {
                UIScreenCategory higherCategory = (UIScreenCategory)i;
                CategoryConfig config = _categoryConfigs[higherCategory];

                if (config.BlocksLowerCategories && _categoryStacks[higherCategory].Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Debug: Print current stack state
        /// </summary>
        public void DebugPrintState()
        {
            GameLogger.Log("=== UI Stack State ===");

            foreach (UIScreenCategory category in Enum.GetValues(typeof(UIScreenCategory)))
            {
                Stack<Type> stack = _categoryStacks[category];
                if (stack.Count > 0)
                {
                    GameLogger.Log($"{category} ({stack.Count}):");
                    foreach (Type screenType in stack.Reverse())
                    {
                        GameLogger.Log($"  - {screenType.Name}");
                    }
                }
            }

            GameLogger.Log($"Total visible screens: {_visibleScreens.Count}");
        }

        private class CategoryConfig
        {
            public bool AllowStacking { get; set; }
            public bool AllowMultiple { get; set; }
            public bool TrackHistory { get; set; }
            public bool BlocksLowerCategories { get; set; }
        }
    }
}
