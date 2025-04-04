using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.UI.Core;

namespace RecipeRage.UI.Core
{
    /// <summary>
    /// ScriptableObject to define the properties of a UI screen.
    /// Create instances of this in the Unity Editor (e.g., in Assets/Resources/UI/Screens).
    /// </summary>
    [CreateAssetMenu(fileName = "ScreenDefinition", menuName = "RecipeRage/UI/Screen Definition", order = 1)]
    public class ScreenDefinition : ScriptableObject
    {
        [Tooltip("Unique identifier for this screen.")]
        [SerializeField] private ScreenId _screenId = ScreenId.None;

        [Tooltip("The UXML asset that defines the layout of this screen.")]
        [SerializeField] private VisualTreeAsset _uxmlAsset;

        [Tooltip("Optional C# controller script associated with this screen's logic.")]
        [SerializeField] private string _controllerTypeName; // Store as string to avoid direct references

        [Tooltip("The group this screen belongs to (e.g., FullScreen, Popup).")]
        [SerializeField] private ScreenGroup _screenGroup = ScreenGroup.FullScreen;

        [Tooltip("Whether to keep this screen's state when navigating away and back.")]
        [SerializeField] private bool _persistState = false;

        public ScreenId ScreenId => _screenId;
        public VisualTreeAsset UxmlAsset => _uxmlAsset;
        public string ControllerTypeName => _controllerTypeName;
        public System.Type ControllerType => !string.IsNullOrEmpty(_controllerTypeName) ? System.Type.GetType(_controllerTypeName) : null;
        public ScreenGroup ScreenGroup => _screenGroup;
        public bool PersistState => _persistState;
    }
}
