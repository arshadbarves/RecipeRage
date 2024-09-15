using System.Collections.Generic;
using UnityEngine;

namespace Core.TouchInputSystem
{
    public class TouchLayoutSaver
    {
        private const string LayoutPrefsKey = "TouchControlLayout";

        [System.Serializable]
        private class ControlLayout
        {
            public string controlName;
            public Vector2 position;
            public Vector2 size;
        }

        [System.Serializable]
        private class LayoutData
        {
            public List<ControlLayout> controls = new List<ControlLayout>();
        }

        public void SaveLayout(Dictionary<string, BaseTouchControl> controls)
        {
            LayoutData layoutData = new LayoutData();
            foreach (var kvp in controls)
            {
                RectTransform rectTransform = kvp.Value.GetComponent<RectTransform>();
                layoutData.controls.Add(new ControlLayout
                {
                    controlName = kvp.Key,
                    position = rectTransform.anchoredPosition,
                    size = rectTransform.sizeDelta
                });
            }
            string json = JsonUtility.ToJson(layoutData);
            PlayerPrefs.SetString(LayoutPrefsKey, json);
            PlayerPrefs.Save();
        }

        public void LoadLayout(Dictionary<string, BaseTouchControl> controls)
        {
            if (PlayerPrefs.HasKey(LayoutPrefsKey))
            {
                string json = PlayerPrefs.GetString(LayoutPrefsKey);
                LayoutData layoutData = JsonUtility.FromJson<LayoutData>(json);
                foreach (var controlLayout in layoutData.controls)
                {
                    if (controls.TryGetValue(controlLayout.controlName, out BaseTouchControl control))
                    {
                        RectTransform rectTransform = control.GetComponent<RectTransform>();
                        rectTransform.anchoredPosition = controlLayout.position;
                        rectTransform.sizeDelta = controlLayout.size;
                    }
                }
            }
        }
    }
}