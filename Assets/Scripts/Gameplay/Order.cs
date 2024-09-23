using Gameplay.Data;
using Unity.Netcode;

namespace Gameplay
{
    public class Order : INetworkSerializable
    {
        public string OrderId;
        public string RecipeId;
        public float TimeRemaining;
        public bool IsDelivered;
        public bool IsFailed;
        public bool IsExpired;
        
        public Order(string orderId, RecipeData recipeData)
        {
            OrderId = orderId;
            RecipeId = recipeData.RecipeId;
            TimeRemaining = recipeData.PreparationTime;
            IsDelivered = false;
            IsFailed = false;
            IsExpired = false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref OrderId);
            serializer.SerializeValue(ref RecipeId);
            serializer.SerializeValue(ref TimeRemaining);
            serializer.SerializeValue(ref IsDelivered);
            serializer.SerializeValue(ref IsFailed);
            serializer.SerializeValue(ref IsExpired);
        }
    }
}