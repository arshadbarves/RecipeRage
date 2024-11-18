using Core.Input.Controls;
using UnityEngine;

namespace Core.Input
{
    public class TouchInputState
    {
        public readonly float StartTime;
        public string ActiveAreaId;
        public InputControl ActiveControl;
        public Vector2 CurrentPosition;
        public int FingerId;
        public bool IsDragging;
        public Vector2 StartPosition;

        public TouchInputState(int fingerId, Vector2 position)
        {
            FingerId = fingerId;
            StartPosition = position;
            CurrentPosition = position;
            StartTime = Time.time;
            IsDragging = false;
            ActiveAreaId = null;
            ActiveControl = null;
        }

        public float GetDuration()
        {
            return Time.time - StartTime;
        }

        public float GetDragDistance()
        {
            return Vector2.Distance(StartPosition, CurrentPosition);
        }

        public Vector2 GetDragDelta()
        {
            return CurrentPosition - StartPosition;
        }
    }
}