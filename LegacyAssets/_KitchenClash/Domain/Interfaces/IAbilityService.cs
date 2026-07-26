namespace KitchenClash.Domain
{
    public interface IAbilityService
    {
        void RegisterChefAbilities(ChefId chefId);
        void RegisterAbility(ChefId chef, IAbility ability);
        AbilityResult TryActivate(AbilitySlot slot, ChefId chef, AbilityContext ctx);
        void ChargeSuper(ChefId chef, int dishesServed);
        int GetSuperCharges(ChefId chef);
        bool IsSuperReady(ChefId chef);
        float GetCooldownRemaining(ChefId chef, AbilitySlot slot);
        AbilityDefinition GetPassiveDefinition(ChefId chef);
        void UpdateCooldowns(float deltaTime);
        void ClearAll();
    }
}
