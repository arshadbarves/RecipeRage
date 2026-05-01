using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain.Enums;
using UnityEngine;

namespace KitchenClash.Application.Services
{
    public class StandardDishValidator
    {
        private const int PERFECT_SCORE_MULTIPLIER = 2;
        private const int GOOD_SCORE_MULTIPLIER = 1;
        private const float ACCEPTABLE_SCORE_MULTIPLIER = 0.5f;
        private const int TIME_BONUS_PER_SECOND = 2;

        public bool ValidateDish(List<object> ingredients, object recipe)
        {
            return ingredients != null && recipe != null && ingredients.Count > 0;
        }

        public DishQuality GetDishQuality(List<object> ingredients, object recipe)
        {
            if (!ValidateDish(ingredients, recipe)) return DishQuality.Wrong;
            return DishQuality.Good;
        }

        public int CalculateScore(List<object> ingredients, object recipe, float timeRemaining)
        {
            var quality = GetDishQuality(ingredients, recipe);
            if (quality == DishQuality.Wrong) return 0;

            int score = 100;
            switch (quality)
            {
                case DishQuality.Perfect: score *= PERFECT_SCORE_MULTIPLIER; break;
                case DishQuality.Good: score *= GOOD_SCORE_MULTIPLIER; break;
                case DishQuality.Acceptable: score = Mathf.RoundToInt(score * ACCEPTABLE_SCORE_MULTIPLIER); break;
            }

            score += Mathf.RoundToInt(timeRemaining * TIME_BONUS_PER_SECOND);
            return Mathf.Max(score, 0);
        }
    }
}
