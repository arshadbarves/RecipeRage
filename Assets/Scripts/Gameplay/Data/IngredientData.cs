using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Editor.Attributes;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "New Ingredient", menuName = "Recipe Rage/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [field: SerializeField, GenerateUniqueId] public string IngredientId { get; private set; }
        [field: SerializeField] public IngredientType Type { get; private set; }
        [field: SerializeField] public string IngredientName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public List<IngredientStateGameObjectPair> IngredientStates { get; private set; }
        [field: SerializeField] public int ChoppingTime { get; private set; }
        [field: SerializeField] public float CookTime { get; private set; }
        [field: SerializeField] public float BurnTime { get; private set; }
        
        public bool IsCookable => IngredientStates.Exists(pair => pair.State == IngredientState.Cooked);
        public bool IsChoppable => IngredientStates.Exists(pair => pair.State == IngredientState.Chopped);
    }

    public enum IngredientType
    {
        None,
        Tomato,
        Lettuce,
        Cheese,
        Meat,
        Bread
    }

    public enum IngredientState
    {
        Raw,
        Chopped,
        Cooked,
        Burned
    }

    [Serializable]
    public class IngredientStateGameObjectPair
    {
        [field: SerializeField] public IngredientState State { get; private set; }
        [field: SerializeField] public GameObject GameObject { get; private set; }
    }
}