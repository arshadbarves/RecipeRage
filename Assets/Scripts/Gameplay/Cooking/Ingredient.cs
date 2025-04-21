using System;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents an ingredient type in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "New Ingredient", menuName = "RecipeRage/Cooking/Ingredient")]
    public class Ingredient : ScriptableObject
    {
        [Header("Basic Properties")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Color _color = Color.white;
        
        [Header("Cooking Properties")]
        [SerializeField] private float _prepTime = 2f;
        [SerializeField] private bool _requiresCutting = false;
        [SerializeField] private bool _requiresCooking = false;
        
        /// <summary>
        /// The unique ID of this ingredient.
        /// </summary>
        public int Id => _id;
        [SerializeField] private int _id;
        
        /// <summary>
        /// The display name of the ingredient.
        /// </summary>
        public string DisplayName => _displayName;
        
        /// <summary>
        /// The icon representing the ingredient.
        /// </summary>
        public Sprite Icon => _icon;
        
        /// <summary>
        /// The prefab used to instantiate this ingredient.
        /// </summary>
        public GameObject Prefab => _prefab;
        
        /// <summary>
        /// The color associated with this ingredient.
        /// </summary>
        public Color Color => _color;
        
        /// <summary>
        /// The time it takes to prepare this ingredient.
        /// </summary>
        public float PrepTime => _prepTime;
        
        /// <summary>
        /// Whether this ingredient requires cutting.
        /// </summary>
        public bool RequiresCutting => _requiresCutting;
        
        /// <summary>
        /// Whether this ingredient requires cooking.
        /// </summary>
        public bool RequiresCooking => _requiresCooking;
        
        /// <summary>
        /// Validates the ingredient data.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_displayName))
            {
                _displayName = name;
            }
            
            if (_id == 0)
            {
                // Generate a simple hash code for the ingredient name
                _id = name.GetHashCode();
            }
        }
    }
    
    /// <summary>
    /// Represents the state of an ingredient instance.
    /// </summary>
    public struct IngredientState : INetworkSerializable, IEquatable<IngredientState>
    {
        /// <summary>
        /// The ID of the ingredient.
        /// </summary>
        public int IngredientId;
        
        /// <summary>
        /// Whether the ingredient has been cut.
        /// </summary>
        public bool IsCut;
        
        /// <summary>
        /// Whether the ingredient has been cooked.
        /// </summary>
        public bool IsCooked;
        
        /// <summary>
        /// Whether the ingredient has been burned.
        /// </summary>
        public bool IsBurned;
        
        /// <summary>
        /// The cooking progress (0-1).
        /// </summary>
        public float CookingProgress;
        
        /// <summary>
        /// The cutting progress (0-1).
        /// </summary>
        public float CuttingProgress;
        
        /// <summary>
        /// Serialize the ingredient state.
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IngredientId);
            serializer.SerializeValue(ref IsCut);
            serializer.SerializeValue(ref IsCooked);
            serializer.SerializeValue(ref IsBurned);
            serializer.SerializeValue(ref CookingProgress);
            serializer.SerializeValue(ref CuttingProgress);
        }
        
        /// <summary>
        /// Check if this ingredient state equals another.
        /// </summary>
        public bool Equals(IngredientState other)
        {
            return IngredientId == other.IngredientId &&
                   IsCut == other.IsCut &&
                   IsCooked == other.IsCooked &&
                   IsBurned == other.IsBurned &&
                   Math.Abs(CookingProgress - other.CookingProgress) < 0.001f &&
                   Math.Abs(CuttingProgress - other.CuttingProgress) < 0.001f;
        }
    }
}
