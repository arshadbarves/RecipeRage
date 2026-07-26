using System;
using System.Threading.Tasks;

namespace Playcenter.UI
{
    /// <summary>
    /// Type-based UI stack contract for multi-title Brawl shells.
    /// Engine-free: no Unity, VContainer, or UniTask types.
    /// </summary>
    public interface IUIService
    {
        bool IsInitialized { get; }

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

        Task ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        void Show<T>(bool animate = true, bool addToHistory = true) where T : class;
        void Show(Type screenType, bool animate = true, bool addToHistory = true);
        void Hide<T>(bool animate = true) where T : class;
        void Hide(Type screenType, bool animate = true);

        void HideAllPopups(bool animate = true);
        void HideAllModals(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);

        T GetScreen<T>() where T : class;

        bool IsScreenVisible<T>() where T : class;
        bool IsScreenVisible(Type screenType);

        bool GoBack(bool animate = true);
        void ClearHistory();

        Task ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        Task ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        event Action<Type> OnScreenShown;
        event Action<Type> OnScreenHidden;
        event Action OnAllScreensHidden;

        /// <summary>
        /// Game host passes its DI resolver (e.g. VContainer IObjectResolver).
        /// Playcenter stays engine-free.
        /// </summary>
        void SetCurrentScope(object scope);
        void Update(float deltaTime);
    }
}
