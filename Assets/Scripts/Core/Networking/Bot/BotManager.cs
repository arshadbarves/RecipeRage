using System.Collections.Generic;
using Core.Logging;

namespace Core.Networking.Bot
{
    /// <summary>
    /// Manages bot players for filling matches
    /// </summary>
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
        
        /// <summary>
        /// Create bots to fill remaining slots
        /// </summary>
        public List<BotPlayer> CreateBots(int count, int startingTeamId = 0)
        {
            var newBots = new List<BotPlayer>();
            
            for (int i = 0; i < count; i++)
            {
                string botId = $"bot_{System.Guid.NewGuid():N}";
                string botName = GetBotName();
                int teamId = startingTeamId + (i % 2); // Alternate teams
                
                var bot = new BotPlayer(botId, botName, teamId);
                newBots.Add(bot);
                _activeBots.Add(bot);
                
                GameLogger.Log($"Created bot: {botName} (Team {teamId})");
            }
            
            return newBots;
        }
        
        /// <summary>
        /// Get a bot name from the pool
        /// </summary>
        private string GetBotName()
        {
            string baseName = BotNames[_botCounter % BotNames.Length];
            _botCounter++;
            
            // Add number if we've cycled through all names
            if (_botCounter > BotNames.Length)
            {
                int suffix = (_botCounter / BotNames.Length);
                return $"{baseName} {suffix}";
            }
            
            return baseName;
        }
        
        /// <summary>
        /// Clear all active bots
        /// </summary>
        public void ClearBots()
        {
            GameLogger.Log($"Clearing {_activeBots.Count} bots");
            _activeBots.Clear();
        }
        
        /// <summary>
        /// Get all active bots
        /// </summary>
        public List<BotPlayer> GetActiveBots()
        {
            return new List<BotPlayer>(_activeBots);
        }
        
        /// <summary>
        /// Get active bot count
        /// </summary>
        public int GetBotCount()
        {
            return _activeBots.Count;
        }
    }
}
