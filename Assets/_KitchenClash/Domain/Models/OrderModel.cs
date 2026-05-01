using System;

namespace KitchenClash.Domain
{
    public sealed class OrderModel
    {
        public Guid Id { get; }
        public int RecipeId { get; }
        public int Tier { get; }
        public float TimeLimit { get; }
        public float CreationTime { get; }
        public float RemainingTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsExpired { get; set; }
        public int PointValue { get; set; }

        public OrderModel(Guid id, int recipeId, int tier, float timeLimit, float creationTime)
        {
            Id = id;
            RecipeId = recipeId;
            Tier = tier;
            TimeLimit = timeLimit;
            CreationTime = creationTime;
            RemainingTime = timeLimit;
        }
    }
}
