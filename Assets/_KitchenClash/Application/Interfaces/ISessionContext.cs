using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Application;

namespace KitchenClash.Application
{
    /// <summary>
    /// Application-facing facade over session-scoped services.
    /// Presentation and flow handlers depend on this contract only — never on Infrastructure DI types.
    /// </summary>
    public interface ISessionContext
    {
        bool IsSessionActive { get; }
        IGameModeService GameModeService { get; }
        ICharacterService CharacterService { get; }
        ISkinsService SkinsService { get; }
        IGameStarter GameStarter { get; }
        IEconomyService EconomyService { get; }
        IPlayerDataService PlayerDataService { get; }
        IFriendsService FriendsService { get; }
        ILobbyManager LobbyManager { get; }
        IMatchmakingService MatchmakingService { get; }
        T Resolve<T>() where T : class;

        /// <summary>
        /// Injects session-scoped dependencies into a Presentation object.
        /// Prefer this over reaching into Infrastructure SessionManager.
        /// </summary>
        void Inject(object target);
    }
}
