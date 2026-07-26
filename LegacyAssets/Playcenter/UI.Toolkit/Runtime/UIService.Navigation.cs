using System;
using System.Linq;
using System.Threading.Tasks;
using Playcenter.UI;

namespace Playcenter.UI.Toolkit
{
    public partial class UIService
    {
        public void SetRootScreen<T>(bool animate = true) where T : class => SetRootScreen(typeof(T), animate);
        public void SetRootScreen(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Screen, animate);
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            ShowAndTrack(screenType, UIScreenCategory.Screen, animate, false);
        }

        public void PushScreen<T>(bool animate = true) where T : class => PushScreen(typeof(T), animate);
        public void PushScreen(Type screenType, bool animate = true)
        {
            Type current = _stackManager.Peek(UIScreenCategory.Screen);
            if (current != null && current != screenType && _screens.TryGetValue(current, out BaseUIScreen currentScreen))
            {
                HideScreenInternal(currentScreen, animate);
            }
            ShowAndTrack(screenType, UIScreenCategory.Screen, animate, false);
        }

        public void ShowSystem<T>(bool animate = true) where T : class => ShowSystem(typeof(T), animate);
        public void ShowSystem(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.System, animate);
            _stackManager.ClearCategory(UIScreenCategory.System);
            ShowAndTrack(screenType, UIScreenCategory.System, animate, false);
        }

        public void HideSystem<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideSystem(Type screenType, bool animate = true) => Hide(screenType, animate);

        public void ShowOverlay<T>(bool animate = true) where T : class => ShowOverlay(typeof(T), animate);
        public void ShowOverlay(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Overlay, animate, false);
        }

        public void HideOverlay<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideOverlay(Type screenType, bool animate = true) => Hide(screenType, animate);

        public void PushModal<T>(bool animate = true) where T : class => PushModal(typeof(T), animate);
        public void PushModal(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Modal, animate, false);
        }

        public void PushPopup<T>(bool animate = true) where T : class => PushPopup(typeof(T), animate);
        public void PushPopup(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Popup, animate, false);
        }

        public void ShowHud<T>(bool animate = true) where T : class => ShowHud(typeof(T), animate);
        public void ShowHud(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.HUD, animate);
            _stackManager.ClearCategory(UIScreenCategory.HUD);
            ShowAndTrack(screenType, UIScreenCategory.HUD, animate, false);
        }

        public void HideHud<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideHud(Type screenType, bool animate = true) => Hide(screenType, animate);

        public bool Back(bool animate = true)
        {
            if (TryPopTopmost(UIScreenCategory.Modal, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Popup, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Overlay, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Screen, animate, true)) return true;
            return false;
        }

        public async Task ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            INotificationScreen notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null) return;
            EnsureNotificationHostMounted(notificationScreen);
            await notificationScreen.Show(message, type, duration);
        }

        public async Task ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            INotificationScreen notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null) return;
            EnsureNotificationHostMounted(notificationScreen);
            await notificationScreen.Show(title, message, type, duration);
        }

        public void Show<T>(bool animate = true, bool addToHistory = true) where T : class => Show(typeof(T), animate, addToHistory);
        public void Show(Type screenType, bool animate = true, bool addToHistory = true)
        {
            UIScreenCategory? category = GetCategory(screenType);
            if (category == null) return;

            switch (category.Value)
            {
                case UIScreenCategory.System: ShowSystem(screenType, animate); break;
                case UIScreenCategory.Overlay: ShowOverlay(screenType, animate); break;
                case UIScreenCategory.Modal: PushModal(screenType, animate); break;
                case UIScreenCategory.Popup: PushPopup(screenType, animate); break;
                case UIScreenCategory.Screen:
                    if (addToHistory) PushScreen(screenType, animate);
                    else SetRootScreen(screenType, animate);
                    break;
                case UIScreenCategory.HUD: ShowHud(screenType, animate); break;
                case UIScreenCategory.Toast:
                    if (_screens.TryGetValue(screenType, out BaseUIScreen toastScreen))
                        ShowScreenInternal(toastScreen, false);
                    break;
            }
        }

        public void Hide<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void Hide(Type screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen)) return;
            UIScreenCategory? category = GetCategory(screenType);
            if (category == null) return;
            if (category != UIScreenCategory.Toast) _stackManager.PopSpecific(screenType, category.Value);
            HideScreenInternal(screen, animate);
        }

        public void HideAllPopups(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Popup, animate);
            _stackManager.ClearCategory(UIScreenCategory.Popup);
        }

        public void HideAllModals(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Modal, animate);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
        }

        public void HideAllGameScreens(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Popup, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Modal, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Overlay, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Screen, animate);
            HideVisibleScreensInCategory(UIScreenCategory.HUD, animate);
            _stackManager.ClearCategory(UIScreenCategory.Popup);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
            _stackManager.ClearCategory(UIScreenCategory.Overlay);
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            _stackManager.ClearCategory(UIScreenCategory.HUD);
        }

        public void HideAllScreens(bool animate = false)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.IsVisible))
            {
                HideScreenInternal(screen, animate);
            }
            _stackManager.ClearAll();
        }

        public T GetScreen<T>() where T : class
        {
            foreach (Type screenType in _controllers.Keys)
            {
                if (typeof(T).IsAssignableFrom(screenType))
                {
                    BaseUIScreen screen = ResolveScreen(screenType);
                    if (screen is T typedScreen) return typedScreen;
                }
            }

            foreach (BaseUIScreen screen in _screens.Values)
            {
                if (screen is T typedScreen) return typedScreen;
            }
            return null;
        }

        public bool IsScreenVisible<T>() where T : class => IsScreenVisible(typeof(T));
        public bool IsScreenVisible(Type screenType)
        {
            return _screens.TryGetValue(screenType, out BaseUIScreen screen) && screen.IsVisible;
        }

        public bool GoBack(bool animate = true) => Back(animate);

        public void ClearHistory()
        {
            _stackManager.ClearAll();
        }

        public async Task ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            await ShowToast(message, type, duration);
        }

        public async Task ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            await ShowToast(title, message, type, duration);
        }
    }
}
