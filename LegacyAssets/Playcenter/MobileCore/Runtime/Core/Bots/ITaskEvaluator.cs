namespace Playcenter.MobileCore
{
    /// <summary>One link in the planner chain. Return null to pass to the next evaluator.</summary>
    public interface ITaskEvaluator<TSnapshot, TTask>
    {
        TTask Evaluate(TSnapshot snapshot);
    }
}
