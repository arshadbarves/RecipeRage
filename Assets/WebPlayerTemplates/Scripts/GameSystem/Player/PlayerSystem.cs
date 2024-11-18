using System;
using System.Threading.Tasks;

namespace GameSystem.Player
{
    public class PlayerSystem : IGameSystem
    {
        public PlayerData PlayerData { get; private set; }

        public Task InitializeAsync()
        {
            PlayerData = new PlayerData();
            return Task.CompletedTask;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public Task CleanupAsync()
        {
            throw new NotImplementedException();
        }
    }
}