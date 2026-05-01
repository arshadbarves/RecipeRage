using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class MatchService : IMatchService
    {
        private readonly IConfigService _cfg;
        private List<MatchQueueDefinition> _queues;

        public MatchService(IConfigService cfg)
        {
            _cfg = cfg;
            BuildQueues();
        }

        public IReadOnlyList<MatchQueueDefinition> GetQueues() => _queues;

        public bool TryGetQueue(string modeId, out MatchQueueDefinition queue)
        {
            queue = _queues.FirstOrDefault(q => q.ModeId == modeId);
            return queue != null;
        }

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
            };
        }
    }
}
