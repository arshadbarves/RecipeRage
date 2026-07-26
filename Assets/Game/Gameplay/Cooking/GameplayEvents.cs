namespace RecipeRage
{
    public readonly struct IngredientFetchedEvent
    {
        public IngredientType Type { get; }
        public IngredientFetchedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct IngredientChoppedEvent
    {
        public IngredientType Type { get; }
        public IngredientChoppedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct CookingStartedEvent
    {
        public int StationId { get; }
        public CookingStartedEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct CookingCompletedEvent
    {
        public int StationId { get; }
        public CookingCompletedEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct IngredientBurntEvent
    {
        public int StationId { get; }
        public IngredientBurntEvent(int stationId) { StationId = stationId; }
    }

    public readonly struct PlateTakenEvent { }

    public readonly struct IngredientPlatedEvent
    {
        public IngredientType Type { get; }
        public IngredientPlatedEvent(IngredientType type) { Type = type; }
    }

    public readonly struct RecipeServedEvent
    {
        public string RecipeId { get; }
        public RecipeServedEvent(string recipeId) { RecipeId = recipeId; }
    }

    public readonly struct MatchStartedEvent { }

    public readonly struct MatchEndedEvent
    {
        public bool Won { get; }
        public int TeamRecipes { get; }
        public int EnemyRecipes { get; }
        public MatchEndedEvent(bool won, int teamRecipes, int enemyRecipes)
        {
            Won = won;
            TeamRecipes = teamRecipes;
            EnemyRecipes = enemyRecipes;
        }
    }
}
