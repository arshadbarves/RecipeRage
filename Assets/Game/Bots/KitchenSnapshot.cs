using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Immutable per-tick world view for one bot. Built by KitchenSnapshotBuilder
    /// from the MatchRuntimeRegistry — never from scene searches.
    /// </summary>
    public sealed class KitchenSnapshot
    {
        public PlayerCarry Carry { get; }
        public RecipeDefinition CurrentRecipe { get; }
        public IReadOnlyList<StationInfo> Stations { get; }
        public IReadOnlyList<IngredientType> NeededIngredients { get; }
        public Vector3 BotPosition { get; }

        public KitchenSnapshot(
            PlayerCarry carry,
            RecipeDefinition currentRecipe,
            IReadOnlyList<StationInfo> stations,
            IReadOnlyList<IngredientType> neededIngredients,
            Vector3 botPosition)
        {
            Carry = carry;
            CurrentRecipe = currentRecipe;
            Stations = stations;
            NeededIngredients = neededIngredients;
            BotPosition = botPosition;
        }
    }

    public sealed class StationInfo
    {
        public StationBase Station { get; }
        public StationKind Kind { get; }
        public Vector3 Position { get; }
        public bool IsClaimed { get; }
        public bool HasReadyItem { get; }
        public bool IsBurning { get; }
        public IngredientType? CrateIngredient { get; }

        public StationInfo(
            StationBase station,
            StationKind kind,
            Vector3 position,
            bool isClaimed,
            bool hasReadyItem,
            bool isBurning,
            IngredientType? crateIngredient)
        {
            Station = station;
            Kind = kind;
            Position = position;
            IsClaimed = isClaimed;
            HasReadyItem = hasReadyItem;
            IsBurning = isBurning;
            CrateIngredient = crateIngredient;
        }
    }

    public enum StationKind
    {
        Crate,
        Cutting,
        Cooking,
        Plate,
        Counter,
        Serving
    }
}
