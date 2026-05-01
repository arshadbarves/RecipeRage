using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Persistence;

namespace KitchenClash.Infrastructure.DI
{
    public interface ISessionContext
    {
        bool IsSessionActive { get; }
        IGameModeService GameModeService { get; }
        ICharacterService CharacterService { get; }
        ISkinsService SkinsService { get; }
        IGameStarter GameStarter { get; }
        EconomyService EconomyService { get; }
        PlayerDataService PlayerDataService { get; }
        IFriendsService FriendsService { get; }
        ILobbyManager LobbyManager { get; }
        IMatchmakingService MatchmakingService { get; }
        T Resolve<T>() where T : class;
    }
}
