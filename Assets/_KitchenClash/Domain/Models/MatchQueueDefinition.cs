using System;

namespace KitchenClash.Domain
{
    public sealed class MatchQueueDefinition
    {
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

        // Ranked mode fields
        public int MinLevelRequired { get; }
        public string SeasonId { get; }

        // Event mode fields
        public string EventId { get; }
        public DateTime? EventStartUtc { get; }
        public DateTime? EventEndUtc { get; }
        public string EventDescription { get; }

        public MatchQueueDefinition(string queueId, string modeId, string displayName,
            int teamCount, int playersPerTeam, int durationSeconds, int targetScore,
            GameModeCategory category, string sceneName, bool isRanked, bool isEnabled,
            int minLevelRequired = 0, string seasonId = null,
            string eventId = null, DateTime? eventStartUtc = null,
            DateTime? eventEndUtc = null, string eventDescription = null)
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
            MinLevelRequired = minLevelRequired;
            SeasonId = seasonId;
            EventId = eventId;
            EventStartUtc = eventStartUtc;
            EventEndUtc = eventEndUtc;
            EventDescription = eventDescription;
        }

        /// <summary>
        /// Returns true if the event queue is currently active based on UTC time.
        /// Non-event queues always return true.
        /// </summary>
        public bool IsEventActive()
        {
            if (string.IsNullOrEmpty(EventId)) return true;
            var now = DateTime.UtcNow;
            if (EventStartUtc.HasValue && now < EventStartUtc.Value) return false;
            if (EventEndUtc.HasValue && now > EventEndUtc.Value) return false;
            return true;
        }
    }
}
