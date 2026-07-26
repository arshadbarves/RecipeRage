using System.Collections.Generic;

namespace Playcenter.SDK
{
    /// <summary>Factory that produces the standard 9-module boot sequence in spec order.</summary>
    public static class DefaultModulePack
    {
        public static IReadOnlyList<IPlaycenterModule> Create()
        {
            return new IPlaycenterModule[]
            {
                new LoggingModule(),
                new ConnectivityModule(),
                new NtpModule(),
                new RemoteConfigModule(),
                new ForceUpdateModule(),
                new MaintenanceModule(),
                new AuthWarmupModule(),
                new AnalyticsModule(),
                new ShellReadyModule()
            };
        }
    }
}
