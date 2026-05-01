using System;
using System.Collections.Generic;

namespace KitchenClash.Application.Services
{
    public interface IUIScreenStackManager
    {
        bool Push(Type screenType, Presentation.Common.UIScreenCategory category);
        Type Pop(Presentation.Common.UIScreenCategory category);
        void ClearAll();
        IReadOnlyList<Type> GetAllVisible();
        bool IsVisible(Type screenType);
    }
}
