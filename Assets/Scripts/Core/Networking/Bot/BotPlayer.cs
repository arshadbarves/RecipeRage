using Core.Networking.Common;
using Epic.OnlineServices;
using UnityEngine;

namespace Core.Networking.Bot
{
    /// <summary>
    /// Represents a bot player in the game
    /// </summary>
    public class BotPlayer
    {
        public string BotId { get; private set; }
        public string BotName { get; private set; }
        public int TeamId { get; set; }
        public bool IsReady { get; set; }
        
        public BotPlayer(string botId, string botName, int teamId = 0)
        {
            BotId = botId;
            BotName = botName;
            TeamId = teamId;
            IsReady = true; // Bots are always ready
        }
        
        /// <summary>
        /// Convert bot to PlayerInfo for lobby display
        /// </summary>
        public PlayerInfo ToPlayerInfo()
        {
            return new PlayerInfo
            {
                PlayerId = BotId,
                DisplayName = BotName,
                Team = (TeamId)TeamId,
                IsReady = IsReady,
                IsBot = true,
                IsLocal = false,
                IsHost = false,
                CharacterClass = CharacterClass.Chef,
                ProductUserId = null
            };
        }
    }
}
