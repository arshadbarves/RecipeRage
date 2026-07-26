using Unity.Netcode;

namespace RecipeRage.Net
{
    public struct CarriedIngredientState : INetworkSerializable, System.IEquatable<CarriedIngredientState>
    {
        public int IngredientTypeIndex;
        public bool IsChopped;
        public bool IsCooked;
        public bool IsBurnt;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IngredientTypeIndex);
            serializer.SerializeValue(ref IsChopped);
            serializer.SerializeValue(ref IsCooked);
            serializer.SerializeValue(ref IsBurnt);
        }

        public bool Equals(CarriedIngredientState other)
        {
            return IngredientTypeIndex == other.IngredientTypeIndex
                && IsChopped == other.IsChopped
                && IsCooked == other.IsCooked
                && IsBurnt == other.IsBurnt;
        }
    }
}
