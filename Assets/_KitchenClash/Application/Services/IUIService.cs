using System;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface IUIService
    {
        bool IsInitialized { get; }
        void Show<T>(bool animate = true, bool addToHistory = true) where T : class;
        void Hide<T>(bool animate = true) where T : class;
        T GetScreen<T>() where T : class;
        bool IsScreenVisible<T>() where T : class;
        bool GoBack(bool animate = true);
        void ClearHistory();
        void HideAllScreens(bool animate = false);
        UniTask ShowNotification(string message, float duration = 3f);
        event Action<Type> OnScreenShown;
        event Action<Type> OnScreenHidden;
    }
}
