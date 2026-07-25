using System;
using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Owns all bot brains; ticks them under an IBotBudget time-slice.
    /// Generic over the game's snapshot/task/intent pipeline; the game supplies
    /// snapshot + act callbacks so the host stays domain-free.
    /// </summary>
    public sealed class BotHost<TSnapshot, TTask>
    {
        private readonly List<BotBrain<TSnapshot, TTask>> _brains = new List<BotBrain<TSnapshot, TTask>>();
        private readonly IBotBudget _budget;
        private readonly IGameClock _clock;
        private readonly Func<TSnapshot> _snapshotSource;
        private readonly Action<BotBrain<TSnapshot, TTask>, TTask> _act;

        public IReadOnlyList<BotBrain<TSnapshot, TTask>> Brains => _brains;

        public BotHost(
            IBotBudget budget,
            IGameClock clock,
            Func<TSnapshot> snapshotSource,
            Action<BotBrain<TSnapshot, TTask>, TTask> act)
        {
            _budget = budget ?? throw new ArgumentNullException(nameof(budget));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
            _act = act ?? throw new ArgumentNullException(nameof(act));
            _clock.Ticked += OnTicked;
        }

        public void Add(BotBrain<TSnapshot, TTask> brain)
        {
            _brains.Add(brain);
        }

        private void OnTicked(float deltaTime)
        {
            _budget.ResetTick();
            TSnapshot snapshot = _snapshotSource();

            for (int i = 0; i < _brains.Count; i++)
            {
                if (!_budget.TryConsume(0.5f))
                {
                    break; // budget exhausted: remaining bots act next tick
                }

                TTask task = _brains[i].Think(snapshot);
                _act(_brains[i], task);
            }
        }
    }
}
