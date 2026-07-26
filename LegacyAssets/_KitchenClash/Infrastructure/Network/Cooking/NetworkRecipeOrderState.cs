using System;
using Unity.Netcode;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Network-serializable rich order data for use in NetworkList.
    /// Contains all order metadata needed by OrderManager, ScoreManager, and ServingStation.
    /// </summary>
    public struct NetworkRecipeOrderState : IEquatable<NetworkRecipeOrderState>, INetworkSerializable
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
        public int RecipeTier;
        public float SpeedRatio;
        public bool RhythmBonus;
        public int ComboCount;

        public bool Equals(NetworkRecipeOrderState other)
        {
            return OrderId == other.OrderId
                && RecipeId == other.RecipeId
                && IsCompleted == other.IsCompleted
                && IsExpired == other.IsExpired
                && CompletedByTeamId == other.CompletedByTeamId;
        }

        public override bool Equals(object obj) => obj is NetworkRecipeOrderState other && Equals(other);
        public override int GetHashCode() => OrderId.GetHashCode();

        public static bool operator ==(NetworkRecipeOrderState left, NetworkRecipeOrderState right) => left.Equals(right);
        public static bool operator !=(NetworkRecipeOrderState left, NetworkRecipeOrderState right) => !left.Equals(right);

        public static implicit operator RecipeOrderState(NetworkRecipeOrderState n) => new RecipeOrderState
        {
            OrderId = n.OrderId,
            RecipeId = n.RecipeId,
            CreationTime = n.CreationTime,
            TimeLimit = n.TimeLimit,
            RemainingTime = n.RemainingTime,
            IsCompleted = n.IsCompleted,
            IsExpired = n.IsExpired,
            PointValue = n.PointValue,
            CompletedByTeamId = n.CompletedByTeamId,
            RecipeTier = n.RecipeTier,
            SpeedRatio = n.SpeedRatio,
            RhythmBonus = n.RhythmBonus,
            ComboCount = n.ComboCount
        };

        public static implicit operator NetworkRecipeOrderState(RecipeOrderState r) => new NetworkRecipeOrderState
        {
            OrderId = r.OrderId,
            RecipeId = r.RecipeId,
            CreationTime = r.CreationTime,
            TimeLimit = r.TimeLimit,
            RemainingTime = r.RemainingTime,
            IsCompleted = r.IsCompleted,
            IsExpired = r.IsExpired,
            PointValue = r.PointValue,
            CompletedByTeamId = r.CompletedByTeamId,
            RecipeTier = r.RecipeTier,
            SpeedRatio = r.SpeedRatio,
            RhythmBonus = r.RhythmBonus,
            ComboCount = r.ComboCount
        };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref OrderId);
            serializer.SerializeValue(ref RecipeId);
            serializer.SerializeValue(ref CreationTime);
            serializer.SerializeValue(ref TimeLimit);
            serializer.SerializeValue(ref RemainingTime);
            serializer.SerializeValue(ref IsCompleted);
            serializer.SerializeValue(ref IsExpired);
            serializer.SerializeValue(ref PointValue);
            serializer.SerializeValue(ref CompletedByTeamId);
            serializer.SerializeValue(ref RecipeTier);
            serializer.SerializeValue(ref SpeedRatio);
            serializer.SerializeValue(ref RhythmBonus);
            serializer.SerializeValue(ref ComboCount);
        }
    }
}
