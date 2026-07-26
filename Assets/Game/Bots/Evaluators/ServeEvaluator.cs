using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>Priority 2: plate full (or recipe complete) → serve it.</summary>
    public sealed class ServeEvaluator : ITaskEvaluator
    {
        public BotTask Evaluate(KitchenSnapshot snapshot)
        {
            if (!snapshot.Carry.HasPlate || snapshot.CurrentRecipe == null)
            {
                return null;
            }

            var required = snapshot.CurrentRecipe.RequiredIngredients.Length;
            if (snapshot.Carry.Plate.Contents.Count < required)
            {
                return null;
            }

            foreach (var station in snapshot.Stations)
            {
                if (station.Kind == StationKind.Serving && !station.IsClaimed)
                {
                    return new BotTask(BotTaskKind.Serve, station.Station);
                }
            }
            return null;
        }
    }
}
