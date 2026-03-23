using System;
using System.Collections.Generic;
using Gameplay.GameModes;

namespace Gameplay.Match
{
    /// <summary>
    /// Resolves the KitchenClash v1 queue catalog from defaults plus remote config overrides.
    /// </summary>
    public sealed class MatchService : IMatchService
    {
        private readonly IConfigService _configService;
        private readonly MatchQueueDefinition[] _defaults;

        public MatchService(IConfigService configService)
        {
            _configService = configService;
            _defaults = new[]
            {
                new MatchQueueDefinition(
                    "quick_2v2",
                    MatchQueueDefinition.ClassicModeId,
                    "Quick 2v2",
                    2,
                    2,
                    180,
                    1000,
                    GameModeCategory.Trophies,
                    "KitchenArena",
                    false,
                    true),
                new MatchQueueDefinition(
                    "quick_3v3",
                    MatchQueueDefinition.TeamBattleModeId,
                    "Quick 3v3",
                    2,
                    3,
                    180,
                    1500,
                    GameModeCategory.Trophies,
                    "TeamArena",
                    false,
                    true),
                new MatchQueueDefinition(
                    "ranked",
                    MatchQueueDefinition.RankedModeId,
                    "Ranked",
                    2,
                    3,
                    300,
                    1800,
                    GameModeCategory.Ranked,
                    "TeamArena",
                    true,
                    true)
            };
        }

        public IReadOnlyList<MatchQueueDefinition> GetQueues()
        {
            List<MatchQueueDefinition> queues = new();

            foreach (MatchQueueDefinition queue in _defaults)
            {
                MatchQueueDefinition resolvedQueue = ResolveQueue(queue);
                if (resolvedQueue.IsEnabled)
                {
                    queues.Add(resolvedQueue);
                }
            }

            return queues;
        }

        public bool TryGetQueue(string modeId, out MatchQueueDefinition queue)
        {
            foreach (MatchQueueDefinition defaultQueue in _defaults)
            {
                if (!string.Equals(defaultQueue.ModeId, modeId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                queue = ResolveQueue(defaultQueue);
                return queue.IsEnabled;
            }

            queue = null;
            return false;
        }

        private MatchQueueDefinition ResolveQueue(MatchQueueDefinition defaults)
        {
            string prefix = $"queues.{defaults.ModeId}";

            return new MatchQueueDefinition(
                defaults.QueueId,
                defaults.ModeId,
                _configService.GetString($"{prefix}.display_name", defaults.DisplayName),
                Math.Max(1, _configService.GetInt($"{prefix}.team_count", defaults.TeamCount)),
                Math.Max(1, _configService.GetInt($"{prefix}.players_per_team", defaults.PlayersPerTeam)),
                Math.Max(60, _configService.GetInt($"{prefix}.duration_seconds", defaults.DurationSeconds)),
                Math.Max(0, _configService.GetInt($"{prefix}.target_score", defaults.TargetScore)),
                defaults.Category,
                _configService.GetString($"{prefix}.scene_name", defaults.SceneName),
                _configService.GetBool($"{prefix}.is_ranked", defaults.IsRanked),
                _configService.GetBool($"{prefix}.enabled", defaults.IsEnabled));
        }
    }
}
