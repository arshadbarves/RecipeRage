using System;
using System.Collections.Generic;

namespace Playcenter.UI
{
    /// <summary>
    /// Screen stack. One screen visible at a time (modals layer later if needed).
    /// Screens are scene-placed UIDocument MonoBehaviours registered at boot.
    /// </summary>
    public sealed class UIService : IUIService
    {
        private readonly Dictionary<Type, BaseUIScreen> _screens = new Dictionary<Type, BaseUIScreen>(16);

        public event Action<BaseUIScreen> OnScreenShown;
        public BaseUIScreen Current { get; private set; }

        public void Register(BaseUIScreen screen)
        {
            _screens[screen.GetType()] = screen;
            screen.gameObject.SetActive(false);
        }

        public void Show<T>() where T : BaseUIScreen
        {
            if (Current != null)
            {
                Current.Hide();
            }

            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                Current = screen;
                screen.Show();
                OnScreenShown?.Invoke(screen);
            }
        }

        public void Hide<T>() where T : BaseUIScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                screen.Hide();
                if (Current == screen)
                {
                    Current = null;
                }
            }
        }

        public void HideAll()
        {
            foreach (var screen in _screens.Values)
            {
                screen.Hide();
            }
            Current = null;
        }
    }
}
