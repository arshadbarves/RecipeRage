using UnityEngine;

namespace Core.UI.Animation
{
    /// <summary>
    /// Collection of easing functions for UI animations.
    /// </summary>
    public static class UIEasing
    {
        /// <summary>
        /// Linear easing (no easing).
        /// </summary>
        public static float Linear(float t) => t;
        
        /// <summary>
        /// Ease in quadratic.
        /// </summary>
        public static float EaseInQuad(float t) => t * t;
        
        /// <summary>
        /// Ease out quadratic.
        /// </summary>
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        
        /// <summary>
        /// Ease in-out quadratic.
        /// </summary>
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }
        
        /// <summary>
        /// Ease in cubic.
        /// </summary>
        public static float EaseInCubic(float t) => t * t * t;
        
        /// <summary>
        /// Ease out cubic.
        /// </summary>
        public static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        
        /// <summary>
        /// Ease in-out cubic.
        /// </summary>
        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        }
        
        /// <summary>
        /// Ease in quartic.
        /// </summary>
        public static float EaseInQuart(float t) => t * t * t * t;
        
        /// <summary>
        /// Ease out quartic.
        /// </summary>
        public static float EaseOutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        
        /// <summary>
        /// Ease in-out quartic.
        /// </summary>
        public static float EaseInOutQuart(float t)
        {
            return t < 0.5f ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
        }
        
        /// <summary>
        /// Ease in quintic.
        /// </summary>
        public static float EaseInQuint(float t) => t * t * t * t * t;
        
        /// <summary>
        /// Ease out quintic.
        /// </summary>
        public static float EaseOutQuint(float t) => 1 - Mathf.Pow(1 - t, 5);
        
        /// <summary>
        /// Ease in-out quintic.
        /// </summary>
        public static float EaseInOutQuint(float t)
        {
            return t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;
        }
        
        /// <summary>
        /// Ease in sine.
        /// </summary>
        public static float EaseInSine(float t) => 1 - Mathf.Cos((t * Mathf.PI) / 2);
        
        /// <summary>
        /// Ease out sine.
        /// </summary>
        public static float EaseOutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2);
        
        /// <summary>
        /// Ease in-out sine.
        /// </summary>
        public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        
        /// <summary>
        /// Ease in exponential.
        /// </summary>
        public static float EaseInExpo(float t) => t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
        
        /// <summary>
        /// Ease out exponential.
        /// </summary>
        public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
        
        /// <summary>
        /// Ease in-out exponential.
        /// </summary>
        public static float EaseInOutExpo(float t)
        {
            return t == 0 ? 0 : t == 1 ? 1 : t < 0.5 ? 
                Mathf.Pow(2, 20 * t - 10) / 2 : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;
        }
        
        /// <summary>
        /// Ease in circular.
        /// </summary>
        public static float EaseInCirc(float t) => 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        
        /// <summary>
        /// Ease out circular.
        /// </summary>
        public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        
        /// <summary>
        /// Ease in-out circular.
        /// </summary>
        public static float EaseInOutCirc(float t)
        {
            return t < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2 : 
                (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;
        }
        
        /// <summary>
        /// Ease in elastic.
        /// </summary>
        public static float EaseInElastic(float t)
        {
            const float c4 = (2 * Mathf.PI) / 3;
            
            return t == 0 ? 0 : t == 1 ? 1 : 
                -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);
        }
        
        /// <summary>
        /// Ease out elastic.
        /// </summary>
        public static float EaseOutElastic(float t)
        {
            const float c4 = (2 * Mathf.PI) / 3;
            
            return t == 0 ? 0 : t == 1 ? 1 : 
                Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
        }
        
        /// <summary>
        /// Ease in-out elastic.
        /// </summary>
        public static float EaseInOutElastic(float t)
        {
            const float c5 = (2 * Mathf.PI) / 4.5f;
            
            return t == 0 ? 0 : t == 1 ? 1 : t < 0.5 ? 
                -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 : 
                (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 + 1;
        }
        
        /// <summary>
        /// Ease in back.
        /// </summary>
        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            
            return c3 * t * t * t - c1 * t * t;
        }
        
        /// <summary>
        /// Ease out back.
        /// </summary>
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
        
        /// <summary>
        /// Ease in-out back.
        /// </summary>
        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            
            return t < 0.5 ? 
                (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2 : 
                (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        }
        
        /// <summary>
        /// Ease in bounce.
        /// </summary>
        public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);
        
        /// <summary>
        /// Ease out bounce.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            
            if (t < 1 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5 / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
        
        /// <summary>
        /// Ease in-out bounce.
        /// </summary>
        public static float EaseInOutBounce(float t)
        {
            return t < 0.5 ? 
                (1 - EaseOutBounce(1 - 2 * t)) / 2 : 
                (1 + EaseOutBounce(2 * t - 1)) / 2;
        }
    }
}
