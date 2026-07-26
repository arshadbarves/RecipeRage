namespace RecipeRage.Bots
{
    /// <summary>Priority 4: carrying chopped, uncooked, needed ingredient → start cooking.</summary>
    public sealed class StartCookEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (!item.Definition.RequiresCooking || item.IsCooked || !item.IsChopped)
                {
                    continue;
                }
                if (!IsNeeded(snapshot, item.Definition.Type))
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Cooking && !station.HasReadyItem && !station.IsBurning && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.StartCook, station.Station, item.Definition.Type);
                    }
                }
            }
            return null;
        }

        private static bool IsNeeded(KitchenSnapshot snapshot, IngredientType type)
        {
            foreach (var needed in snapshot.NeededIngredients)
            {
                if (needed == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
