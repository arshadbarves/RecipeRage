using UnityEngine;
using UnityEngine.UI;

namespace Core.TouchInputSystem
{
    public abstract class BaseTouchControl : MonoBehaviour
    {
        protected TouchControlConfig Config;
        protected Image Image;
        protected RectTransform RectTransform;

        public virtual void Initialize(TouchControlConfig config)
        {
            Config = config;
            RectTransform = gameObject.AddComponent<RectTransform>();
            Image = gameObject.AddComponent<Image>();
            Image.sprite = config.icon;
            RectTransform.sizeDelta = config.defaultSize;
            RectTransform.anchoredPosition = config.defaultPosition;
        }

        public abstract void StartEdit();
        public abstract void StopEdit();
    }
}