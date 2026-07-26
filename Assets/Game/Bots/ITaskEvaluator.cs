namespace RecipeRage.Bots
{
    /// <summary>
    /// Ordered evaluators: first non-null task wins. Chain order (fixed):
    /// burnt-recovery → serve → collect → cook → chop → fetch → wander.
    /// </summary>
    public interface ITaskEvaluator
    {
        BotTask Evaluate(KitchenSnapshot snapshot);
    }
}
