using Core.Input.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Input.Controls
{
    public class TouchJoystick : InputControl
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image handleImage;

        private Vector2 _center;
        private InputControlConfig.JoystickConfig _config;
        private float _radius;

        public void Initialize(InputControlConfig.JoystickConfig config)
        {
            _config = config;
            _radius = config.radius;

            backgroundImage.sprite = config.backgroundSprite;
            handleImage.sprite = config.handleSprite;

            SetPosition(config.defaultPosition);
            ResetHandle();
        }

        protected override Vector2 GetNormalizedInput(Vector2 pointerPosition)
        {
            Vector2 delta = (pointerPosition - _center) / _radius;
            return Vector2.ClampMagnitude(delta, 1f);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            Vector2 delta = eventData.position - _center;
            handleImage.rectTransform.anchoredPosition = Vector2.ClampMagnitude(delta, _radius);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            ResetHandle();
        }

        private void ResetHandle()
        {
            handleImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}