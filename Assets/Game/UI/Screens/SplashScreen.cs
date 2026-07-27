using Playcenter.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Splash screen with premium blur-reveal animation (matches the HTML reference):
    /// black pause (0.4s) → title fades/scales/translates in (1.8s) → subtitle
    /// fades in staggered (2s). Uses UITween for smooth value animation.
    /// </summary>
    [UIScreen]
    public sealed class SplashScreen : BaseUIScreen
    {
        private const float BlackPauseSec = 0.4f;
        private const float TitleDurationSec = 1.8f;
        private const float TitleDelaySec = 0.2f;
        private const float SubtitleDurationSec = 2f;
        private const float SubtitleDelaySec = 0.6f;

        protected override void OnShow()
        {
            var title = Root.Q<VisualElement>("brand-title");
            var subtitle = Root.Q<Label>("brand-subtitle");

            // Start invisible (off-state before animation)
            SetState(title, opacity: 0f, scale: 0.97f, translateY: 8f);
            SetState(subtitle, opacity: 0f, scale: 0.98f, translateY: 4f);

            // Title: blur-fade in (opacity + scale 0.97→1 + translate 8→0)
            UITween.Animate(TitleDurationSec, BlackPauseSec + TitleDelaySec, UITween.EaseOutCubic, t =>
            {
                SetState(title, opacity: t, scale: Mathf.Lerp(0.97f, 1f, t), translateY: Mathf.Lerp(8f, 0f, t));
            });

            // Subtitle: blur-fade in, staggered
            UITween.Animate(SubtitleDurationSec, BlackPauseSec + SubtitleDelaySec, UITween.EaseOutCubic, t =>
            {
                SetState(subtitle, opacity: t, scale: Mathf.Lerp(0.98f, 1f, t), translateY: Mathf.Lerp(4f, 0f, t));
            });
        }

        private static void SetState(VisualElement element, float opacity, float scale, float translateY)
        {
            if (element == null)
            {
                return;
            }
            element.style.opacity = opacity;
            element.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            element.style.translate = new StyleTranslate(new Translate(0, translateY));
        }
    }
}
