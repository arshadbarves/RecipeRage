using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// Represents a recipe that can be prepared in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "RecipeRage/Cooking/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Header("Basic Properties")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _pointValue = 100;
        [SerializeField] private float _baseTimeLimit = 60f;
        
        [Header("Ingredients")]
        [SerializeField] private List<RecipeIngredient> _ingredients = new List<RecipeIngredient>();
        
        [Header("Difficulty")]
        [SerializeField] private RecipeDifficulty _difficulty = RecipeDifficulty.Easy;
        
        /// <summary>
        /// The unique ID of this recipe.
        /// </summary>
        public int Id => _id;
        [SerializeField] private int _id;
        
        /// <summary>
        /// The display name of the recipe.
        /// </summary>
        public string DisplayName => _displayName;
        
        /// <summary>
        /// The icon representing the recipe.
        /// </summary>
        public Sprite Icon => _icon;
        
        /// <summary>
        /// The point value awarded for completing this recipe.
        /// </summary>
        public int PointValue => _pointValue;
        
        /// <summary>
        /// The base time limit for completing this recipe.
        /// </summary>
        public float BaseTimeLimit => _baseTimeLimit;
        
        /// <summary>
        /// The ingredients required for this recipe.
        /// </summary>
        public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients;
        
        /// <summary>
        /// The difficulty level of this recipe.
        /// </summary>
        public RecipeDifficulty Difficulty => _difficulty;
        
        /// <summary>
        /// Validates the recipe data.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_displayName))
            {
                _displayName = name;
            }
            
            if (_id == 0)
            {
                // Generate a simple hash code for the recipe name
                _id = name.GetHashCode();
            }
        }
    }
    
    /// <summary>
    /// Represents an ingredient in a recipe with specific preparation requirements.
    /// </summary>
    [Serializable]
    public class RecipeIngredient
    {
        [SerializeField] private Ingredient _ingredient;
        [SerializeField] private bool _requireCut = false;
        [SerializeField] private bool _requireCooked = false;
        
        /// <summary>
        /// The ingredient.
        /// </summary>
        public Ingredient Ingredient => _ingredient;
        
        /// <summary>
        /// Whether the ingredient needs to be cut.
        /// </summary>
        public bool RequireCut => _requireCut;
        
        /// <summary>
        /// Whether the ingredient needs to be cooked.
        /// </summary>
        public bool RequireCooked => _requireCooked;
    }
    
    /// <summary>
    /// Difficulty levels for recipes.
    /// </summary>
    public enum RecipeDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    /// <summary>
    /// Represents the state of a recipe order.
    /// </summary>
    public struct RecipeOrderState : INetworkSerializable, IEquatable<RecipeOrderState>
    {
        /// <summary>
        /// The ID of the recipe.
        /// </summary>
        public int RecipeId;
        
        /// <summary>
        /// The unique ID of this order.
        /// </summary>
        public int OrderId;
        
        /// <summary>
        /// The time when the order was created.
        /// </summary>
        public float CreationTime;
        
        /// <summary>
        /// The time limit for this order.
        /// </summary>
        public float TimeLimit;
        
        /// <summary>
        /// Whether the order has been completed.
        /// </summary>
        public bool IsCompleted;
        
        /// <summary>
        /// Whether the order has expired.
        /// </summary>
        public bool IsExpired;
        
        /// <summary>
        /// The point value for this order.
        /// </summary>
        public int PointValue;
        
        /// <summary>
        /// Serialize the recipe order state.
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref RecipeId);
            serializer.SerializeValue(ref OrderId);
            serializer.SerializeValue(ref CreationTime);
            serializer.SerializeValue(ref TimeLimit);
            serializer.SerializeValue(ref IsCompleted);
            serializer.SerializeValue(ref IsExpired);
            serializer.SerializeValue(ref PointValue);
        }
        
        /// <summary>
        /// Check if this recipe order state equals another.
        /// </summary>
        public bool Equals(RecipeOrderState other)
        {
            return RecipeId == other.RecipeId &&
                   OrderId == other.OrderId &&
                   Math.Abs(CreationTime - other.CreationTime) < 0.001f &&
                   Math.Abs(TimeLimit - other.TimeLimit) < 0.001f &&
                   IsCompleted == other.IsCompleted &&
                   IsExpired == other.IsExpired &&
                   PointValue == other.PointValue;
        }
    }
}
