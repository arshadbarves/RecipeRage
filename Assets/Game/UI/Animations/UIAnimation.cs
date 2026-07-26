using System;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Premium UI motion using USS transitions + IVisualElementScheduledItem.
    /// Flat colors and transforms only — no gradients, no shaders.
    /// Usage: add the transition USS classes once (ui-animations.uss), then call.
    /// </summary>
    public static class UIAnimation
    {
        public const string TransitionClass = "ui-transition";
        public const string HiddenClass = "ui-hidden";
        public const string ScaleDownClass = "ui-scale-down";

        public static void FadeIn(VisualElement element, float durationSec = 0.3f, Action onComplete = null)
        {
            element.AddToClassList(TransitionClass);
            element.RemoveFromClassList(HiddenClass);
            element.style.opacity = 0f;
            element.schedule.Execute(() => element.style.opacity = 1f).StartingIn(10);
            if (onComplete != null)
            {
                element.schedule.Execute(onComplete).StartingIn((long)(durationSec * 1000));
            }
        }

        public static void FadeOut(VisualElement element, float durationSec = 0.3f, Action onComplete = null)
        {
            element.AddToClassList(TransitionClass);
            element.style.opacity = 0f;
            element.schedule.Execute(() =>
            {
                element.AddToClassList(HiddenClass);
                onComplete?.Invoke();
            }).StartingIn((long)(durationSec * 1000));
        }

        public static void ScaleBounce(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.scale = new StyleScale(new Scale(new UnityEngine.Vector2(0.5f, 0.5f)));
            element.schedule.Execute(() =>
                element.style.scale = new StyleScale(new Scale(UnityEngine.Vector2.one))).StartingIn(10);
        }

        public static void ScalePulse(VisualElement element, float periodSec = 1f)
        {
            var up = true;
            element.schedule.Execute(() =>
            {
                element.style.scale = new StyleScale(new Scale(up
                    ? new UnityEngine.Vector2(1.05f, 1.05f)
                    : UnityEngine.Vector2.one));
                up = !up;
            }).Every((long)(periodSec * 500));
        }

        public static void SlideInFromRight(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.translate = new StyleTranslate(new Translate(new Length(100, LengthUnit.Percent), 0));
            element.schedule.Execute(() =>
                element.style.translate = new StyleTranslate(new Translate(0, 0))).StartingIn(10);
        }

        public static void SlideInFromBottom(VisualElement element, float durationSec = 0.3f)
        {
            element.AddToClassList(TransitionClass);
            element.style.translate = new StyleTranslate(new Translate(0, new Length(100, LengthUnit.Percent)));
            element.schedule.Execute(() =>
                element.style.translate = new StyleTranslate(new Translate(0, 0))).StartingIn(10);
        }

        public static void StaggerChildren(VisualElement parent, float delaySec = 0.1f)
        {
            var index = 0;
            foreach (var child in parent.Children())
            {
                var captured = child;
                captured.style.opacity = 0f;
                parent.schedule.Execute(() => FadeIn(captured, 0.25f)).StartingIn((long)(index * delaySec * 1000));
                index++;
            }
        }

        public static void CountUp(Label label, int from, int to, float durationSec = 0.5f)
        {
            var elapsed = 0f;
            var stepMs = 33L;
            label.schedule.Execute(() =>
            {
                elapsed += stepMs / 1000f;
                var t = UnityEngine.Mathf.Clamp01(elapsed / durationSec);
                label.text = ((int)UnityEngine.Mathf.Lerp(from, to, t)).ToString();
            }).Every(stepMs).Until(() => elapsed >= durationSec);
        }
    }
}
