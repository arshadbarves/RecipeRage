using System;

namespace KitchenClash.Domain
{
    public sealed class OrderModel
    {
        public Guid Id { get; }
        public string RecipeId { get; }
        public int Tier { get; }
        public IngredientType[] RequiredIngredients { get; }
        public float TimeLimit { get; }
        public float CreatedAt { get; }
        public float ExpiresAt { get; }
        public float RemainingTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsExpired { get; set; }
        public int PointValue { get; set; }

        public OrderModel(Guid id, string recipeId, int tier, IngredientType[] requiredIngredients,
            float timeLimit, float createdAt)
        {
            Id = id;
            RecipeId = recipeId;
            Tier = tier;
            RequiredIngredients = requiredIngredients;
            TimeLimit = timeLimit;
            CreatedAt = createdAt;
            ExpiresAt = createdAt + timeLimit;
            RemainingTime = timeLimit;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility.
        /// </summary>
        public OrderModel(Guid id, int recipeIdNumeric, int tier, float timeLimit, float creationTime)
        {
            Id = id;
            RecipeId = recipeIdNumeric.ToString();
            Tier = tier;
            RequiredIngredients = System.Array.Empty<IngredientType>();
            TimeLimit = timeLimit;
            CreatedAt = creationTime;
            ExpiresAt = creationTime + timeLimit;
            RemainingTime = timeLimit;
        }

        /// <summary>
        /// Alias for backward compatibility — same as CreatedAt.
        /// </summary>
        public float CreationTime => CreatedAt;
    }
}
