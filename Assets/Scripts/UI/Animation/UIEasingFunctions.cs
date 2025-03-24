using System;
using UnityEngine;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Collection of common easing functions for animations.
    /// Each function takes a value between 0-1 and returns a transformed value.
    /// </summary>
    public static class UIEasingFunctions
    {
        // Linear (no easing)
        public static readonly Func<float, float> Linear = t => t;

        // Quadratic easing
        public static readonly Func<float, float> EaseInQuad = t => t * t;
        public static readonly Func<float, float> EaseOutQuad = t => 1 - (1 - t) * (1 - t);
        public static readonly Func<float, float> EaseInOutQuad = t =>
            t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

        // Cubic easing
        public static readonly Func<float, float> EaseInCubic = t => t * t * t;
        public static readonly Func<float, float> EaseOutCubic = t => 1 - Mathf.Pow(1 - t, 3);
        public static readonly Func<float, float> EaseInOutCubic = t =>
            t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;

        // Elastic easing
        public static readonly Func<float, float> EaseInElastic = t =>
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * ((2 * Mathf.PI) / 3));
        };

        public static readonly Func<float, float> EaseOutElastic = t =>
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * ((2 * Mathf.PI) / 3)) + 1;
        };

        public static readonly Func<float, float> EaseInOutElastic = t =>
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            if (t < 0.5f)
                return -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * ((2 * Mathf.PI) / 4.5f))) / 2;
            return (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * ((2 * Mathf.PI) / 4.5f))) / 2 + 1;
        };

        // Bounce easing
        public static readonly Func<float, float> EaseOutBounce = t =>
        {
            if (t < 1 / 2.75f)
                return 7.5625f * t * t;
            else if (t < 2 / 2.75f)
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            else if (t < 2.5 / 2.75)
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            else
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        };

        public static readonly Func<float, float> EaseInBounce = t => 1 - EaseOutBounce(1 - t);

        public static readonly Func<float, float> EaseInOutBounce = t =>
            t < 0.5f ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;

        // Back easing
        public static readonly Func<float, float> EaseInBack = t =>
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return c3 * t * t * t - c1 * t * t;
        };

        public static readonly Func<float, float> EaseOutBack = t =>
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        };

        public static readonly Func<float, float> EaseInOutBack = t =>
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5
                ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        };

        // Sine easing
        public static readonly Func<float, float> EaseInSine = t => 1 - Mathf.Cos((t * Mathf.PI) / 2);
        public static readonly Func<float, float> EaseOutSine = t => Mathf.Sin((t * Mathf.PI) / 2);
        public static readonly Func<float, float> EaseInOutSine = t => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }
}