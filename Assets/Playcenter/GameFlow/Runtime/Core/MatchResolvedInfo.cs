namespace Playcenter.GameFlow
{
    /// <summary>
    /// Snapshot when matchmaking resolves (humans and/or bots).
    /// </summary>
    public sealed class MatchResolvedInfo
    {
        public string LobbyId { get; set; }
        public string ModeId { get; set; }
        public string MapId { get; set; }
        public int TeamSize { get; set; }
        public int HumanCount { get; set; }
        public int BotCount { get; set; }
        public bool FilledWithBots { get; set; }
    }
}
