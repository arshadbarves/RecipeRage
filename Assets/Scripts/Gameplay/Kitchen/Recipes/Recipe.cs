using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeRage.Gameplay.Kitchen.Recipes
{
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe Rage/Recipe")]
    public class Recipe : ScriptableObject
    {
        [Header("Recipe Info")]
        public int Id;
        public string RecipeName;
        [TextArea] public string Description;
        public Sprite Icon;
        public RecipeCategory Category;
        public int Difficulty;
        public int BasePoints;

        [Header("Cooking Settings")]
        public float BaseCookTime = 10f;
        public float BurnTime = 5f;
        public float OptimalQualityThreshold = 0.8f;

        [Header("Ingredients")]
        public List<IngredientRequirement> RequiredIngredients;
        public List<IngredientBonus> OptionalIngredients;

        [Header("Special Effects")]
        public List<CookingEffect> CookingEffects;
        public List<PowerUpEffect> PowerUpEffects;

        public bool ValidateIngredients(List<InventoryItem> ingredients)
        {
            // Check required ingredients
            foreach (var requirement in RequiredIngredients)
            {
                var matchingIngredients = ingredients.Where(i => 
                    i.ItemId == (int)requirement.Type &&
                    i.Quality >= requirement.MinQuality
                ).ToList();

                if (matchingIngredients.Count < requirement.Count)
                    return false;
            }

            return true;
        }

        public float CalculateQuality(List<InventoryItem> ingredients, float cookingProgress)
        {
            float quality = 1f;

            // Base quality from required ingredients
            foreach (var requirement in RequiredIngredients)
            {
                var matchingIngredients = ingredients.Where(i => 
                    i.ItemId == (int)requirement.Type
                ).OrderByDescending(i => i.Quality).Take(requirement.Count);

                float avgQuality = matchingIngredients.Average(i => i.Quality);
                quality *= avgQuality;
            }

            // Bonus from optional ingredients
            foreach (var bonus in OptionalIngredients)
            {
                var matchingIngredients = ingredients.Where(i => 
                    i.ItemId == (int)bonus.Type &&
                    i.Quality >= bonus.MinQuality
                );

                if (matchingIngredients.Any())
                {
                    quality *= 1f + bonus.QualityBonus;
                }
            }

            // Cooking progress impact
            float progressImpact = Mathf.Abs(cookingProgress - OptimalQualityThreshold);
            quality *= 1f - progressImpact;

            return Mathf.Clamp01(quality);
        }

        public int CalculatePoints(float quality)
        {
            float multiplier = 1f;

            // Quality bonus
            if (quality >= OptimalQualityThreshold)
            {
                multiplier += (quality - OptimalQualityThreshold) * 2f;
            }

            // Difficulty bonus
            multiplier += Difficulty * 0.2f;

            return Mathf.RoundToInt(BasePoints * multiplier);
        }

        public List<PowerUpEffect> GetPowerUpEffects(float quality)
        {
            return PowerUpEffects.Where(e => quality >= e.MinQualityRequired).ToList();
        }
    }

    [Serializable]
    public struct IngredientRequirement
    {
        public IngredientType Type;
        public int Count;
        public float MinQuality;
    }

    [Serializable]
    public struct IngredientBonus
    {
        public IngredientType Type;
        public float MinQuality;
        public float QualityBonus;
        [TextArea] public string Description;
    }

    [Serializable]
    public struct CookingEffect
    {
        public ParticleSystem VisualEffect;
        public AudioClip SoundEffect;
        public float ProgressThreshold;
    }

    [Serializable]
    public struct PowerUpEffect
    {
        public PowerUpType Type;
        public float Duration;
        public float Strength;
        public float MinQualityRequired;
        [TextArea] public string Description;
    }

    public enum RecipeCategory
    {
        Starter,
        MainCourse,
        Dessert,
        Special
    }

    public enum PowerUpType
    {
        SpeedBoost,
        AttackBoost,
        DefenseBoost,
        CookingBoost,
        HealthRegen,
        StaminaRegen
    }
}
