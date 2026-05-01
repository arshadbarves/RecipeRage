namespace KitchenClash.Application.Services
{
    public interface ISessionContext
    {
        bool IsSessionActive { get; }
        IGameModeService GameModeService { get; }
        ICharacterService CharacterService { get; }
        ISkinsService SkinsService { get; }
        EconomyService EconomyService { get; }
        PlayerDataServiceAdapter PlayerDataService { get; }
        IGameStarter GameStarter { get; }
        T Resolve<T>() where T : class;
    }
}
