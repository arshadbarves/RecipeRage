using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Ability.Components
{
    [CreateAssetMenu(fileName = "AbilityContainer", menuName = "RecipeRage/Abilities/Container")]
    public class AbilityContainer : ScriptableObject
    {
        public List<AbilityData> abilities = new List<AbilityData>();

        [Serializable]
        public class AbilityData
        {
            public string abilityName;
            public BaseAbility abilityPrefab;
            public Sprite icon;
            public string description;
            public int unlockLevel;
            public int resourceCost;
        }
    }
}