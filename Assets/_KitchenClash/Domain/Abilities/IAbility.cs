namespace KitchenClash.Domain
{
    public interface IAbility
    {
        AbilitySlot Slot { get; }
        float Cooldown { get; }
        bool CanActivate(AbilityContext ctx);
        AbilityResult Activate(AbilityContext ctx);
    }
}
