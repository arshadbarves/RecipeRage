namespace RecipeRage.Bots
{
    public interface IBotBudget
    {
        bool TryConsume(int microseconds);
    }

    /// <summary>
    /// Per-tick thinking budget. Evaluators check before expensive work;
    /// over budget = planning resumes next tick. Config: bot_budget_ms (2ms).
    /// Stopwatch via alias — NGO dependency chain ships System.dll types that
    /// collide with netstandard's Stopwatch in some reference sets.
    /// </summary>
    public sealed class BotBudget : IBotBudget
    {
        private readonly BudgetStopwatch _stopwatch = new BudgetStopwatch();
        private readonly long _budgetTicks;

        public BotBudget(int budgetMs)
        {
            _budgetTicks = budgetMs * (BudgetStopwatch.Frequency / 1000);
        }

        public void BeginTick()
        {
            _stopwatch.Restart();
        }

        public bool TryConsume(int microseconds)
        {
            return _stopwatch.ElapsedTicks + microseconds * (BudgetStopwatch.Frequency / 1_000_000) < _budgetTicks;
        }

        private sealed class BudgetStopwatch : System.Diagnostics.Stopwatch { }
    }
}
