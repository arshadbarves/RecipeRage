using System;
using Core.UI.Interfaces;

namespace Core.UI.Core
{
    /// <summary>
    /// Attribute to mark and configure UI screen classes for auto-registration
    /// Uses category-based system - categories auto-determine priority
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIScreenAttribute : Attribute
    {
        public UIScreenType ScreenType { get; }
        public UIScreenCategory Category { get; }
        public UIScreenPriority Priority { get; }
        public string TemplatePath { get; }
        public bool AutoRegister { get; }

        /// <summary>
        /// Create screen attribute with category
        /// Priority is auto-assigned based on category
        /// </summary>
        /// <param name="screenType">Type of screen</param>
        /// <param name="category">Category (System, Overlay, Modal, Popup, Screen, Persistent)</param>
        /// <param name="templatePath">Path to UXML template (relative to Resources/UI/Templates/)</param>
        /// <param name="autoRegister">Auto-register this screen</param>
        public UIScreenAttribute(
            UIScreenType screenType,
            UIScreenCategory category,
            string templatePath = null,
            bool autoRegister = true)
        {
            ScreenType = screenType;
            Category = category;
            TemplatePath = templatePath;
            AutoRegister = autoRegister;
            Priority = CategoryToPriority(category);
        }

        private static UIScreenPriority CategoryToPriority(UIScreenCategory category)
        {
            return category switch
            {
                UIScreenCategory.System => UIScreenPriority.Splash,
                UIScreenCategory.Overlay => UIScreenPriority.Loading,
                UIScreenCategory.Modal => UIScreenPriority.Modal,
                UIScreenCategory.Popup => UIScreenPriority.Popup,
                UIScreenCategory.Screen => UIScreenPriority.Menu,
                UIScreenCategory.Persistent => UIScreenPriority.HUD,
                _ => UIScreenPriority.Menu
            };
        }
    }
}