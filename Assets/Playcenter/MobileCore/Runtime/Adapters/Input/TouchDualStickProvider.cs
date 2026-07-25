using UnityEngine;
using UnityEngine.InputSystem;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Pumps Unity InputSystem touch/mouse samples into a DualStickModel as PointerEvents.
    /// The ONLY Unity-coupled input type in the module.
    /// </summary>
    public sealed class TouchDualStickProvider
    {
        private readonly DualStickModel _model;

        public TouchDualStickProvider(DualStickModel model)
        {
            _model = model;
        }

        public void Pump()
        {
            float halfW = Screen.width * 0.5f;
            float halfH = Screen.height * 0.5f;

            Touchscreen ts = Touchscreen.current;
            if (ts != null)
            {
                for (int i = 0; i < ts.touches.Count; i++)
                {
                    var touch = ts.touches[i];
                    UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();
                    if (!touch.isInProgress && phase != UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        continue;
                    }

                    _model.OnPointer(new PointerEvent(
                        touch.touchId.ReadValue(),
                        touch.position.ReadValue().x,
                        touch.position.ReadValue().y,
                        MapPhase(phase),
                        halfW,
                        halfH));
                }
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                bool pressed = mouse.leftButton.isPressed;
                Vector2 pos = mouse.position.ReadValue();
                _model.OnPointer(new PointerEvent(
                    0,
                    pos.x,
                    pos.y,
                    pressed ? PointerPhase.Moved : PointerPhase.Ended,
                    halfW,
                    halfH));
            }
        }

        private static PointerPhase MapPhase(UnityEngine.InputSystem.TouchPhase phase)
        {
            return phase switch
            {
                UnityEngine.InputSystem.TouchPhase.Began => PointerPhase.Began,
                UnityEngine.InputSystem.TouchPhase.Moved => PointerPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Stationary => PointerPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Ended => PointerPhase.Ended,
                _ => PointerPhase.Cancelled,
            };
        }
    }
}
