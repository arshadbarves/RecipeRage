using System;
using System.Collections.Generic;
using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using UI.Core;
using UI.Screens;
using UnityEngine.UIElements;

namespace UI
{
    public interface IUIService : IInitializable
    {
        bool IsInitialized { get; }
        void Initialize(UIDocument uiDocument);
        void InitializeScreens();

        void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true);
        void HideScreen(UIScreenType screenType, bool animate = true);
        void HideScreensOfType(UIScreenType screenType, bool animate = true);
        void HideAllPopups(bool animate = true);
        void HideAllModals(bool animate = true);
        void HideAllGameScreens(bool animate = true);
        void HideAllScreens(bool animate = false);

        T GetScreen<T>() where T : BaseUIScreen;
        BaseUIScreen GetScreen(UIScreenType screenType);
        T GetScreen<T>(UIScreenType screenType) where T : BaseUIScreen;
        bool IsScreenVisible(UIScreenType screenType);
        IReadOnlyList<BaseUIScreen> GetVisibleScreens();
        IReadOnlyList<BaseUIScreen> GetScreensByPriority();

        bool GoBack(bool animate = true);
        void ClearHistory();

        UIScreenCategory GetScreenCategory(UIScreenType screenType);
        bool IsInteractionBlocked(UIScreenType screenType);

        UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f);
        UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f);

        event Action<UIScreenType> OnScreenShown;
        event Action<UIScreenType> OnScreenHidden;
        event Action OnAllScreensHidden;

        void Update(float deltaTime);
    }
}
