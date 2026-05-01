namespace KitchenClash.Domain
{
    public sealed class AbilityContext
    {
        public ChefId Chef { get; }
        public float AimDirX { get; }
        public float AimDirY { get; }
        public int DishesServed { get; }
        public float MatchTimeRemaining { get; }

        public AbilityContext(ChefId chef, float aimDirX = 0f, float aimDirY = 0f,
            int dishesServed = 0, float matchTimeRemaining = 0f)
        {
            Chef = chef;
            AimDirX = aimDirX;
            AimDirY = aimDirY;
            DishesServed = dishesServed;
            MatchTimeRemaining = matchTimeRemaining;
        }
    }
}
