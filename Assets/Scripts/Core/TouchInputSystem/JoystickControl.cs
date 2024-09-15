using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TouchInputSystem
{
    public class JoystickControl : BaseTouchControl, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _knob;
        private Vector2 _joystickCenter;
        private bool _isDragging;

        public Vector2 Value { get; private set; }

        public override void Initialize(TouchControlConfig config)
        {
            base.Initialize(config);
            CreateKnob();
        }

        private void CreateKnob()
        {
            GameObject knobObject = new GameObject("Knob");
            knobObject.transform.SetParent(transform, false);
            _knob = knobObject.AddComponent<RectTransform>();
            Image knobImage = knobObject.AddComponent<Image>();
            knobImage.sprite = Config.icon;
            _knob.sizeDelta = Config.defaultSize * 0.5f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _joystickCenter = RectTransform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                Vector2 direction = eventData.position - _joystickCenter;
                Value = direction.magnitude > Config.deadZone ? direction.normalized : Vector2.zero;
                _knob.position = _joystickCenter + direction.normalized * Mathf.Min(direction.magnitude, RectTransform.sizeDelta.x * 0.5f);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            Value = Vector2.zero;
            _knob.position = _joystickCenter;
        }

        public override void StartEdit()
        {
            gameObject.AddComponent<DraggableUI>();
        }

        public override void StopEdit()
        {
            Destroy(GetComponent<DraggableUI>());
        }
    }
}