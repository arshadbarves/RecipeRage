using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Priority-chain planner: evaluators run in registration order, first non-null
    /// task wins. Games register domain evaluators (RecipeRage: fire → deliver → cook → prep).
    /// </summary>
    public sealed class TaskPlanner<TSnapshot, TTask>
    {
        private readonly List<ITaskEvaluator<TSnapshot, TTask>> _evaluators =
            new List<ITaskEvaluator<TSnapshot, TTask>>();

        public void Register(ITaskEvaluator<TSnapshot, TTask> evaluator)
        {
            _evaluators.Add(evaluator);
        }

        public TTask Plan(TSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return default;
            }

            for (int i = 0; i < _evaluators.Count; i++)
            {
                TTask task = _evaluators[i].Evaluate(snapshot);
                if (task != null)
                {
                    return task;
                }
            }

            return default;
        }
    }
}
