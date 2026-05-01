using System;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Interface for managing UI screen stacks per category
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
