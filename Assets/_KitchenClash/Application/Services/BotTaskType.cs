namespace KitchenClash.Application.Services
{
    public enum BotTaskType
    {
        Idle,
        FetchIngredient,
        ProcessIngredient,
        AcquirePlate,
        AssembleDish,
        ServeDish,
        WashDishes,
        ClaimOrder,
        Recover,
        BringToPrep,
        BringToCooking,
        DeliverToServing,
        ExtinguishFire,
        Wander
    }
}
