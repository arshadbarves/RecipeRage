using System.Collections.Generic;

namespace KitchenClash.Application.Services
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots);
        void DespawnAllBots();
    }
}
