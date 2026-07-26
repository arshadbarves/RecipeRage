namespace RecipeRage.Bots
{
    /// <summary>Priority 5.5 (registered after Chop, before Fetch): need a plate.</summary>
    public sealed class TakePlateEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (snapshot.Carry.HasPlate || snapshot.CurrentRecipe == null)
            {
                return null;
            }

            // Take a plate once we hold at least one ready (chopped+cooked) ingredient
            foreach (var item in snapshot.Carry.Items)
            {
                var ready = (!item.Definition.RequiresChopping || item.IsChopped)
                         && (!item.Definition.RequiresCooking || item.IsCooked);
                if (ready)
                {
                    foreach (var station in snapshot.Stations)
                    {
                        if (station.Kind == StationKind.Plate && !station.IsClaimed)
                        {
                            return new BotTask(BotTaskKind.TakePlate, station.Station);
                        }
                    }
                }
            }
            return null;
        }
    }

    /// <summary>Registered with TakePlate: holding plate + ready items → arrange.</summary>
    public sealed class ArrangePlateEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (!snapshot.Carry.HasPlate || snapshot.Carry.Items.Count == 0 || snapshot.Carry.Plate.IsFull)
            {
                return null;
            }

            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Plate && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.ArrangePlate, station.Station);
                }
            }
            return null;
        }
    }
}
