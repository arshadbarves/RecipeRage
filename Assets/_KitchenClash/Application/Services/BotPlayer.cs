namespace KitchenClash.Application.Services
{
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
            IsReady = true;
        }
    }
}
