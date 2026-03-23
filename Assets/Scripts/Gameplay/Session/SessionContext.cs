using Core.Networking;
using Core.Networking.Interfaces;
using Gameplay.Characters;
using Gameplay.Economy;
using Gameplay.GameModes;
using Gameplay.Persistence;
using Gameplay.Skins;
using VContainer;

namespace Core.Session
{
    /// <summary>
    /// Centralized access point for session-scoped services.
    /// </summary>
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
        public ILobbyManager LobbyManager => Resolve<ILobbyManager>();
        public IGameStarter GameStarter => Resolve<IGameStarter>();
        public IFriendsService FriendsService => Resolve<INetworkingServices>()?.FriendsService;
        public IMatchmakingService MatchmakingService => Resolve<IMatchmakingService>();
        public IGameModeService GameModeService => Resolve<IGameModeService>();
        public ICharacterService CharacterService => ResolveRoot<ICharacterService>();
        public ISkinsService SkinsService => ResolveRoot<ISkinsService>();
        public EconomyService EconomyService => ResolveRoot<EconomyService>();
        public PlayerDataService PlayerDataService => ResolveRoot<PlayerDataService>();

        public T Resolve<T>() where T : class
        {
            return _sessionManager?.SessionContainer?.Resolve<T>();
        }

        private T ResolveRoot<T>() where T : class
        {
            if (!IsSessionActive)
            {
                return null;
            }

            return _rootContainer?.Resolve<T>();
        }
    }
}
