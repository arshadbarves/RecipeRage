using VContainer.Unity;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Logging
{
    /// <summary>
    /// Wires the static <see cref="GameLogger"/> facade to the DI-owned
    /// <see cref="ILoggingService"/> so product code reaches UnityEngine.Debug
    /// and the in-game debug console (OnLogAdded).
    /// </summary>
    public sealed class LoggingBootstrap : IInitializable
    {
        private readonly ILoggingService _logging;

        public LoggingBootstrap(ILoggingService logging)
        {
            _logging = logging;
        }

        public void Initialize()
        {
            GameLogger.SetService(_logging);
            _logging.LogInfo("LoggingBootstrap: GameLogger wired to ILoggingService", "Logging");
        }
    }
}
