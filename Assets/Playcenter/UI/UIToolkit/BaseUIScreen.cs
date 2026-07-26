using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.UI
{
    /// <summary>
    /// Base for all screens. UXML/USS own the layout; this class owns bindings
    /// and lifecycle. No code-behind layout — Query + bind only.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class BaseUIScreen : MonoBehaviour
    {
        private UIDocument _document;

        public VisualElement Root => _document.rootVisualElement;

        protected virtual void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
