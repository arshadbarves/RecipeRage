using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecipeRage.Gameplay.Cooking
{
    /// <summary>
    /// ScriptableObject containing recipe data, which combines multiple ingredients with specific cooking steps
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "RecipeRage/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string recipeId;
        public string displayName;
        public Sprite icon;
        public string description;

        [Header("Recipe Properties")]
        public List<RecipeStep> steps;
        public float baseValue;
        public float timeLimit;

        [Header("Visual Settings")]
        public GameObject finalDishPrefab;
        public GameObject platedPrefab;
    }

    /// <summary>
    /// Represents a single step in a recipe, combining an ingredient with its required cooking method
    /// </summary>
    [Serializable]
    public class RecipeStep
    {
        [Header("Ingredient")]
        public IngredientData ingredient;
        public int quantity = 1;

        [Header("Cooking Requirements")]
        public CookingMethod requiredMethod;
        public CookingState requiredState;

        [Header("Step Properties")]
        public string stepDescription;
        public bool isOptional;
        public float stepOrder; // For parallel cooking steps, same order number means can be done in parallel
    }

    /// <summary>
    /// Manages the state and progress of a recipe being cooked
    /// </summary>
    public class RecipeProgress
    {

        public RecipeProgress(RecipeData recipe)
        {
            Recipe = recipe;
            TimeRemaining = recipe.timeLimit;
            StepProgress = new Dictionary<RecipeStep, StepProgress>();

            foreach (var step in recipe.steps)
            {
                StepProgress[step] = new StepProgress();
            }
        }
        public RecipeData Recipe { get; private set; }
        public Dictionary<RecipeStep, StepProgress> StepProgress { get; }
        public float TimeRemaining { get; set; }
        public bool IsCompleted => StepProgress.Values.All(step => step.IsCompleted);
    }

    /// <summary>
    /// Tracks the progress of a single recipe step
    /// </summary>
    public class StepProgress
    {
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public float Progress { get; set; }
        public CookingState CurrentState { get; set; }
    }
}