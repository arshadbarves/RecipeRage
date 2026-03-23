using Core.Networking;
using Core.Networking.Interfaces;
using Gameplay.Characters;
using Gameplay.Economy;
using Gameplay.GameModes;
using Gameplay.Persistence;
using Gameplay.Skins;

namespace Core.Session
{
    /// <summary>
    /// Typed gateway for services that are only valid after a session has been created.
    /// </summary>
    public interface ISessionContext
    {
        bool IsSessionActive { get; }
        ILobbyManager LobbyManager { get; }
        IGameStarter GameStarter { get; }
        IFriendsService FriendsService { get; }
        IMatchmakingService MatchmakingService { get; }
        IGameModeService GameModeService { get; }
        ICharacterService CharacterService { get; }
        ISkinsService SkinsService { get; }
        EconomyService EconomyService { get; }
        PlayerDataService PlayerDataService { get; }

        T Resolve<T>() where T : class;
    }
}
