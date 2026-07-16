using Playcenter.UI.Toolkit;
using VContainer.Unity;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Thin VContainer entry point that drives <see cref="UIService"/> Start/Tick
    /// without coupling Toolkit to VContainer.Unity interfaces.
    /// </summary>
    public sealed class UIServiceEntryPoint : IStartable, ITickable
    {
        private readonly UIService _ui;

        public UIServiceEntryPoint(UIService ui) => _ui = ui;

        public void Start() => _ui.Start();
        public void Tick() => _ui.Tick();
    }
}
