using System.Collections.Generic;
using UnityEngine;
using Utilities.Editor.Attributes;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe Rage/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [field: SerializeField, GenerateUniqueId] public string RecipeId { get; private set; }
        [field: SerializeField] public string RecipeName { get; private set; }
        [field: SerializeField] public Sprite RecipeIcon { get; private set; }
        [field: SerializeField] public GameObject CompleteDishPrefab { get; private set; }
        [field: SerializeField] public float PreparationTime { get; private set; }
        [field: SerializeField] public float CookingTime { get; private set; }
        [field: SerializeField] public int Reward { get; private set; }
        [field: SerializeField] public List<IngredientRequirement> Ingredients { get; private set; }

        [System.Serializable]
        public class IngredientRequirement
        {
            [field: SerializeField] public IngredientData Ingredient { get; private set; }
            [field: SerializeField] public IngredientState RequiredState { get; private set; }
            [field: SerializeField] public int Quantity { get; private set; }
        }
    }
}