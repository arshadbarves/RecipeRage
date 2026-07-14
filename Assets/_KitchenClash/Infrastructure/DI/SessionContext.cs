using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using VContainer;

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
        public IEconomyService EconomyService => Resolve<IEconomyService>();
        public IPlayerDataService PlayerDataService => Resolve<IPlayerDataService>();
        public IFriendsService FriendsService => Resolve<IFriendsService>();
        public ILobbyManager LobbyManager => Resolve<ILobbyManager>();
        public IMatchmakingService MatchmakingService => Resolve<IMatchmakingService>();

        public T Resolve<T>() where T : class
        {
            return _sessionManager?.SessionContainer?.Resolve<T>();
        }

        public void Inject(object target)
        {
            if (target == null)
            {
                return;
            }

            _sessionManager?.SessionContainer?.Inject(target);
        }
    }
}
