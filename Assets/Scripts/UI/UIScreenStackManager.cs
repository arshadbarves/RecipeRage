using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using Core.UI;

namespace UI
{
    /// <summary>
    /// AAA-grade stack-based UI management system
    /// Each category has its own stack, preventing priority conflicts
    /// Enables clean state management and proper back navigation
    /// </summary>
    public class UIScreenStackManager
    {
        // Category-based stacks
        private readonly Dictionary<UIScreenCategory, Stack<UIScreenType>> _categoryStacks = new();

        // Track all visible screens across categories
        private readonly HashSet<UIScreenType> _visibleScreens = new();

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
                _categoryStacks[category] = new Stack<UIScreenType>();
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
        public bool Push(UIScreenType screenType, UIScreenCategory category)
        {
            CategoryConfig config = _categoryConfigs[category];
            Stack<UIScreenType> stack = _categoryStacks[category];

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
                GameLogger.LogWarning($"Screen {screenType} already visible in category {category}");
                return false;
            }
            else
            {
                // Add to stack
                stack.Push(screenType);
            }

            _visibleScreens.Add(screenType);

            GameLogger.Log($"Pushed {screenType} to {category} stack (depth: {stack.Count})");
            return true;
        }

        /// <summary>
        /// Pop the top screen from a category stack
        /// </summary>
        public UIScreenType? Pop(UIScreenCategory category)
        {
            Stack<UIScreenType> stack = _categoryStacks[category];

            if (stack.Count == 0)
                return null;

            UIScreenType screenType = stack.Pop();
            _visibleScreens.Remove(screenType);

            GameLogger.Log($"Popped {screenType} from {category} stack (remaining: {stack.Count})");
            return screenType;
        }

        /// <summary>
        /// Pop a specific screen from its category
        /// </summary>
        public bool PopSpecific(UIScreenType screenType, UIScreenCategory category)
        {
            Stack<UIScreenType> stack = _categoryStacks[category];

            if (stack.Count == 0)
                return false;

            // If it's the top screen, just pop
            if (stack.Peek() == screenType)
            {
                Pop(category);
                return true;
            }

            // Otherwise, we need to rebuild the stack without this screen
            var temp = new List<UIScreenType>();
            bool found = false;

            while (stack.Count > 0)
            {
                UIScreenType current = stack.Pop();
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
                GameLogger.Log($"Removed {screenType} from {category} stack");
            }

            return found;
        }

        /// <summary>
        /// Peek at the top screen in a category without removing it
        /// </summary>
        public UIScreenType? Peek(UIScreenCategory category)
        {
            Stack<UIScreenType> stack = _categoryStacks[category];
            return stack.Count > 0 ? stack.Peek() : null;
        }

        /// <summary>
        /// Clear all screens from a category
        /// </summary>
        public void ClearCategory(UIScreenCategory category)
        {
            Stack<UIScreenType> stack = _categoryStacks[category];

            foreach (UIScreenType screenType in stack)
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
        public IReadOnlyList<UIScreenType> GetVisibleInCategory(UIScreenCategory category)
        {
            return _categoryStacks[category].ToList().AsReadOnly();
        }

        /// <summary>
        /// Get all visible screens across all categories, sorted by priority
        /// </summary>
        public IReadOnlyList<UIScreenType> GetAllVisible()
        {
            return _visibleScreens.ToList().AsReadOnly();
        }

        /// <summary>
        /// Check if a screen is visible
        /// </summary>
        public bool IsVisible(UIScreenType screenType)
        {
            return _visibleScreens.Contains(screenType);
        }

        /// <summary>
        /// Get the category of a screen type
        /// </summary>
        public UIScreenCategory GetCategory(UIScreenType screenType)
        {
            // This should be configured via attributes or a mapping
            // For now, return based on screen type
            return screenType switch
            {
                UIScreenType.Splash => UIScreenCategory.System,
                UIScreenType.Maintenance => UIScreenCategory.System,

                UIScreenType.Loading => UIScreenCategory.Overlay,
                UIScreenType.Login => UIScreenCategory.Overlay,

                UIScreenType.Modal => UIScreenCategory.Modal,

                UIScreenType.Popup => UIScreenCategory.Popup,
                UIScreenType.FriendsPopup => UIScreenCategory.Popup,
                UIScreenType.UsernamePopup => UIScreenCategory.Popup,
                UIScreenType.Notification => UIScreenCategory.Popup,

                UIScreenType.HUD => UIScreenCategory.Persistent,
                UIScreenType.Background => UIScreenCategory.Persistent,

                _ => UIScreenCategory.Screen
            };
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
                Stack<UIScreenType> stack = _categoryStacks[category];
                if (stack.Count > 0)
                {
                    GameLogger.Log($"{category} ({stack.Count}):");
                    foreach (UIScreenType screenType in stack.Reverse())
                    {
                        GameLogger.Log($"  - {screenType}");
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
