using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class BotManager
    {
        private readonly List<BotPlayer> _activeBots = new List<BotPlayer>();
        private int _botCounter = 0;

        private static readonly string[] BotNames = new[]
        {
            "Chef Bot Alpha", "Chef Bot Beta", "Chef Bot Gamma", "Chef Bot Delta",
            "Chef Bot Epsilon", "Chef Bot Zeta", "Chef Bot Eta", "Chef Bot Theta",
            "Robo Chef", "Cyber Cook", "AI Chef", "Bot Gordon",
            "Digital Chef", "Virtual Cook", "Auto Chef", "Mecha Cook"
        };

        public List<BotPlayer> CreateBots(int count, int startingTeamId = 0)
        {
            var newBots = new List<BotPlayer>();
            for (int i = 0; i < count; i++)
            {
                string botId = $"bot_{System.Guid.NewGuid():N}";
                string botName = BotNames[_botCounter++ % BotNames.Length];
                int teamId = startingTeamId + (i % 2);
                var bot = new BotPlayer(botId, botName, teamId);
                newBots.Add(bot);
                _activeBots.Add(bot);
            }
            return newBots;
        }

        public void ClearBots() => _activeBots.Clear();
        public List<BotPlayer> GetActiveBots() => new List<BotPlayer>(_activeBots);
        public int GetBotCount() => _activeBots.Count;
    }
}
