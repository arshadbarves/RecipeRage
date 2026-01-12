using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Economy.Data
{
    [Serializable]
    public class EconomyData : ISerializationCallbackReceiver
    {
        // Public Data (Dictionaries) - Runtime access
        public Dictionary<string, long> Balances = new Dictionary<string, long>();
        public HashSet<string> Inventory = new HashSet<string>();
        public Dictionary<string, string> Data = new Dictionary<string, string>();

        // Serialization Containers (Lists) - Unity Serializer access
        [SerializeField] private List<string> _balanceKeys = new List<string>();
        [SerializeField] private List<long> _balanceValues = new List<long>();

        [SerializeField] private List<string> _inventoryItems = new List<string>();

        [SerializeField] private List<string> _dataKeys = new List<string>();
        [SerializeField] private List<string> _dataValues = new List<string>();

        // Called BEFORE serialization: Flatten Dictionaries -> Lists
        public void OnBeforeSerialize()
        {
            _balanceKeys.Clear();
            _balanceValues.Clear();
            foreach (var kvp in Balances)
            {
                _balanceKeys.Add(kvp.Key);
                _balanceValues.Add(kvp.Value);
            }

            _inventoryItems.Clear();
            foreach (var item in Inventory)
            {
                _inventoryItems.Add(item);
            }

            _dataKeys.Clear();
            _dataValues.Clear();
            foreach (var kvp in Data)
            {
                _dataKeys.Add(kvp.Key);
                _dataValues.Add(kvp.Value);
            }
        }

        // Called AFTER deserialization: Reconstruct Lists -> Dictionaries
        public void OnAfterDeserialize()
        {
            Balances = new Dictionary<string, long>();
            for (int i = 0; i < Math.Min(_balanceKeys.Count, _balanceValues.Count); i++)
            {
                Balances[_balanceKeys[i]] = _balanceValues[i];
            }

            Inventory = new HashSet<string>();
            foreach (var item in _inventoryItems)
            {
                Inventory.Add(item);
            }

            Data = new Dictionary<string, string>();
            for (int i = 0; i < Math.Min(_dataKeys.Count, _dataValues.Count); i++)
            {
                Data[_dataKeys[i]] = _dataValues[i];
            }
        }
    }
}
