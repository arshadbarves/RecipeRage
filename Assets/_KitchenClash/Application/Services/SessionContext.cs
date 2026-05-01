using VContainer;

namespace KitchenClash.Application.Services
{
    public class SessionContext : ISessionContext
    {
        private readonly SessionManager _sessionManager;
        private readonly IObjectResolver _rootContainer;

        public SessionContext(SessionManager sessionManager, IObjectResolver rootContainer)
        {
            _sessionManager = sessionManager;
            _rootContainer = rootContainer;
        }

        public bool IsSessionActive => _sessionManager?.IsSessionActive == true;
        public IGameModeService GameModeService => Resolve<IGameModeService>();
        public ICharacterService CharacterService => Resolve<ICharacterService>();
        public ISkinsService SkinsService => Resolve<ISkinsService>();
        public EconomyService EconomyService => Resolve<EconomyService>();
        public PlayerDataServiceAdapter PlayerDataService => Resolve<PlayerDataServiceAdapter>();
        public IGameStarter GameStarter => Resolve<IGameStarter>();

        public T Resolve<T>() where T : class
        {
            return _sessionManager?.SessionContainer?.Resolve<T>();
        }
    }
}
