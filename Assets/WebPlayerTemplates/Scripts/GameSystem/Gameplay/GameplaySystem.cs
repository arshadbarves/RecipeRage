using System.Threading.Tasks;
using Core;
using Gameplay.GameMode;

namespace GameSystem.Gameplay
{
    public class GameplaySystem : IGameSystem
    {
        public BaseGameMode CurrentGameMode { get; private set; }

        public Task InitializeAsync()
        {
            GameModeConfig[] gameModeConfigs = GameManager.Instance.GetGameModeConfigs();
            return Task.CompletedTask;
        }

        public void Update()
        {
        }

        public Task CleanupAsync()
        {
            return Task.CompletedTask;
        }

        public void StartGame()
        {
            CurrentGameMode.StartGame();
        }

        public void GameOver()
        {
            CurrentGameMode.GameOver();
        }

        public void SetGameMode(BaseGameMode baseGameMode)
        {
            CurrentGameMode = baseGameMode;
        }
    }
}