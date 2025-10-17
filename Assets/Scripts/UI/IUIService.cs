using System;
using System.Collections.Generic;
using UI.UISystem.Core;

namespace UI.UISystem
{
    /// <summary>
    /// UI service interface - manages all UI screens
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface IUIService
    {
        // Initialization
        bool IsInitialized { get; }
        
        // Screen management
        void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true);
        void HideScreen(UIScreenType screenType, bool animate = true);
        void HideScreensOfType(UIScreenType screenType, bool animate = true);
        void HideAllPopups(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);
        
        // Screen queries
        T GetScreen<T>() where T : BaseUIScreen;
        BaseUIScreen GetScreen(UIScreenType screenType);
        bool IsScreenVisible(UIScreenType screenType);
        IReadOnlyList<BaseUIScreen> GetVisibleScreens();
        IReadOnlyList<BaseUIScreen> GetScreensByPriority();
        
        // Navigation
        bool GoBack(bool animate = true);
        void ClearHistory();
        
        // Events
        event Action<UIScreenType> OnScreenShown;
        event Action<UIScreenType> OnScreenHidden;
        event Action OnAllScreensHidden;
        
        // Update
        void Update(float deltaTime);
    }
}
