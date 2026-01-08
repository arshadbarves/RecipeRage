using System.Collections.Generic;
using Gameplay.Networking.Bot;
using Core.Enums;

namespace Modules.Networking.Interfaces
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots, TeamCategory teamCategory = TeamCategory.Neutral);
        void DespawnAllBots();
    }
}
