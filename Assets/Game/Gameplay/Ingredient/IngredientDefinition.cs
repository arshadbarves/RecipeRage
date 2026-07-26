using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Ingredient", menuName = "RecipeRage/Ingredient Definition")]
    public sealed class IngredientDefinition : ScriptableObject
    {
        [SerializeField] private IngredientType _type;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _requiresChopping = true;
        [SerializeField] private bool _requiresCooking = true;
        [SerializeField] private int _chopTaps = 8;
        [SerializeField] private float _cookSeconds = 12f;

        public IngredientType Type => _type;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public bool RequiresChopping => _requiresChopping;
        public bool RequiresCooking => _requiresCooking;
        public int ChopTaps => _chopTaps;
        public float CookSeconds => _cookSeconds;
    }
}
