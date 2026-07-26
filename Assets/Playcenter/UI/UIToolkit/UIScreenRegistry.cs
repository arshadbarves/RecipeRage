using UnityEngine;

namespace Playcenter.UI
{
    /// <summary>
    /// Scene-placed registrar: drag every screen GameObject in; registers all
    /// with UIService at boot (no reflection at runtime on mobile).
    /// </summary>
    public sealed class UIScreenRegistry : MonoBehaviour
    {
        [SerializeField] private BaseUIScreen[] _screens;

        private void Start()
        {
            var ui = ServiceLocator.Get<IUIService>();
            foreach (var screen in _screens)
            {
                ui.Register(screen);
            }
        }
    }
}
