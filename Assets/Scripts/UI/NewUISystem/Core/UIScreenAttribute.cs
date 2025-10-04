using System;
using UI.UISystem;

namespace UI.UISystem.Core
{
    /// <summary>
    /// Attribute to mark and configure UI screen classes for auto-registration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIScreenAttribute : Attribute
    {
        public UIScreenType ScreenType { get; }
        public UIScreenPriority Priority { get; }
        public string TemplatePath { get; }
        public bool AutoRegister { get; }

        public UIScreenAttribute(UIScreenType screenType, UIScreenPriority priority, string templatePath = null, bool autoRegister = true)
        {
            ScreenType = screenType;
            Priority = priority;
            TemplatePath = templatePath;
            AutoRegister = autoRegister;
        }
    }
}