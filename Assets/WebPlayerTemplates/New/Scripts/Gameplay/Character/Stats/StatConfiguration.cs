using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Character.Stats
{
    [CreateAssetMenu(fileName = "StatConfiguration", menuName = "Game/Stats/Stat Configuration")]
    public class StatConfiguration : ScriptableObject
    {

        [SerializeField] private StatDefinition[] statDefinitions;

        private Dictionary<StatType, StatDefinition> _statLookup;

        public void Initialize()
        {
            _statLookup = new Dictionary<StatType, StatDefinition>();
            foreach (StatDefinition def in statDefinitions)
            {
                _statLookup[def.type] = def;
            }
        }

        public StatDefinition GetStatDefinition(StatType type)
        {
            return _statLookup.GetValueOrDefault(type);
        }
        [Serializable]
        public class StatDefinition
        {
            public StatType type;
            public float baseValue;
            public float minValue;
            public float maxValue;
            public bool isPercentage;
        }
    }
}