namespace RecipeRage
{
    /// <summary>
    /// Brawl Stars-style progressive costs. Total to max one chef: 17,000 coins.
    /// </summary>
    public static class ChefUpgradeCosts
    {
        private static readonly int[] Costs = { 100, 200, 400, 700, 1100, 1700, 2600, 4000, 6200 };

        public const int MaxLevel = 10;

        public static int ForLevel(int currentLevel)
        {
            if (currentLevel < 1 || currentLevel >= MaxLevel)
            {
                return 0;
            }
            return Costs[currentLevel - 1];
        }
    }
}
