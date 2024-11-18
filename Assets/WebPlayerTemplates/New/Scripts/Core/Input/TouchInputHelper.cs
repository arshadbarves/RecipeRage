using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Input
{
    public static class TouchInputHelper
    {
        public const float MinDragDistance = 10f;
        public static bool IsTouchOverUI(Vector2 position)
        {
            if (EventSystem.current == null)
                return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current) {
                position = position
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }
        public static Vector2 GetWorldPosition(Vector2 screenPosition, Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
        }

        public static float GetTouchPressure(Touch touch)
        {
            return touch.pressure > 0 ? touch.pressure : 1f;
        }

        public static bool IsDoubleTap(float lastTapTime, float currentTime, Vector2 lastTapPos, Vector2 currentPos)
        {
            const float doubleTapTime = 0.3f;
            const float doubleTapDistance = 50f;

            return currentTime - lastTapTime < doubleTapTime &&
                   Vector2.Distance(lastTapPos, currentPos) < doubleTapDistance;
        }

        public static Vector2 ClampToScreen(Vector2 position, RectTransform rectTransform, Canvas canvas)
        {
            Vector2 size = rectTransform.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            float scale = canvas.scaleFactor;

            return new Vector2(
                Mathf.Clamp(position.x, size.x * 0.5f * scale, screenSize.x - size.x * 0.5f * scale),
                Mathf.Clamp(position.y, size.y * 0.5f * scale, screenSize.y - size.y * 0.5f * scale)
            );
        }
    }
}