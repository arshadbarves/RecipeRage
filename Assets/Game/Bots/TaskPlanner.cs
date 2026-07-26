using System.Collections.Generic;

namespace RecipeRage.Bots
{
    public sealed class TaskPlanner
    {
        private readonly List<ITaskEvaluator> _evaluators = new List<ITaskEvaluator>(8);

        public void Register(ITaskEvaluator evaluator)
        {
            _evaluators.Add(evaluator);
        }

        public BotTask Plan(KitchenSnapshot snapshot)
        {
            for (int i = 0; i < _evaluators.Count; i++)
            {
                var task = _evaluators[i].Evaluate(snapshot);
                if (task != null)
                {
                    return task;
                }
            }
            return null;
        }
    }
}
