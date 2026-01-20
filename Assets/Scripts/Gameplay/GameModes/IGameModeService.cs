using System;
using Cysharp.Threading.Tasks;

namespace Gameplay.GameModes
{
    public interface IGameModeService
    {
        GameMode SelectedGameMode { get; }
        GameMode[] GetAvailableGameModes();
        GameMode GetGameMode(string id);
        bool SelectGameMode(string id);
        bool SelectGameMode(GameMode mode);
        event Action<GameMode> OnGameModeChanged;
        
        // Map loading (merged from MapLoader)
        UniTask<bool> LoadMapAsync(string sceneName);
        UniTask UnloadCurrentMapAsync();
        
        // Game mode logic factory
        IGameModeLogic CreateGameModeLogic();
    }
}
