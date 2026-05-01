using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots, TeamCategory team = TeamCategory.Neutral);
        void DespawnAllBots();
        int GetSpawnedBotCount();
    }
}
