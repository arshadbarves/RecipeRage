using Playcenter.SDK;
using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.SDK.Unity
{
    /// <summary>
    /// Thin MonoBehaviour helper that owns the UIDocument and forwards
    /// lifecycle events to ToolkitShellUi. Optional — game bootstrap
    /// can instantiate ToolkitShellUi directly without this component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShellScreenController : MonoBehaviour
    {
        [SerializeField] UIDocument _document;

        ToolkitShellUi _shellUi;

        /// <summary>Exposes the underlying IShellUi for injection into game bootstrap.</summary>
        public IShellUi ShellUi => _shellUi;

        void Awake()
        {
            _shellUi = new ToolkitShellUi(_document);
            DontDestroyOnLoad(gameObject);
        }
    }
}
