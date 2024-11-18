using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Input.Controls
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Canvas))]
    public abstract class InputControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        protected Canvas Canvas;
        protected CanvasGroup CanvasGroup;
        protected bool IsPressed;
        protected Vector2 PressPosition;
        protected float PressTime;
        protected RectTransform RectTransform;

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            Canvas = GetComponentInParent<Canvas>();
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            OnInputChanged?.Invoke(GetNormalizedInput(eventData.position));
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            PressPosition = eventData.position;
            PressTime = Time.time;
            OnInputStart?.Invoke(GetNormalizedInput(eventData.position));
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            OnInputEnd?.Invoke(GetNormalizedInput(eventData.position));
        }

        public event Action<Vector2> OnInputStart;
        public event Action<Vector2> OnInputChanged;
        public event Action<Vector2> OnInputEnd;

        protected abstract Vector2 GetNormalizedInput(Vector2 pointerPosition);

        public virtual void SetPosition(Vector2 position)
        {
            RectTransform.anchoredPosition = position;
        }

        public virtual void SetActive(bool active)
        {
            CanvasGroup.alpha = active ? 1f : 0f;
            CanvasGroup.blocksRaycasts = active;
            CanvasGroup.interactable = active;
        }
    }
}