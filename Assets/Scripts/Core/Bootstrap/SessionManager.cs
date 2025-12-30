using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.SaveSystem;
using VContainer;

namespace Core.Bootstrap
{
    public class SessionManager
    {
        public GameSession CurrentSession { get; private set; }
        
        private readonly ISaveService _saveService;
        private readonly IEventBus _eventBus;
        private readonly ILoggingService _loggingService;

        [Inject]
        public SessionManager(ISaveService saveService, IEventBus eventBus, ILoggingService loggingService)
        {
            _saveService = saveService;
            _eventBus = eventBus;
            _loggingService = loggingService;
        }

        public void CreateSession()
        {
            if (CurrentSession != null)
            {
                CurrentSession.Dispose();
            }
            CurrentSession = new GameSession(_saveService, _eventBus, _loggingService);
        }
        
        public void DestroySession()
        {
            CurrentSession?.Dispose();
            CurrentSession = null;
        }
    }
}
