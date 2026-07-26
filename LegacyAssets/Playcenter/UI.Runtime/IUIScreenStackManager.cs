using System;

namespace Playcenter.UI
{
    /// <summary>
    /// Engine-free per-category screen stack history.
    /// </summary>
    public interface IUIScreenStackManager
    {
        void Push(Type screenType, UIScreenCategory category);
        Type Pop(UIScreenCategory category);
        Type Peek(UIScreenCategory category);
        void PopSpecific(Type screenType, UIScreenCategory category);
        void ClearCategory(UIScreenCategory category);
        void ClearAll();
        bool IsInHistory(Type screenType);
        int GetStackDepth(UIScreenCategory category);
    }
}
