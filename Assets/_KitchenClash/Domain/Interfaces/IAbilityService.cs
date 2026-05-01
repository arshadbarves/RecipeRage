namespace KitchenClash.Domain
{
    public interface IAbilityService
    {
        AbilityResult TryActivate(AbilitySlot slot, ChefId chef, AbilityContext ctx);
        void ChargeSuper(ChefId chef, int dishesServed);
        float GetCooldownRemaining(ChefId chef, AbilitySlot slot);
    }
}
