using System.Collections.Generic;
using RecipeRage.Net;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Builds the immutable world view each bot plans against. Reads from
    /// MatchRuntimeRegistry (never scene searches).
    /// </summary>
    public sealed class KitchenSnapshotBuilder
    {
        private readonly MatchRuntimeRegistry _registry;
        private readonly BotClaimRegistry _claims;
        private readonly MatchController _match;
        private readonly List<StationInfo> _stationBuffer = new List<StationInfo>(16);
        private readonly List<IngredientType> _neededBuffer = new List<IngredientType>(4);

        public KitchenSnapshotBuilder(MatchRuntimeRegistry registry, BotClaimRegistry claims, MatchController match)
        {
            _registry = registry;
            _claims = claims;
            _match = match;
        }

        public KitchenSnapshot Build(PlayerCarry carry, Vector3 botPosition)
        {
            _stationBuffer.Clear();
            _neededBuffer.Clear();

            var recipe = _match.CurrentRecipe;
            if (recipe != null)
            {
                foreach (var requirement in recipe.RequiredIngredients)
                {
                    _neededBuffer.Add(requirement.Type);
                }
            }

            return new KitchenSnapshot(
                carry,
                recipe,
                new List<StationInfo>(_stationBuffer),
                new List<IngredientType>(_neededBuffer),
                botPosition);
        }
    }
}
