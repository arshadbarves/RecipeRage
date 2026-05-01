using System;

namespace KitchenClash.Domain
{
    public struct RecipeOrderState : IEquatable<RecipeOrderState>
    {
        public int OrderId;
        public int RecipeId;
        public float CreationTime;
        public float TimeLimit;
        public float RemainingTime;
        public bool IsCompleted;
        public bool IsExpired;
        public int PointValue;
        public int CompletedByTeamId;

        public bool Equals(RecipeOrderState other)
        {
            return OrderId == other.OrderId
                && RecipeId == other.RecipeId
                && IsCompleted == other.IsCompleted
                && IsExpired == other.IsExpired;
        }

        public override bool Equals(object obj) => obj is RecipeOrderState other && Equals(other);
        public override int GetHashCode() => OrderId.GetHashCode();

        public static bool operator ==(RecipeOrderState left, RecipeOrderState right) => left.Equals(right);
        public static bool operator !=(RecipeOrderState left, RecipeOrderState right) => !left.Equals(right);
    }
}
