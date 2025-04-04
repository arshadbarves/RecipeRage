using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace RecipeRage.UI.Core
{
    /// <summary>
    /// Abstract base class for all UI screen controllers.
    /// Provides common lifecycle methods for screens.
    /// </summary>
    public abstract class BaseScreenController
    {
        /// <summary>
        /// The Screen Definition asset associated with this controller.
        /// </summary>
        public ScreenDefinition Definition { get; protected set; }

        /// <summary>
        /// The root VisualElement of the screen's UI hierarchy.
        /// </summary>
        public VisualElement RootElement { get; protected set; }

        /// <summary>
        /// Reference to the Screen Manager.
        /// </summary>
        protected UIScreenManager ScreenManager { get; private set; }

        /// <summary>
        /// Called once when the screen is first initialized.
        /// Use this for setting up references and initial state.
        /// </summary>
        /// <param name="manager">The UI Screen Manager.</param>
        /// <param name="definition">The screen definition.</param>
        /// <param name="rootElement">The root visual element of the screen.</param>
        public virtual void Initialize(UIScreenManager manager, ScreenDefinition definition, VisualElement rootElement)
        {
            ScreenManager = manager;
            Definition = definition;
            RootElement = rootElement;
        }

        /// <summary>
        /// Called just before the screen becomes visible.
        /// Use this for data loading or animations that should happen before showing.
        /// Can be asynchronous.
        /// </summary>
        /// <param name="data">Optional data passed from the previous screen or caller.</param>
        /// <returns>A Task that completes when the screen is ready to be shown.</returns>
        public virtual Task OnBeforeShowAsync(object data = null)
        {
            return Task.CompletedTask; // Default implementation
        }

        /// <summary>
        /// Called immediately after the screen becomes visible.
        /// Use this for starting animations or logic that depends on the screen being visible.
        /// </summary>
        public virtual void OnAfterShow()
        {
            // Default implementation
        }

        /// <summary>
        /// Called just before the screen is hidden.
        /// Use this for cleanup or saving state before hiding.
        /// Can be asynchronous.
        /// </summary>
        /// <returns>A Task that completes when the screen is ready to be hidden.</returns>
        public virtual Task OnBeforeHideAsync()
        {
            return Task.CompletedTask; // Default implementation
        }

        /// <summary>
        /// Called immediately after the screen is hidden.
        /// </summary>
        public virtual void OnAfterHide()
        {
            // Default implementation
        }

        /// <summary>
        /// Called when the screen is completely destroyed (e.g., removed from persistence).
        /// Use this for final cleanup.
        /// </summary>
        public virtual void OnDestroy()
        {
            // Default implementation
        }

        /// <summary>
        /// Called when navigating back to a screen that had its state persisted.
        /// Use this to restore state or refresh data.
        /// </summary>
        /// <param name="restoredData">The data that was previously saved (implementation specific).</param>
        public virtual void OnRestored(object restoredData = null)
        {
            // Default implementation
        }

        /// <summary>
        /// Called when the screen needs to save its state before being hidden (if PersistState is true).
        /// </summary>
        /// <returns>An object representing the state to be saved.</returns>
        public virtual object OnSaveState()
        {
            return null; // Default: no state saved
        }
    }
}
