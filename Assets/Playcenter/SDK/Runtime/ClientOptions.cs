using System;
using System.Collections.Generic;

namespace Playcenter.SDK
{
    public sealed class ClientOptions
    {
        private readonly List<IPlaycenterModule> _modules = new List<IPlaycenterModule>();

        public IServiceRegistry Services { get; } = new ServiceRegistry();

        internal IReadOnlyList<IPlaycenterModule> Modules => _modules;
        internal IGameEntry GameEntry { get; private set; }
        internal IShellUi Shell { get; private set; } = new NullShellUi();
        internal IShellTheme Theme { get; private set; }

        public void AddModule(IPlaycenterModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            _modules.Add(module);
        }

        public void SetGameEntry(IGameEntry entry)
        {
            GameEntry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        public void UseShell(IShellUi shell)
        {
            Shell = shell ?? throw new ArgumentNullException(nameof(shell));
        }

        public void UseTheme(IShellTheme theme)
        {
            Theme = theme;
        }

        /// <summary>Registers the standard nine-module boot pack in spec order.</summary>
        public void UseDefaultModules()
        {
            foreach (var module in DefaultModulePack.Create())
                AddModule(module);
        }
    }
}
