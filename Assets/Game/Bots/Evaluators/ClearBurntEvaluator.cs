namespace RecipeRage.Bots
{
    /// <summary>Priority 1: clear burnt food blocking a stove.</summary>
    public sealed class ClearBurntEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Cooking && station.IsBurning && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.ClearBurnt, station.Station);
                }
            }
            return null;
        }
    }
}
