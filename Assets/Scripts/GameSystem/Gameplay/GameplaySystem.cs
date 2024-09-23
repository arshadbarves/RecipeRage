using System.Threading.Tasks;
using Gameplay.GameMode;

namespace GameSystem.Gameplay
{
    public class GameplaySystem : IGameSystem
    {
        public BaseGameMode CurrentGameMode { get; private set; }

        public Task InitializeAsync()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public Task CleanupAsync()
        {
            throw new System.NotImplementedException();
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