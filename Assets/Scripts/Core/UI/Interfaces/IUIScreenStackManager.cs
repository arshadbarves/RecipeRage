using System;
using System.Collections.Generic;

namespace Core.UI.Interfaces
{
    /// <summary>
    /// Interface for UI Screen Stack Manager
    /// TYPE-BASED: Uses Type instead of UIScreenType enum
    /// </summary>
    public interface IUIScreenStackManager
    {
        bool Push(Type screenType, UIScreenCategory category);
        Type Pop(UIScreenCategory category);
        bool PopSpecific(Type screenType, UIScreenCategory category);
        Type Peek(UIScreenCategory category);
        void ClearCategory(UIScreenCategory category);
        void ClearAll();
        IReadOnlyList<Type> GetVisibleInCategory(UIScreenCategory category);
        IReadOnlyList<Type> GetAllVisible();
        bool IsVisible(Type screenType);
        int GetStackDepth(UIScreenCategory category);
        bool IsBlockedByHigherCategory(UIScreenCategory category);
        bool IsInHistory(Type screenType);
        void DebugPrintState();
    }
}
