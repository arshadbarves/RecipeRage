using System.Collections.Generic;
using System.Linq;
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

        private void BuildQueues()
        {
            int matchDuration = _cfg.Get("match_duration_sec", 180);

            _queues = new List<MatchQueueDefinition>
            {
                new("quick_2v2", "quick_2v2", "Quick Match 2v2", 2, 2, matchDuration, 0,
                    GameModeCategory.Trophies, "sushi_shuffle", false, true),
                new("quick_3v3", "quick_3v3", "Quick Match 3v3", 2, 3, matchDuration, 0,
                    GameModeCategory.Trophies, "pirate_pot", false, true),
                new("ranked", "ranked", "Ranked", 2, 2, _cfg.Get("ranked_duration_sec", 300), 0,
                    GameModeCategory.Ranked, "burger_boulevard", true, _cfg.Get("enableRankedMode", true)),
                new("event", "event", "Event Mode", 2, 2, matchDuration, 0,
                    GameModeCategory.Trophies, "haunted_kitchen", false, _cfg.Get("enableEventMode", false)),
            };
        }
    }
}
