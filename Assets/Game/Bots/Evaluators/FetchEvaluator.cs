namespace RecipeRage.Bots
{
    /// <summary>Priority 6: recipe needs an ingredient we don't carry → fetch from crate.</summary>
    public sealed class FetchEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (snapshot.Carry.Items.Count >= 2) // default capacity; chef bonus applied in Slice 4
            {
                return null;
            }

            foreach (var needed in snapshot.NeededIngredients)
            {
                if (AlreadyCovered(snapshot, needed))
                {
                    continue;
                }

                foreach (var station in snapshot.Stations)
                {
                    if (station.Kind == StationKind.Crate
                        && station.CrateIngredient == needed
                        && !station.IsClaimed)
                    {
                        return new BotTask(BotTaskKind.Fetch, station.Station, needed);
                    }
                }
            }
            return null;
        }

        private static bool AlreadyCovered(KitchenSnapshot snapshot, IngredientType type)
        {
            foreach (var item in snapshot.Carry.Items)
            {
                if (item.Definition.Type == type)
                {
                    return true;
                }
            }
            if (snapshot.Carry.HasPlate)
            {
                foreach (var item in snapshot.Carry.Plate.Contents)
                {
                    if (item.Definition.Type == type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
