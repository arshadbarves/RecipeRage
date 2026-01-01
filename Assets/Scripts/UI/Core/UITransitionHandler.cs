using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Extensions;

namespace UI.Core
{
    /// <summary>
    /// Standardized transition handler for UI Toolkit using DOTween.
    /// Handles Show/Hide animations generically.
    /// </summary>
    public static class UITransitionHandler
    {
        private const float DefaultDuration = 0.3f;

        public static async UniTask AnimateShow(VisualElement element, UITransitionType type, float duration = DefaultDuration)
        {
            if (element == null) return;

            element.style.display = DisplayStyle.Flex;
            element.style.opacity = 1; // Default visibility

            switch (type)
            {
                case UITransitionType.Fade:
                    element.style.opacity = 0;
                    await DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, 1f, duration)
                        .SetEase(Ease.OutQuad).ToUniTask();
                    break;

                case UITransitionType.SlideUp:
                    await AnimateSlide(element, new Vector3(0, 100, 0), Vector3.zero, duration);
                    break;

                case UITransitionType.SlideDown:
                    await AnimateSlide(element, new Vector3(0, -100, 0), Vector3.zero, duration);
                    break;

                case UITransitionType.SlideLeft:
                    await AnimateSlide(element, new Vector3(-100, 0, 0), Vector3.zero, duration);
                    break;

                case UITransitionType.SlideRight:
                    await AnimateSlide(element, new Vector3(100, 0, 0), Vector3.zero, duration);
                    break;

                case UITransitionType.Scale:
                    element.transform.scale = Vector3.zero;
                    await DOTween.To(() => element.transform.scale, x => element.transform.scale = x, Vector3.one, duration)
                        .SetEase(Ease.OutBack).ToUniTask();
                    break;
            }
        }

        public static async UniTask AnimateHide(VisualElement element, UITransitionType type, float duration = DefaultDuration)
        {
            if (element == null) return;

            switch (type)
            {
                case UITransitionType.Fade:
                    await DOTween.To(() => element.style.opacity.value, x => element.style.opacity = x, 0f, duration)
                        .SetEase(Ease.InQuad).ToUniTask();
                    break;

                case UITransitionType.SlideUp:
                    await AnimateSlide(element, Vector3.zero, new Vector3(0, 100, 0), duration);
                    break;

                case UITransitionType.SlideDown:
                    await AnimateSlide(element, Vector3.zero, new Vector3(0, -100, 0), duration);
                    break;

                case UITransitionType.SlideLeft:
                    await AnimateSlide(element, Vector3.zero, new Vector3(-100, 0, 0), duration);
                    break;

                case UITransitionType.SlideRight:
                    await AnimateSlide(element, Vector3.zero, new Vector3(100, 0, 0), duration);
                    break;

                case UITransitionType.Scale:
                    await DOTween.To(() => element.transform.scale, x => element.transform.scale = x, Vector3.zero, duration)
                        .SetEase(Ease.InBack).ToUniTask();
                    break;
            }

            element.style.display = DisplayStyle.None;
        }

        private static async UniTask AnimateSlide(VisualElement element, Vector3 start, Vector3 end, float duration)
        {
            element.transform.position = start;
            await DOTween.To(() => element.transform.position, x => element.transform.position = x, end, duration)
                .SetEase(Ease.OutQuad).ToUniTask();
        }
    }
}