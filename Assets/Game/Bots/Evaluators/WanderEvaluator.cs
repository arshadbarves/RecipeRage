namespace RecipeRage.Bots
{
    /// <summary>Fallback: nothing to do → wander (keeps bots alive-looking, repositions).</summary>
    public sealed class WanderEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            return new BotTask(BotTaskKind.Wander, null);
        }
    }
}
