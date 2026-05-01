namespace KitchenClash.Domain
{
    /// <summary>
    /// Logical ingredient types used by recipe definitions and the order system.
    /// Maps to Ingredient ScriptableObject instances at the infrastructure layer.
    /// </summary>
    public enum IngredientType
    {
        None = 0,

        // Proteins
        Egg = 1,
        Fish = 2,
        Beef = 3,
        Chicken = 4,

        // Grains / Carbs
        Rice = 10,
        Bread = 11,
        Pasta = 12,
        Dough = 13,
        Noodles = 14,

        // Vegetables
        Lettuce = 20,
        Tomato = 21,
        Onion = 22,
        Vegetables = 23,

        // Dairy
        Cheese = 30,
        Butter = 31,

        // Condiments / Misc
        Sauce = 40,
        Seaweed = 41,
        Broth = 42,
        Cream = 43,
        Frosting = 44
    }
}
