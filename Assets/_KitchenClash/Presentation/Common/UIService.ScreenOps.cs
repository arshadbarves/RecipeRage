using System;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.Common
{
    public partial class UIService
    {
        private BaseUIScreen ResolveScreen(Type screenType)
        {
            if (_screens.TryGetValue(screenType, out BaseUIScreen existing))
                return existing;

            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null) return null;
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return null;

            VContainer.IObjectResolver resolver = _currentScope ?? _container;
            var screen = (BaseUIScreen)resolver.Resolve(screenType);
            if (screen == null) return null;

            screen.Initialize(attribute.Priority, attribute.Category, controller);
            _screens[screenType] = screen;
            return screen;
        }

        private void ShowAndTrack(Type screenType, UIScreenCategory category, bool animate, bool clearExisting)
        {
            BaseUIScreen screen = ResolveScreen(screenType);
            if (screen == null) return;

            if (clearExisting)
            {
                HideVisibleScreensInCategory(category, animate);
                _stackManager.ClearCategory(category);
            }

            if (!screen.IsVisible)
            {
                _stackManager.Push(screenType, category);
                ShowScreenInternal(screen, animate);
            }
        }

        private UIScreenCategory? GetCategory(Type screenType)
        {
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return null;
            return controller.Category;
        }

        private void HideVisibleScreensInCategory(UIScreenCategory category, bool animate)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.Category == category && s.IsVisible))
            {
                HideScreenInternal(screen, animate);
            }
        }

        private void EnsureNotificationHostMounted(INotificationScreen notificationScreen)
        {
            if (notificationScreen is BaseUIScreen screen && !screen.IsVisible)
            {
                ShowScreenInternal(screen, false);
            }
        }

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return;

            controller.Container?.BringToFront();

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeShowAnimation();

            controller.Show(screen.AnimateShow, duration, animate, () =>
            {
                controller.Container?.BringToFront();
                screen.OnAfterShowAnimation();
                OnScreenShown?.Invoke(screenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return;

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeHideAnimation();

            controller.Hide(screen.AnimateHide, duration, animate, () =>
            {
                screen.OnAfterHideAnimation();
                OnScreenHidden?.Invoke(screenType);

                if (_screens.Values.Where(v => v.Category != UIScreenCategory.Toast).All(v => !v.IsVisible))
                {
                    OnAllScreensHidden?.Invoke();
                }

                if (!_stackManager.IsInHistory(screenType))
                {
                    screen.ResetState();
                }
            });
        }

        private bool TryPopTopmost(UIScreenCategory category, bool animate, bool requireHistory = false)
        {
            int stackDepth = _stackManager.GetStackDepth(category);
            if (stackDepth == 0 || (requireHistory && stackDepth <= 1)) return false;

            Type currentType = _stackManager.Pop(category);
            if (currentType != null && _screens.TryGetValue(currentType, out BaseUIScreen currentScreen))
            {
                HideScreenInternal(currentScreen, animate);
            }

            Type previousType = _stackManager.Peek(category);
            if (previousType != null && _screens.TryGetValue(previousType, out BaseUIScreen previousScreen))
            {
                ShowScreenInternal(previousScreen, animate);
            }

            return currentType != null;
        }
    }
}
