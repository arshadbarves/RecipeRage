using Core.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// MonoBehaviour component that provides UIDocument to the UIService
    /// This is the only MonoBehaviour needed for the UI system
    /// Attach this to a GameObject with a UIDocument component
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIDocumentProvider : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private bool _isInitialized;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
            {
                Debug.LogError("[UIDocumentProvider] UIDocument component not found!");
                return;
            }
        }

        public void Initialize(IUIService uiService)
        {
            // Get the UI service
            if (uiService == null)
            {
                Debug.LogError("[UIDocumentProvider] UIService not found or is not of type UIService");
                return;
            }

            // Initialize the UI service with this UIDocument
            uiService.Initialize(_uiDocument);
            _isInitialized = true;

            Debug.Log("[UIDocumentProvider] ✅ UIService initialized with UIDocument");
        }

        /// <summary>
        /// Check if the UI system is ready
        /// </summary>
        public bool IsInitialized => _isInitialized;
    }
}
