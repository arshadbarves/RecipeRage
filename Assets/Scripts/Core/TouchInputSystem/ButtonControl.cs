using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.TouchInputSystem
{
    public class ButtonControl : BaseTouchControl, IPointerDownHandler, IPointerUpHandler
    {
        private Button _button;
        public bool IsPressed { get; private set; }

        public override void Initialize(TouchControlConfig config)
        {
            base.Initialize(config);
            _button = gameObject.AddComponent<Button>();
            _button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = _button.colors;
            _button.colors = colors;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
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