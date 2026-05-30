using KitchenClash.Application;
using VContainer;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Persistence;

namespace KitchenClash.Infrastructure.DI
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
        public IGameStarter GameStarter => Resolve<IGameStarter>();
        public EconomyService EconomyService => Resolve<EconomyService>();
        public PlayerDataService PlayerDataService => Resolve<PlayerDataService>();
        public IFriendsService FriendsService => Resolve<IFriendsService>();
        public ILobbyManager LobbyManager => Resolve<ILobbyManager>();
        public IMatchmakingService MatchmakingService => Resolve<IMatchmakingService>();

        public T Resolve<T>() where T : class
        {
            return _sessionManager?.SessionContainer?.Resolve<T>();
        }
    }
}
