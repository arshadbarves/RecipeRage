using System.Diagnostics;

namespace RecipeRage.Bots
{
    public interface IBotBudget
    {
        bool TryConsume(int microseconds);
    }

    /// <summary>
    /// Per-tick thinking budget. Evaluators check before expensive work;
    /// over budget = planning resumes next tick. Config: bot_budget_ms (2ms).
    /// </summary>
    public sealed class BotBudget : IBotBudget
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly long _budgetTicks;

        public BotBudget(int budgetMs)
        {
            _budgetTicks = budgetMs * (Stopwatch.Frequency / 1000);
        }

        public void BeginTick()
        {
            _stopwatch.Restart();
        }

        public bool TryConsume(int microseconds)
        {
            return _stopwatch.ElapsedTicks + microseconds * (Stopwatch.Frequency / 1_000_000) < _budgetTicks;
        }
    }
}
