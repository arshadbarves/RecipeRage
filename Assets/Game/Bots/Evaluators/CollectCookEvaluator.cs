namespace RecipeRage.Bots
{
    /// <summary>Priority 3: cooked item waiting → collect before it burns.</summary>
    public sealed class CollectCookEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Cooking && station.HasReadyItem && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.CollectCook, station.Station);
                }
            }
            return null;
        }
    }
}
