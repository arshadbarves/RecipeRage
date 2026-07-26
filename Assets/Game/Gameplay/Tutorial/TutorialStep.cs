using System;
using UnityEngine;

namespace RecipeRage
{
    [Serializable]
    public sealed class TutorialStep
    {
        public string Instruction;
        public Transform HighlightTarget;
        public TutorialCondition Condition;
    }

    public enum TutorialCondition
    {
        MovedDistance,
        FetchedIngredient,
        ChoppedIngredient,
        CookingStarted,
        CookingCollected,
        PlateTaken,
        IngredientPlated,
        RecipeServed,
        BurnWarningShown
    }
}
