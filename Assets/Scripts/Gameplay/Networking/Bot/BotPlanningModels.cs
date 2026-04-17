using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Networking.Bot
{
    public enum BotTaskType
    {
        Idle,
        ClaimOrder,
        FetchIngredient,
        ProcessIngredient,
        AcquirePlate,
        AssembleDish,
        ServeDish,
        WashDishes,
        Recover
    }

    public enum BotStationType
    {
        IngredientCrate,
        CuttingStation,
        CookingStation,
        CounterStation,
        ServingStation,
        SinkStation,
        PlateDispenser
    }

    public enum BotHeldItemType
    {
        None,
        Ingredient,
        Plate
    }

    public sealed class BotIngredientRequirement
    {
        public int IngredientId { get; set; }
        public bool RequiresCut { get; set; }
        public bool RequiresCooked { get; set; }
    }

    public sealed class BotHeldItemState
    {
        public BotHeldItemType Type { get; set; }
        public int IngredientId { get; set; }
        public bool IsCut { get; set; }
        public bool IsCooked { get; set; }
        public bool IsBurned { get; set; }
        public int PlateIngredientCount { get; set; }

        public static BotHeldItemState Empty()
        {
            return new BotHeldItemState
            {
                Type = BotHeldItemType.None
            };
        }
    }

    public sealed class BotStationDescriptor
    {
        public string StationId { get; set; }
        public BotStationType Type { get; set; }
        public Vector3 Position { get; set; }
        public int TeamId { get; set; }
        public int IngredientId { get; set; }
        public bool IsBusy { get; set; }
        public bool HasItem { get; set; }
        public bool HasPlate { get; set; }
        public bool IsShared { get; set; }
        public int StockCount { get; set; }
        public int StoredIngredientId { get; set; }
        public bool StoredIngredientIsCut { get; set; }
        public bool StoredIngredientIsCooked { get; set; }
        public bool StoredIngredientIsBurned { get; set; }
        public bool StoredIngredientReady { get; set; }
    }

    public sealed class BotOrderDescriptor
    {
        public int OrderId { get; set; }
        public int RecipeId { get; set; }
        public float RemainingTime { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsClaimedByAnotherBot { get; set; }
        public string ClaimedCounterId { get; set; }
        public bool CounterHasPlate { get; set; }
        public bool CounterReadyToServe { get; set; }
        public bool HasInvalidAssembly { get; set; }
        public List<BotIngredientRequirement> MissingIngredients { get; set; } = new List<BotIngredientRequirement>();
    }

    public sealed class BotPlanningSnapshot
    {
        public int TeamId { get; set; }
        public string ClaimedCounterId { get; set; }
        public int? ClaimedOrderId { get; set; }
        public bool OwnSinkDirty { get; set; }
        public BotHeldItemState HeldItem { get; set; } = BotHeldItemState.Empty();
        public List<BotOrderDescriptor> Orders { get; set; } = new List<BotOrderDescriptor>();
        public List<BotStationDescriptor> Stations { get; set; } = new List<BotStationDescriptor>();
    }

    public sealed class BotTaskPlan
    {
        public BotTaskType Type { get; set; }
        public int? OrderId { get; set; }
        public int? RecipeId { get; set; }
        public int? IngredientId { get; set; }
        public string TargetStationId { get; set; }
        public BotStationType? TargetStationType { get; set; }

        public static BotTaskPlan Idle()
        {
            return new BotTaskPlan
            {
                Type = BotTaskType.Idle
            };
        }
    }
}
