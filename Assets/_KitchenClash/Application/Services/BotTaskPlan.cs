using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public sealed class BotTaskPlan
    {
        public BotTaskType Type { get; set; }
        public string TaskType { get; set; }
        public string TargetId { get; set; }
        public string TargetStationId { get; set; }
        public int? OrderId { get; set; }
        public float Priority { get; set; }
        public bool IsComplete { get; set; }
        public IngredientType TargetIngredient { get; set; }
        public float DelayBeforeAction { get; set; }

        public static BotTaskPlan Idle() => new BotTaskPlan { Type = BotTaskType.Idle };
    }
}
