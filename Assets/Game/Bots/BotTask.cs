namespace RecipeRage.Bots
{
    public enum BotTaskKind
    {
        Fetch,
        Chop,
        StartCook,
        CollectCook,
        TakePlate,
        ArrangePlate,
        Serve,
        ClearBurnt,
        Wander
    }

    /// <summary>
    /// One unit of bot work. The planner produces it; BotController executes it.
    /// </summary>
    public sealed class BotTask
    {
        public BotTaskKind Kind { get; }
        public StationBase TargetStation { get; }
        public IngredientType? TargetIngredient { get; }

        public bool IsComplete { get; set; }

        public BotTask(BotTaskKind kind, StationBase targetStation, IngredientType? targetIngredient = null)
        {
            Kind = kind;
            TargetStation = targetStation;
            TargetIngredient = targetIngredient;
        }
    }
}
