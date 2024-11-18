using System;
using System.Collections.Generic;
using Gameplay.Character.Stats;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "CharacterStatConfiguration", menuName = "Game/Stats/Character Stat Configuration")]
    public class StatConfigurationData : ScriptableObject
    {

        public CharacterClassStats[] characterClasses;

        public Dictionary<string, StatDefinition[]> GetStatsLookup()
        {
            Dictionary<string, StatDefinition[]> lookup = new Dictionary<string, StatDefinition[]>();
            foreach (CharacterClassStats classStats in characterClasses)
            {
                lookup[classStats.className] = classStats.stats;
            }
            return lookup;
        }
        [Serializable]
        public class CharacterClassStats
        {
            public string className;
            public StatDefinition[] stats;
        }

        [Serializable]
        public class StatDefinition
        {
            public StatType type;
            public float baseValue;
            public float minValue;
            public float maxValue;
            public bool isPercentage;

            [TextArea(2, 3)]
            public string description;
        }
    }
}