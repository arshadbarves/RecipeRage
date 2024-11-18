using System;
using Unity.Netcode;

namespace RecipeRage.Gameplay.Core
{
    public enum ItemType
    {
        None,
        Ingredient,
        Plate,
        Dish,
        Tool
    }

    public enum IngredientType
    {
        None,
        Potato,
        Fish,
        Egg,
        Tomato,
        Onion,
        Mixed,
        Chopped,
        Cooked
    }

    public struct InventoryItem : INetworkSerializable, IEquatable<InventoryItem>
    {
        public int ItemId;
        public ItemType Type;
        public float Quality;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ItemId);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Quality);
        }

        public bool Equals(InventoryItem other)
        {
            return ItemId == other.ItemId && Type == other.Type && Quality == other.Quality;
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryItem item && Equals(item);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemId, Type, Quality);
        }
    }
}
