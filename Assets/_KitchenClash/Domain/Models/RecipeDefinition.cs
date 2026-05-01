namespace KitchenClash.Domain
{
    /// <summary>
    /// Pure domain model describing a recipe's requirements.
    /// Independent of Unity ScriptableObjects — used by OrderService and validation.
    /// </summary>
    public sealed class RecipeDefinition
    {
        public string RecipeId { get; set; }
        public string DisplayName { get; set; }
        public int Tier { get; set; } // 1, 2, or 3
        public IngredientType[] RequiredIngredients { get; set; }
        public float BaseTimeLimitSec { get; set; }
        public string KitchenTheme { get; set; }
        public int BasePoints { get; set; }
    }
}
