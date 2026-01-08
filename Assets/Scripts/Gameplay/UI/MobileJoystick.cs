using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.UI
{
    /// <summary>
    /// Mobile joystick controller similar to PUBG Mobile.
    /// Supports both fixed and floating joystick modes.
    /// </summary>
    public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Joystick Settings")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRadius = 50f;
        [SerializeField] private bool isFixedPosition = false;
        
        [Header("Joystick Properties")]
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private float sensitivity = 1.0f;
        
        private Vector2 _inputVector;
        private Vector2 _joystickStartPosition;
        private Canvas _canvas;
        private Camera _camera;
        private bool _isDragging = false;
        
        public Vector2 InputVector => _inputVector;
        public float Horizontal => _inputVector.x;
        public float Vertical => _inputVector.y;
        
        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            _camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            
            if (joystickBackground != null)
            {
                _joystickStartPosition = joystickBackground.anchoredPosition;
            }
            
            LoadSettings();
            
            if (!isFixedPosition && joystickBackground != null)
            {
                joystickBackground.gameObject.SetActive(false);
            }
        }
        
        private void LoadSettings()
        {
            float size = PlayerPrefs.GetFloat("JoystickSize", 1.0f);
            float opacity = PlayerPrefs.GetFloat("JoystickOpacity", 0.7f);
            deadZone = PlayerPrefs.GetFloat("JoystickDeadZone", 0.1f);
            isFixedPosition = PlayerPrefs.GetInt("JoystickFixed", 0) == 1;
            
            if (joystickBackground != null)
            {
                joystickBackground.localScale = Vector3.one * size;
                CanvasGroup canvasGroup = joystickBackground.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = joystickBackground.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = opacity;
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            
            if (!isFixedPosition && joystickBackground != null)
            {
                joystickBackground.gameObject.SetActive(true);
                
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    transform as RectTransform,
                    eventData.position,
                    _camera,
                    out localPoint
                );
                
                joystickBackground.anchoredPosition = localPoint;
            }
            
            OnDrag(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || joystickBackground == null) return;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                _camera,
                out localPoint
            );
            
            Vector2 direction = localPoint.normalized;
            float distance = Mathf.Clamp(localPoint.magnitude, 0, joystickRadius);
            
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = direction * distance;
            }
            
            float normalizedDistance = distance / joystickRadius;
            
            if (normalizedDistance < deadZone)
            {
                _inputVector = Vector2.zero;
            }
            else
            {
                float adjustedDistance = (normalizedDistance - deadZone) / (1 - deadZone);
                _inputVector = direction * adjustedDistance * sensitivity;
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _inputVector = Vector2.zero;
            
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
            
            if (!isFixedPosition && joystickBackground != null)
            {
                joystickBackground.gameObject.SetActive(false);
            }
        }
        
        public void ResetJoystick()
        {
            _inputVector = Vector2.zero;
            _isDragging = false;
            
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
            
            if (joystickBackground != null && isFixedPosition)
            {
                joystickBackground.anchoredPosition = _joystickStartPosition;
            }
        }
    }
}
