using System;

namespace Core.GameModes
{
    public interface IGameModeService
    {
        GameMode SelectedGameMode { get; }
        GameMode[] GetAvailableGameModes();
        GameMode GetGameMode(string id);
        bool SelectGameMode(string id);
        bool SelectGameMode(GameMode mode);
        event Action<GameMode> OnGameModeChanged;
    }
}
