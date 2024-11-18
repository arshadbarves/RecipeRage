using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Input
{
    public class InputAreaManager : MonoBehaviour
    {

        public enum InputPriority
        {
            Low = 0,
            Medium = 1,
            High = 2
        }

        [SerializeField] private List<InputArea> inputAreas = new List<InputArea>();
        private readonly Dictionary<string, InputArea> _areaLookup = new Dictionary<string, InputArea>();

        private void Awake()
        {
            InitializeAreas();
        }

        private void InitializeAreas()
        {
            _areaLookup.Clear();
            foreach (InputArea area in inputAreas)
            {
                if (!string.IsNullOrEmpty(area.id))
                {
                    _areaLookup[area.id] = area;
                }
            }
        }

        public bool IsPointInArea(string areaId, Vector2 screenPoint)
        {
            if (_areaLookup.TryGetValue(areaId, out InputArea area))
            {
                return RectTransformUtility.RectangleContainsScreenPoint(area.area, screenPoint);
            }
            return false;
        }

        public InputArea GetHighestPriorityAreaAtPoint(Vector2 screenPoint)
        {
            InputArea highestPriorityArea = null;
            InputPriority highestPriority = InputPriority.Low;

            foreach (InputArea area in inputAreas)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(area.area, screenPoint))
                {
                    if (area.priority > highestPriority)
                    {
                        highestPriority = area.priority;
                        highestPriorityArea = area;
                    }
                }
            }

            return highestPriorityArea;
        }
        [Serializable]
        public class InputArea
        {
            public string id;
            public RectTransform area;
            public bool isExclusive;
            public bool allowDragging;
            public InputPriority priority;
        }
    }
}