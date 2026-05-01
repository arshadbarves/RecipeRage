using System;

namespace KitchenClash.Domain
{
    public interface IRouterService
    {
        void Push(ScreenId id, object param = null);
        void Pop();
        void PopToRoot();
        void Replace(ScreenId id, object param = null);
        ScreenId Current { get; }
        event Action<ScreenId> OnScreenChanged;
    }
}
