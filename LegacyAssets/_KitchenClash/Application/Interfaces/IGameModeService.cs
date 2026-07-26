using System;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Models;

namespace KitchenClash.Application
{
    public interface IGameModeService
    {
        GameMode SelectedGameMode { get; }
        GameMode[] GetAvailableGameModes();
        GameMode GetGameMode(string id);
        bool SelectGameMode(string id);
        event Action<GameMode> OnGameModeChanged;
        UniTask<bool> LoadMapAsync(string sceneName);
        UniTask UnloadCurrentMapAsync();
    }
}
