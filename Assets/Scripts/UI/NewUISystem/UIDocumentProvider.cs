using Core.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem
{
    /// <summary>
    /// MonoBehaviour component that provides UIDocument to the UIManager service
    /// This is the only MonoBehaviour needed for the UI system
    /// Attach this to a GameObject with a UIDocument component
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIDocumentProvider : MonoBehaviour
    {
        private UIDocument _uiDocument;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            
            if (_uiDocument == null)
            {
                Debug.LogError("[UIDocumentProvider] UIDocument component not found!");
                return;
            }
        }

        private void Start()
        {
            // Initialize the UI service with this UIDocument
            var uiService = GameBootstrap.Services?.UIService as UIManager;
            if (uiService != null)
            {
                uiService.Initialize(_uiDocument);
                Debug.Log("[UIDocumentProvider] UIManager initialized with UIDocument");
            }
            else
            {
                Debug.LogError("[UIDocumentProvider] UIService not found in ServiceContainer or is not UIManager type");
            }
        }
    }
}
