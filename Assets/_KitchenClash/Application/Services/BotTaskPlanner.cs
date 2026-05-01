namespace KitchenClash.Application.Services
{
    public sealed class BotTaskPlanner
    {
        public BotTaskPlan Plan(BotPlanningSnapshot snapshot)
        {
            return new BotTaskPlan { Type = BotTaskType.Idle };
        }
    }
}
