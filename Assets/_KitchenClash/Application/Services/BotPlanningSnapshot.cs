using System.Collections.Generic;

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
    }
}
