using System.Collections.Generic;
using Core.Networking.Models;
using Core.Shared.Enums;

namespace Core.Networking.Interfaces
{
    public interface IBotSpawner
    {
        void SpawnBots(List<BotPlayer> bots, TeamCategory teamCategory = TeamCategory.Neutral);
        void DespawnAllBots();
    }
}