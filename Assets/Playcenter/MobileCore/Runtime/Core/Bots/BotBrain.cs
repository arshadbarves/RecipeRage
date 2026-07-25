using System;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Per-bot decision loop: snapshot → plan → claim → task. Seeded Random keeps
    /// behavior deterministic per match seed. TTask chosen by the game.
    /// </summary>
    public sealed class BotBrain<TSnapshot, TTask>
    {
        private readonly string _botId;
        private readonly TaskPlanner<TSnapshot, TTask> _planner;
        private readonly Random _random;

        public TTask CurrentTask { get; private set; }
        public string BotId => _botId;
        public Random Random => _random;

        public BotBrain(string botId, TaskPlanner<TSnapshot, TTask> planner, int seed)
        {
            _botId = botId ?? throw new ArgumentNullException(nameof(botId));
            _planner = planner ?? throw new ArgumentNullException(nameof(planner));
            _random = new Random(seed);
        }

        public TTask Think(TSnapshot snapshot)
        {
            CurrentTask = _planner.Plan(snapshot);
            return CurrentTask;
        }
    }
}
