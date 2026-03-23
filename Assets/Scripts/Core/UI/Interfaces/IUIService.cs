using System;
using Cysharp.Threading.Tasks;

namespace Core.UI.Interfaces
{
    /// <summary>
    /// UI Service Interface - TYPE-BASED (uses class Type, not enum)
    /// </summary>
    public interface IUIService
    {
        bool IsInitialized { get; }

        // Explicit router API
        void SetRootScreen<T>(bool animate = true) where T : class;
        void SetRootScreen(Type screenType, bool animate = true);
        void PushScreen<T>(bool animate = true) where T : class;
        void PushScreen(Type screenType, bool animate = true);
        void ShowSystem<T>(bool animate = true) where T : class;
        void ShowSystem(Type screenType, bool animate = true);
        void HideSystem<T>(bool animate = true) where T : class;
        void HideSystem(Type screenType, bool animate = true);
        void ShowOverlay<T>(bool animate = true) where T : class;
        void ShowOverlay(Type screenType, bool animate = true);
        void HideOverlay<T>(bool animate = true) where T : class;
        void HideOverlay(Type screenType, bool animate = true);
        void PushModal<T>(bool animate = true) where T : class;
        void PushModal(Type screenType, bool animate = true);
        void PushPopup<T>(bool animate = true) where T : class;
        void PushPopup(Type screenType, bool animate = true);
        void ShowHud<T>(bool animate = true) where T : class;
        void ShowHud(Type screenType, bool animate = true);
        void HideHud<T>(bool animate = true) where T : class;
        void HideHud(Type screenType, bool animate = true);
        bool Back(bool animate = true);
        UniTask ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        UniTask ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        // Type-based Show/Hide
        void Show<T>(bool animate = true, bool addToHistory = true) where T : class;
        void Show(Type screenType, bool animate = true, bool addToHistory = true);
        void Hide<T>(bool animate = true) where T : class;
        void Hide(Type screenType, bool animate = true);

        void HideAllPopups(bool animate = true);
        void HideAllModals(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);

        // Type-based get screen
        T GetScreen<T>() where T : class;

        bool IsScreenVisible<T>() where T : class;
        bool IsScreenVisible(Type screenType);

        bool GoBack(bool animate = true);
        void ClearHistory();

        UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        event Action<Type> OnScreenShown;
        event Action<Type> OnScreenHidden;
        event Action OnAllScreensHidden;

        void Update(float deltaTime);
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
