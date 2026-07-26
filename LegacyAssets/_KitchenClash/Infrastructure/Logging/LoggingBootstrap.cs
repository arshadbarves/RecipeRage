using VContainer.Unity;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Logging
{
    /// <summary>
    /// Confirms the static <see cref="GameLogger"/> facade is wired to the
    /// DI-owned <see cref="ILoggingService"/>. Root also wires via
    /// <c>RegisterBuildCallback</c> so the facade is ready before any
    /// <see cref="IInitializable"/> runs; this entry point re-applies the
    /// wire (idempotent) and emits a visible bootstrap line.
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
