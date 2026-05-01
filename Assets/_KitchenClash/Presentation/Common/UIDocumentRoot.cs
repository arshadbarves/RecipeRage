using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD: Holds UIDocument. Passes rootVisualElement to RouterService.
    /// MonoBehaviour used ONLY here in Presentation.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIDocumentRoot : MonoBehaviour
    {
        private UIDocument _document;

        public VisualElement Root => _document?.rootVisualElement;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }
    }
}
