#if false
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystem.UI.Effects
{
    public class UIEffectDemo : MonoBehaviour
    {

        private IUIEffectTransition currentEffect;
        private VisualElement effectButtonContainer;
        private VisualElement uiElement;
        private Label uiLabel;

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;

            // Creating the UI Label Element to apply transitions
            uiElement = new Label("UI Effect Demo");
            uiElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            uiElement.style.fontSize = 24;
            uiElement.style.width = 300;
            uiElement.style.height = 100;

            uiElement.style.backgroundColor = new StyleColor(Color.cyan);
            uiElement.style.display = DisplayStyle.Flex;
            root.Add(uiElement);

            // Creating Buttons for each effect
            effectButtonContainer = new VisualElement();
            effectButtonContainer.style.flexDirection = FlexDirection.Column;
            effectButtonContainer.style.alignItems = Align.Center;
            root.Add(effectButtonContainer);

            // Add buttons for each effect
            AddEffectButton("Bounce", new UIBounceEffect(direction: UIBounceEffect.BounceDirection.Down));
            AddEffectButton("Wobble", new UIWobbleEffect());
            AddEffectButton("Pulse", new UIPulseEffect());
            AddEffectButton("Elastic", new UIElasticEffect());
            AddEffectButton("Zoom", new UIZoomEffect());
            AddEffectButton("Spin", new UISpinEffect());
            AddEffectButton("Shake", new UIShakeEffect());
            AddEffectButton("Blink", new UIBlinkEffect());
            AddEffectButton("Fade", new UIFadeEffect());
            AddEffectButton("Slide", new UISlideEffect());
            AddEffectButton("Scale", new UIScaleEffect());
            AddEffectButton("Flip", new UIFlipEffect());
        }

        private void AddEffectButton(string effectName, IUIEffectTransition effect)
        {
            Button button = new Button(() => ApplyEffect(effect)) {
                text = $"Apply {effectName} Effect"
            };
            effectButtonContainer.Add(button);
        }

        private void ApplyEffect(IUIEffectTransition effect)
        {
            // Apply the current effect's transition out if the element is visible
            if (currentEffect != null)
            {
                currentEffect.ApplyTransitionOut(uiElement, () =>
                {
                    currentEffect = effect;
                    ApplyCurrentEffect();
                });
            }
            else
            {
                currentEffect = effect;
                ApplyCurrentEffect();
            }
        }

        private void ApplyCurrentEffect()
        {
            currentEffect?.ApplyTransitionIn(uiElement, null);
        }
    }
}
#endif