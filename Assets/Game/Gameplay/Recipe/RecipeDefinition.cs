using System;
using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "RecipeRage/Recipe Definition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private RecipeTier _tier = RecipeTier.Easy;
        [SerializeField] private Sprite _icon;
        [SerializeField] private IngredientRequirement[] _requiredIngredients = Array.Empty<IngredientRequirement>();

        public string Id => _id;
        public string DisplayName => _displayName;
        public RecipeTier Tier => _tier;
        public Sprite Icon => _icon;
        public IngredientRequirement[] RequiredIngredients => _requiredIngredients;
    }

    [Serializable]
    public sealed class IngredientRequirement
    {
        [SerializeField] private IngredientType _type;
        [SerializeField] private bool _requiresChopped = true;
        [SerializeField] private bool _requiresCooked = true;

        public IngredientType Type => _type;
        public bool RequiresChopped => _requiresChopped;
        public bool RequiresCooked => _requiresCooked;
    }
}
