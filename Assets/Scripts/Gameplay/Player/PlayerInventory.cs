using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Player
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxSlots = 4;

        private readonly NetworkList<InventoryItem> _items;
        private readonly Dictionary<InventoryItem, int> _itemCounts = new();

        public PlayerInventory()
        {
            _items = new NetworkList<InventoryItem>();
        }

        public bool TryAddItem(InventoryItem item)
        {
            if (!IsServer) return false;
            if (_items.Count >= maxSlots) return false;

            _items.Add(item);
            if (_itemCounts.ContainsKey(item))
                _itemCounts[item]++;
            else
                _itemCounts[item] = 1;

            OnInventoryUpdatedClientRpc();
            return true;
        }

        public bool TryRemoveItem(InventoryItem item)
        {
            if (!IsServer) return false;
            if (!_itemCounts.ContainsKey(item) || _itemCounts[item] <= 0) return false;

            _items.Remove(item);
            _itemCounts[item]--;
            if (_itemCounts[item] <= 0)
                _itemCounts.Remove(item);

            OnInventoryUpdatedClientRpc();
            return true;
        }

        public bool HasItem(InventoryItem item, int count = 1)
        {
            return _itemCounts.ContainsKey(item) && _itemCounts[item] >= count;
        }

        [ClientRpc]
        private void OnInventoryUpdatedClientRpc()
        {
            // Notify UI or other systems of inventory change
        }
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
            return ItemId == other.ItemId && Type == other.Type && Quality.Equals(other.Quality);
        }
    }

    public enum ItemType
    {
        Ingredient,
        Tool,
        PowerUp
    }
}
