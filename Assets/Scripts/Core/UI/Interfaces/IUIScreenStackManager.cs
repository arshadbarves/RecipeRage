using System.Collections.Generic;

namespace Core.UI.Interfaces
{
    public interface IUIScreenStackManager
    {
        bool Push(UIScreenType screenType, UIScreenCategory category);
        UIScreenType? Pop(UIScreenCategory category);
        bool PopSpecific(UIScreenType screenType, UIScreenCategory category);
        UIScreenType? Peek(UIScreenCategory category);
        void ClearCategory(UIScreenCategory category);
        void ClearAll();
        IReadOnlyList<UIScreenType> GetVisibleInCategory(UIScreenCategory category);
        IReadOnlyList<UIScreenType> GetAllVisible();
        bool IsVisible(UIScreenType screenType);
        UIScreenCategory GetCategory(UIScreenType screenType);
        int GetStackDepth(UIScreenCategory category);
        bool IsBlockedByHigherCategory(UIScreenCategory category);
        void DebugPrintState();
    }
}
