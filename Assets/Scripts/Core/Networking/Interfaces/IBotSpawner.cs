using System.Collections.Generic;
using Core.Core.Shared.Enums;

namespace Core.Core.Networking.Interfaces
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots, TeamCategory teamCategory = TeamCategory.Neutral);
        void DespawnAllBots();
    }
}