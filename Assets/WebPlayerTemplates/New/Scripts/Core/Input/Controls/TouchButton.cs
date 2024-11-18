using Core.Input.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Input.Controls
{
    public class TouchButton : InputControl
    {
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image cooldownImage;

        private InputControlConfig.ButtonConfig _config;
        private float _cooldownProgress;

        public void Initialize(InputControlConfig.ButtonConfig config)
        {
            _config = config;

            buttonImage.sprite = config.normalSprite;
            SetPosition(config.defaultPosition);
            RectTransform.sizeDelta = config.size;

            UpdateCooldownVisual(0f);
        }

        protected override Vector2 GetNormalizedInput(Vector2 pointerPosition)
        {
            return (pointerPosition - (Vector2)RectTransform.position).normalized;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            buttonImage.sprite = _config.pressedSprite;
            buttonImage.color = _config.pressedColor;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            buttonImage.sprite = _config.normalSprite;
            buttonImage.color = _config.normalColor;
        }

        public void UpdateCooldownVisual(float progress)
        {
            _cooldownProgress = progress;
            cooldownImage.fillAmount = progress;
        }
    }
}