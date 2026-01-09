using System.Collections.Generic;
using Modules.Networking.Models;
using Modules.Shared.Enums;

namespace Modules.Networking.Interfaces
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots, TeamCategory teamCategory = TeamCategory.Neutral);
        void DespawnAllBots();
    }
}