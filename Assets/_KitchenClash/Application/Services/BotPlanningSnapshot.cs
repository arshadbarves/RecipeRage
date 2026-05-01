using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public sealed class BotPlanningSnapshot
    {
        public string[] AvailableIngredients { get; set; }
        public string[] ActiveOrders { get; set; }
        public string[] OccupiedStations { get; set; }
        public float TimeRemaining { get; set; }
        public int? ClaimedOrderId { get; set; }
        public List<BotOrderDescriptor> Orders { get; set; } = new();

        // Held item state
        public bool IsHoldingItem { get; set; }
        public bool HeldItemIsRaw { get; set; }
        public bool HeldItemIsCut { get; set; }
        public bool HeldItemIsCooked { get; set; }
        public bool HeldItemIsBurned { get; set; }
        public bool HeldItemIsPlate { get; set; }
        public IngredientType HeldIngredientType { get; set; }

        // Fire state
        public List<string> StationsOnFire { get; set; } = new();

        // Available stations for targeting
        public List<string> IngredientStationIds { get; set; } = new();
        public List<string> PrepStationIds { get; set; } = new();
        public List<string> CookingStationIds { get; set; } = new();
        public List<string> ServingStationIds { get; set; } = new();
        public List<string> PlateDispenserIds { get; set; } = new();
    }
}
