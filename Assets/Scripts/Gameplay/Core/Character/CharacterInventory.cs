using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;

namespace RecipeRage.Gameplay.Core
{
    public class CharacterInventory : NetworkBehaviour
    {
        [SerializeField] private int maxItems = 4;

        private readonly NetworkList<InventoryItem> _items;

        public CharacterInventory()
        {
            _items = new NetworkList<InventoryItem>();
        }

        public bool TryAddItem(InventoryItem item)
        {
            if (!IsServer) return false;
            if (_items.Count >= maxItems) return false;

            _items.Add(item);
            return true;
        }

        public bool TryRemoveItem(InventoryItem item)
        {
            if (!IsServer) return false;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(item))
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool UpdateItem(InventoryItem oldItem, InventoryItem newItem)
        {
            if (!IsServer) return false;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(oldItem))
                {
                    _items[i] = newItem;
                    return true;
                }
            }
            return false;
        }

        public List<InventoryItem> GetItems()
        {
            return new List<InventoryItem>(_items);
        }

        public bool HasItem(ItemType type)
        {
            foreach (var item in _items)
            {
                if (item.Type == type)
                    return true;
            }
            return false;
        }

        public bool HasIngredient(IngredientType type)
        {
            foreach (var item in _items)
            {
                if (item.Type == ItemType.Ingredient && item.ItemId == (int)type)
                    return true;
            }
            return false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _items.Clear();
            }
        }
    }
}
