using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Attribute to mark and configure UI screen classes for auto-registration
    /// Uses category-based placement - categories auto-determine priority
    /// TYPE-BASED: No longer requires UIScreenType enum
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIScreenAttribute : Attribute
    {
        public UIScreenCategory Category { get; }
        public UIScreenPriority Priority { get; }
        public string TemplatePath { get; }
        public bool AutoRegister { get; }

        public UIScreenAttribute(
            UIScreenCategory category,
            string templatePath = null,
            bool autoRegister = true)
        {
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
                UIScreenCategory.HUD => UIScreenPriority.HUD,
                UIScreenCategory.Toast => UIScreenPriority.Notification,
                _ => UIScreenPriority.Menu
            };
        }
    }
}
