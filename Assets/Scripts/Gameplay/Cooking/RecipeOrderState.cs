using System;
using Unity.Netcode;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents the state of a recipe order.
    /// </summary>
    public struct RecipeOrderState : INetworkSerializable, IEquatable<RecipeOrderState>
    {
        /// <summary>
        /// The unique ID of the order.
        /// </summary>
        public int OrderId;

        /// <summary>
        /// The ID of the recipe for this order.
        /// </summary>
        public int RecipeId;

        /// <summary>
        /// The time when the order was created.
        /// </summary>
        public float CreationTime;

        /// <summary>
        /// The time limit for this order.
        /// </summary>
        public float TimeLimit;

        /// <summary>
        /// The remaining time for this order.
        /// </summary>
        public float RemainingTime;

        /// <summary>
        /// Whether the order has been completed.
        /// </summary>
        public bool IsCompleted;

        /// <summary>
        /// Whether the order has expired.
        /// </summary>
        public bool IsExpired;

        /// <summary>
        /// The point value of the order.
        /// </summary>
        public int PointValue;

        /// <summary>
        /// The ID of the team that completed this order (or -1 if none/co-op).
        /// </summary>
        public int CompletedByTeamId;

        /// <summary>
        /// Serialize the order state to the network.
        /// </summary>
        /// <param name="serializer">The network serializer</param>
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
        }

        /// <summary>
        /// Check if this order state is equal to another.
        /// </summary>
        /// <param name="other">The other order state</param>
        /// <returns>True if the order states are equal</returns>
        public bool Equals(RecipeOrderState other)
        {
            return OrderId == other.OrderId &&
                   RecipeId == other.RecipeId &&
                   CreationTime == other.CreationTime &&
                   TimeLimit == other.TimeLimit &&
                   RemainingTime == other.RemainingTime &&
                   IsCompleted == other.IsCompleted &&
                   IsExpired == other.IsExpired &&
                   PointValue == other.PointValue &&
                   CompletedByTeamId == other.CompletedByTeamId;
        }

        /// <summary>
        /// Check if this order state is equal to another object.
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return obj is RecipeOrderState other && Equals(other);
        }

        /// <summary>
        /// Get the hash code for this order state.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(OrderId, RecipeId, CreationTime, TimeLimit, RemainingTime, IsCompleted, IsExpired, PointValue);
        }
    }
}
