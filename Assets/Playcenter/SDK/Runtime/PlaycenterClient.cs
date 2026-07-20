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

        /// <summary>
        /// The built service container. Throws <see cref="InvalidOperationException"/> if accessed before <see cref="RunAsync"/> completes service construction.
        /// </summary>
        public IPlaycenterServices Services
        {
            get
            {
                if (_services == null)
                    throw new InvalidOperationException("Services are not available until RunAsync has built the registry.");
                return _services;
            }
        }
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
            _services = _options.Services.Build();

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

        /// <summary>
        /// Re-runs the full module pipeline after a transient failure (e.g. connectivity restored).
        /// Requires <see cref="RunAsync"/> to have been called at least once so that the service
        /// registry is already built. Shell gate screens stay visible until modules pass, then
        /// <see cref="IShellUi.HideAll"/> is called and <see cref="IGameEntry.OnPlaycenterReadyAsync"/>
        /// fires again.
        /// </summary>
        /// <remarks>
        /// Services are intentionally reused from the first run (built once, reused forever).
        /// Modules are re-executed in full so connectivity/NTP/RC modules get a second chance.
        /// </remarks>
        public async Task RetryBootAsync(CancellationToken ct)
        {
            if (_services == null)
                throw new InvalidOperationException("RetryBootAsync called before RunAsync has completed service construction.");

            var moduleWeights = new List<(string id, float weight)>();
            foreach (var m in _options.Modules)
                moduleWeights.Add((m.Id, m.Weight));
            var progress = new BootProgress(moduleWeights);

            _options.Shell.Show(ShellScreenId.Loading);
            progress.Changed += (overall01, status) => _options.Shell.SetProgress(overall01, status);

            var context = new ModuleContext(_services, progress);
            var host = new ModuleHost();
            var failure = await host.RunAsync(_options.Modules, context, ct);

            if (failure == null)
            {
                _options.Shell.HideAll();
                await _options.GameEntry.OnPlaycenterReadyAsync(this, ct);
            }
            else
            {
                _options.Shell.Show(MapFailureToScreen(failure.Code));
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
