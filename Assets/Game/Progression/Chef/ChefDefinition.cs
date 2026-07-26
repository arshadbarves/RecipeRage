using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Chef", menuName = "RecipeRage/Chef Definition")]
    public sealed class ChefDefinition : ScriptableObject
    {
        [SerializeField] private ChefId _id;
        [SerializeField] private string _displayName;
        [SerializeField] private ChefRarity _rarity;
        [SerializeField] private int _unlockCost;
        [SerializeField] private ChefAbilityType _abilityType;
        [Tooltip("Ability value per level (index 0 = level 1). Interpretation depends on ability type.")]
        [SerializeField] private float[] _abilityPerLevel = new float[10];
        [SerializeField] private Sprite _portrait;
        [SerializeField] private GameObject _modelPrefab;

        public ChefId Id => _id;
        public string DisplayName => _displayName;
        public ChefRarity Rarity => _rarity;
        public int UnlockCost => _unlockCost;
        public ChefAbilityType AbilityType => _abilityType;
        public float[] AbilityPerLevel => _abilityPerLevel;
        public Sprite Portrait => _portrait;
        public GameObject ModelPrefab => _modelPrefab;
    }
}
