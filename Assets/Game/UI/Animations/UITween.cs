using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Smooth value-tween animation for UI Toolkit elements (opacity, scale,
    /// translate). Drives the premium splash reveal: black pause → title
    /// blur-fade → subtitle blur-fade (staggered). Self-creating singleton.
    /// </summary>
    public sealed class UITween : MonoBehaviour
    {
        private static UITween _instance;

        public static UITween Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("UITween");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<UITween>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        public static Coroutine Animate(float duration, float delay, Func<float, float> ease, Action<float> apply, Action onComplete = null)
        {
            return Instance.StartCoroutine(Run(duration, delay, ease, apply, onComplete));
        }

        private static IEnumerator Run(float duration, float delay, Func<float, float> ease, Action<float> apply, Action onComplete)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                apply(ease(t));
                yield return null;
            }
            apply(ease(1f));
            onComplete?.Invoke();
        }

        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    }
}
