using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public sealed class PlaycenterClient
    {
        private readonly ClientOptions _options;
        private IPlaycenterServices _services;

        private PlaycenterClient(ClientOptions options)
        {
            _options = options;
        }

        public IPlaycenterServices Services => _services;
        public IShellUi Shell => _options.Shell;

        public static PlaycenterClient Create(Action<ClientOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var options = new ClientOptions();
            configure(options);
            return new PlaycenterClient(options);
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (_options.GameEntry == null)
                throw new InvalidOperationException("No IGameEntry configured. Call options.SetGameEntry() before RunAsync.");

            // 1. Build service registry
            _services = ((ServiceRegistry)_options.Services).Build();

            // 2. Build BootProgress from module weights
            var moduleWeights = new List<(string id, float weight)>();
            foreach (var m in _options.Modules)
                moduleWeights.Add((m.Id, m.Weight));
            var progress = new BootProgress(moduleWeights);

            // 3. Apply theme if set
            if (_options.Theme != null)
                _options.Shell.SetTheme(_options.Theme);

            // 4. Show splash then loading
            _options.Shell.Show(ShellScreenId.Splash);
            _options.Shell.Show(ShellScreenId.Loading);

            // 5. Subscribe progress → shell
            progress.Changed += (overall01, status) => _options.Shell.SetProgress(overall01, status);

            // 6. Run modules
            var context = new ModuleContext(_services, progress);
            var host = new ModuleHost();
            var failure = await host.RunAsync(_options.Modules, context, ct);

            // 7. Dispatch to game entry
            if (failure == null)
            {
                _options.Shell.HideAll();
                await _options.GameEntry.OnPlaycenterReadyAsync(this, ct);
            }
            else
            {
                var gateScreen = MapFailureToScreen(failure.Code);
                _options.Shell.Show(gateScreen);
                await _options.GameEntry.OnPlaycenterFailedAsync(failure, ct);
            }
        }

        private static ShellScreenId MapFailureToScreen(BootFailureCode code)
        {
            switch (code)
            {
                case BootFailureCode.Offline:      return ShellScreenId.NoConnection;
                case BootFailureCode.ForceUpdate:   return ShellScreenId.ForceUpdate;
                case BootFailureCode.Maintenance:   return ShellScreenId.Maintenance;
                default:                            return ShellScreenId.NoConnection;
            }
        }
    }
}
