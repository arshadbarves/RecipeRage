using System;
using Core.Core.Shared.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.Core.UI.Interfaces
{
    public interface IUIService : IInitializable
    {
        bool IsInitialized { get; }
        // Initialize with generic object to avoid UI dependency
        void Initialize(object uiDocument);
        void InitializeScreens();

        void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true);
        void HideScreen(UIScreenType screenType, bool animate = true);
        void HideScreensOfType(UIScreenType screenType, bool animate = true);
        void HideAllPopups(bool animate = true);
        void HideAllModals(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);

        // Generic get screen methods to avoid BaseUIScreen dependency in Core
        T GetScreen<T>() where T : class;
        object GetScreen(UIScreenType screenType);
        T GetScreen<T>(UIScreenType screenType) where T : class;

        bool IsScreenVisible(UIScreenType screenType);

        bool GoBack(bool animate = true);
        void ClearHistory();

        // Removed GetScreenCategory as it depends on UIScreenCategory (which is likely in UI)
        // Removed IsInteractionBlocked

        UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        event Action<UIScreenType> OnScreenShown;
        event Action<UIScreenType> OnScreenHidden;
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
