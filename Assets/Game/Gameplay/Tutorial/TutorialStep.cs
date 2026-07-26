using System;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>Gameplay-facing contract for the tutorial HUD (implemented in RecipeRage.UI).</summary>
    public interface ITutorialHUD
    {
        void ShowStep(int index, int total, TutorialStep step);
        void SetProgress(float progress01, string label);
    }

    [Serializable]
    public sealed class TutorialStep
    {
        [Tooltip("Main instruction shown in the tutorial panel")]
        public string Instruction;

        [Tooltip("Button hint shown below the instruction (e.g. 'Press LEFT-CLICK to Fetch')")]
        public string ButtonHint;

        [Tooltip("Station highlight color (hex, e.g. #EC4899)")]
        public string StationColorHex = "#EC4899";

        [Tooltip("Station to highlight/arrow-point (Transform)")]
        public Transform HighlightTarget;

        [Tooltip("What completes this step")]
        public TutorialCondition Condition;

        [Tooltip("Optional: track numeric progress (e.g. chop taps 0/8)")]
        public bool TrackProgress;
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
