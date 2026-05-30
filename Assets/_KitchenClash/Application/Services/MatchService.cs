using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Application.Config;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class MatchService : IMatchService
    {
        private readonly IConfigService _cfg;
        private readonly MapRotationCalculator _mapRotation;
        private List<MatchQueueDefinition> _queues;

        public MatchService(IConfigService cfg, MapRotationCalculator mapRotation)
        {
            _cfg = cfg;
            _mapRotation = mapRotation;
            BuildQueues();
        }

        public IReadOnlyList<MatchQueueDefinition> GetQueues() => _queues;

        public bool TryGetQueue(string modeId, out MatchQueueDefinition queue)
        {
            queue = _queues.FirstOrDefault(q => q.ModeId == modeId);
            return queue != null;
        }

        public string GetCurrentMap(string queueId) => _mapRotation.GetCurrentMap(queueId);

        public bool CanJoinQueue(string queueId, int playerLevel)
        {
            if (!TryGetQueue(queueId, out MatchQueueDefinition queue))
            {
                return false;
            }

            if (!queue.IsEnabled)
            {
                return false;
            }

            if (playerLevel < queue.MinLevelRequired)
            {
                return false;
            }

            if (!queue.IsEventActive())
            {
                return false;
            }

            return true;
        }

        private void BuildQueues()
        {
            string currentSeasonId = _cfg.Get("ranked_season_id", "season_1");
            int normalDuration = _cfg.Get(RemoteConfigKeys.MatchDurationSec,
                RemoteConfigKeys.Defaults.MatchDurationSec);
            int rankedDuration = _cfg.Get(RemoteConfigKeys.MatchDurationRankedSec,
                RemoteConfigKeys.Defaults.MatchDurationRankedSec);

            _queues = new List<MatchQueueDefinition>
            {
                // quick_2v2: 2v2, 4 players
                new("quick_2v2", "quick_2v2", "Quick Match 2v2", 2, 2, normalDuration, 0,
                    GameModeCategory.Trophies, "sushi_shuffle", false, true),

                // quick_3v3: 3v3, 6 players
                new("quick_3v3", "quick_3v3", "Quick Match 3v3", 2, 3, normalDuration, 0,
                    GameModeCategory.Trophies, "pirate_pot", false, true),

                // ranked: 2v2, 4 players, uses ranked duration, requires level 5+
                new("ranked", "ranked", "Ranked", 2, 2, rankedDuration, 0,
                    GameModeCategory.Ranked, "burger_boulevard", true,
                    _cfg.Get("enableRankedMode", true),
                    minLevelRequired: 5,
                    seasonId: currentSeasonId),

                // event: 2v2, 4 players, special rules
                new("event", "event", "Event Mode", 2, 2, normalDuration, 0,
                    GameModeCategory.Special, "clash_kitchen", false,
                    _cfg.Get("enableEventMode", false),
                    eventId: _cfg.Get("event_id", (string)null),
                    eventStartUtc: ParseUtcDate(_cfg.Get("event_start_utc", (string)null)),
                    eventEndUtc: ParseUtcDate(_cfg.Get("event_end_utc", (string)null)),
                    eventDescription: _cfg.Get("event_description", "Special Event")),
            };
        }

        private static DateTime? ParseUtcDate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTime dt) ? dt : null;
        }
    }
}
