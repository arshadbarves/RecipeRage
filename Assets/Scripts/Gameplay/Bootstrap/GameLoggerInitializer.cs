using VContainer.Unity;
using Core.Logging;

namespace Gameplay.Bootstrap
{
    public class GameLoggerInitializer : IStartable
    {
        private readonly ILoggingService _loggingService;

        public GameLoggerInitializer(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public void Start()
        {
            GameLogger.Initialize(_loggingService);
            GameLogger.Log("GameLogger initialized via VContainer.");
        }
    }
}
