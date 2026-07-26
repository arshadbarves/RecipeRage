namespace RecipeRage.Bots
{
    /// <summary>Priority 5: carrying unchopped, needed ingredient → chop it.</summary>
    public sealed class ChopEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (!item.Definition.RequiresChopping || item.IsChopped)
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Cutting && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.Chop, station.Station, item.Definition.Type);
                    }
                }
            }
            return null;
        }
    }
}
