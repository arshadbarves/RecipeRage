using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using VContainer;
using Playcenter.Services;

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
            IObjectResolver session = _sessionManager?.SessionContainer;
            if (session != null && session.TryResolve(out T fromSession))
            {
                return fromSession;
            }

            // Root-owned services (e.g. IPlayerDataService) remain available when session is inactive.
            if (_rootContainer != null && _rootContainer.TryResolve(out T fromRoot))
            {
                return fromRoot;
            }

            return null;
        }

        public void Inject(object target)
        {
            if (target == null)
            {
                return;
            }

            IObjectResolver session = _sessionManager?.SessionContainer;
            if (session != null)
            {
                session.Inject(target);
                return;
            }

            _rootContainer?.Inject(target);
        }
    }
}
