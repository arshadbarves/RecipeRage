using Gameplay.GameModes;

namespace Gameplay.Match
{
    /// <summary>
    /// GDD-aligned queue definition used to drive launch matchmaking behavior.
    /// </summary>
    public sealed class MatchQueueDefinition
    {
        public const string ClassicModeId = "classic";
        public const string TeamBattleModeId = "team_battle";
        public const string RankedModeId = "ranked";

        public MatchQueueDefinition(
            string queueId,
            string modeId,
            string displayName,
            int teamCount,
            int playersPerTeam,
            int durationSeconds,
            int targetScore,
            GameModeCategory category,
            string sceneName,
            bool isRanked,
            bool isEnabled)
        {
            QueueId = queueId;
            ModeId = modeId;
            DisplayName = displayName;
            TeamCount = teamCount;
            PlayersPerTeam = playersPerTeam;
            DurationSeconds = durationSeconds;
            TargetScore = targetScore;
            Category = category;
            SceneName = sceneName;
            IsRanked = isRanked;
            IsEnabled = isEnabled;
        }

        public string QueueId { get; }
        public string ModeId { get; }
        public string DisplayName { get; }
        public int TeamCount { get; }
        public int PlayersPerTeam { get; }
        public int DurationSeconds { get; }
        public int TargetScore { get; }
        public GameModeCategory Category { get; }
        public string SceneName { get; }
        public bool IsRanked { get; }
        public bool IsEnabled { get; }
        public int MaxPlayers => TeamCount * PlayersPerTeam;
    }
}
