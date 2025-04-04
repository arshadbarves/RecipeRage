using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeRage.Core.Patterns;
using RecipeRage.Modules.Logging;
using RecipeRage.UI.Core; // Added UI Core namespace

namespace RecipeRage.UI.Core
{
    /// <summary>
    /// Manages the UI screen lifecycle, navigation stack, and screen state persistence.
    /// Responsible for loading UXML, instantiating controllers, and handling transitions.
    /// </summary>
    public class UIScreenManager : MonoBehaviourSingleton<UIScreenManager>
    {
        [Header("UI Setup")]
        [Tooltip("The main UIDocument component in the scene.")]
        [SerializeField] private UIDocument _uiDocument;

        [Tooltip("Path within the Resources folder where ScreenDefinition assets are located.")]
        [SerializeField] private string _screenDefinitionPath = "UI/Screens";

        [Tooltip("The ID of the initial screen to show when the game starts.")]
        [SerializeField] private ScreenId _initialScreen = ScreenId.SplashScreen;

        private Dictionary<ScreenId, ScreenDefinition> _screenDefinitions;
        private Dictionary<ScreenId, BaseScreenController> _activeControllers = new Dictionary<ScreenId, BaseScreenController>();
        private Dictionary<ScreenId, object> _persistedState = new Dictionary<ScreenId, object>();
        private Stack<ScreenHistoryEntry> _navigationStack = new Stack<ScreenHistoryEntry>();
        private bool _isTransitioning = false;
        private bool _isInitialized = false; // Added flag

        // Class to hold screen history information
        private class ScreenHistoryEntry
        {
            public ScreenId ScreenId { get; set; }
            public BaseScreenController Controller { get; set; }
            public VisualElement RootElement { get; set; }
            public object PassedData { get; set; }

            // Constructor for potentially persisted screens
            public ScreenHistoryEntry(ScreenId id, BaseScreenController controller, VisualElement root, object data = null)
            {
                ScreenId = id;
                Controller = controller;
                RootElement = root;
                PassedData = data;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            LoadScreenDefinitions();
            // Ensure the root VE is clear initially (optional, good practice)
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                _uiDocument.rootVisualElement.Clear();
            }
        }

        private void Start()
        {
            // Initialization is now triggered by StartUIManager
            // We still might need Start for other Unity lifecycle events if necessary
        }

        /// <summary>
        /// Public method to initialize and show the first screen. 
        /// Should be called by GameBootstrap after its own initialization.
        /// </summary>
        public void StartUIManager()
        {
            if (_isInitialized) return; // Prevent double initialization

            if (_uiDocument == null)
            {
                LogHelper.Error("UIScreenManager", "UIDocument is not assigned in the Inspector.");
                enabled = false;
                return;
            }
            if (_uiDocument.rootVisualElement == null)
            {
                LogHelper.Error("UIScreenManager", "UIDocument rootVisualElement is null. Cannot initialize UI.");
                enabled = false;
                return;
            }

            if (_screenDefinitions.Count == 0)
            {
                LogHelper.Warning("UIScreenManager", "No ScreenDefinitions found. UI system may not function correctly.");
            }

            LogHelper.Info("UIScreenManager", "Starting UI Manager and showing initial screen.");
            _isInitialized = true;

            // Show the initial screen
            _ = ShowScreenAsync(_initialScreen);
        }

        private void LoadScreenDefinitions()
        {
            _screenDefinitions = new Dictionary<ScreenId, ScreenDefinition>();
            var definitions = Resources.LoadAll<ScreenDefinition>(_screenDefinitionPath);
            foreach (var def in definitions)
            {
                if (def.ScreenId == ScreenId.None)
                {
                    LogHelper.Warning("UIScreenManager", $"ScreenDefinition '{def.name}' has ScreenId set to None. It will be ignored.");
                    continue;
                }
                if (_screenDefinitions.ContainsKey(def.ScreenId))
                {
                    LogHelper.Warning("UIScreenManager", $"Duplicate ScreenId '{def.ScreenId}' found in ScreenDefinition '{def.name}'. Only the first one loaded will be used.");
                    continue;
                }
                _screenDefinitions.Add(def.ScreenId, def);
                LogHelper.Debug("UIScreenManager", $"Loaded Screen Definition: {def.ScreenId}");
            }
        }

        /// <summary>
        /// Navigates to a specific screen.
        /// </summary>
        /// <param name="screenId">The ID of the screen to show.</param>
        /// <param name="data">Optional data to pass to the new screen's controller.</param>
        /// <param name="addToHistory">Whether to add this navigation to the history stack (for back button functionality).</param>
        /// <returns>A Task that completes when the transition is finished.</returns>
        public async Task ShowScreenAsync(ScreenId screenId, object data = null, bool addToHistory = true)
        {
            await TransitionToScreenAsync(screenId, data, addToHistory, isGoingBack: false);
        }

        /// <summary>
        /// Navigates back to the previous screen in the history stack.
        /// </summary>
        /// <returns>A Task that completes when the transition is finished.</returns>
        public async Task GoBackAsync()
        {
            if (_navigationStack.Count <= 1 || _isTransitioning)
            {
                LogHelper.Debug("UIScreenManager", "Cannot go back. Stack is empty or transition in progress.");
                return; // Can't go back from the initial screen or during transition
            }

            // Pop the current screen
            var currentEntry = _navigationStack.Pop();

            // Peek the previous screen
            var previousEntry = _navigationStack.Peek();

            await TransitionToScreenAsync(previousEntry.ScreenId, previousEntry.PassedData, false, isGoingBack: true, currentHistoryEntryToHide: currentEntry);
        }

        private async Task TransitionToScreenAsync(ScreenId targetScreenId, object data, bool addToHistory, bool isGoingBack, ScreenHistoryEntry currentHistoryEntryToHide = null)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("UIScreenManager", "Transition attempted before UI Manager was initialized.");
                return;
            }
            if (_isTransitioning)
            {
                LogHelper.Warning("UIScreenManager", "Transition already in progress. Ignoring request.");
                return;
            }
            _isTransitioning = true;

            LogHelper.Info("UIScreenManager", $"Transitioning to screen: {targetScreenId}");

            if (!_screenDefinitions.TryGetValue(targetScreenId, out var targetDefinition))
            {
                LogHelper.Error("UIScreenManager", $"ScreenDefinition not found for ID: {targetScreenId}");
                _isTransitioning = false;
                return;
            }

            // --- Hide Current Screen(s) ---
            List<Task> hideTasks = new List<Task>();
            List<ScreenHistoryEntry> entriesToHide = new List<ScreenHistoryEntry>();

            if (isGoingBack)
            {
                // If going back, we only hide the specific entry provided
                if (currentHistoryEntryToHide != null) entriesToHide.Add(currentHistoryEntryToHide);
            }
            else if (_navigationStack.Count > 0)
            {
                // If going forward, determine which screens to hide based on groups
                var currentTopEntry = _navigationStack.Peek();
                if (targetDefinition.ScreenGroup == ScreenGroup.FullScreen)
                {
                    // Hide all current screens if the new one is FullScreen
                    entriesToHide.AddRange(_navigationStack);
                }
                else if (targetDefinition.ScreenGroup == ScreenGroup.Popup && currentTopEntry.Controller.Definition.ScreenGroup == ScreenGroup.Popup)
                {
                    // Option: Hide previous popup when showing a new one? Or allow stacking?
                    // For now, let's assume we hide the previous popup
                    // entriesToHide.Add(currentTopEntry);
                    // OR Keep the previous popup (stacking popups) - do nothing here
                }
                else if (targetDefinition.ScreenGroup == ScreenGroup.Overlay)
                {
                    // Overlays typically don't hide other screens
                }
                else
                {
                    // Default: Hide the current top screen if it's a FullScreen
                    if (currentTopEntry.Controller.Definition.ScreenGroup == ScreenGroup.FullScreen)
                    {
                        entriesToHide.Add(currentTopEntry);
                    }
                }
            }

            foreach (var entry in entriesToHide)
            {
                hideTasks.Add(HideScreenInternalAsync(entry));
            }
            await Task.WhenAll(hideTasks);


            // --- Show Target Screen ---
            BaseScreenController targetController = null;
            VisualElement targetRootElement = null;
            object restoredData = null;
            bool wasRestored = false;

            // Check if the screen exists in history (for going back or potentially revisiting a persisted screen)
            ScreenHistoryEntry existingEntry = isGoingBack ? _navigationStack.Peek() : _navigationStack.FirstOrDefault(e => e.ScreenId == targetScreenId);

            if (existingEntry != null && existingEntry.Controller != null && existingEntry.Controller.Definition.PersistState)
            {
                // Restore existing persisted screen
                targetController = existingEntry.Controller;
                targetRootElement = existingEntry.RootElement;
                _persistedState.TryGetValue(targetScreenId, out restoredData);
                targetRootElement.style.display = DisplayStyle.Flex; // Make visible again
                targetController.OnRestored(restoredData);
                wasRestored = true;
                LogHelper.Debug("UIScreenManager", $"Restoring persisted screen: {targetScreenId}");
            }
            else
            {
                // Create new instance
                if (targetDefinition.UxmlAsset == null)
                {
                    LogHelper.Error("UIScreenManager", $"UXML Asset is null for ScreenDefinition: {targetScreenId}");
                    _isTransitioning = false;
                    return;
                }

                targetRootElement = targetDefinition.UxmlAsset.CloneTree();
                targetRootElement.style.flexGrow = 1; // Make it fill the parent
                targetRootElement.name = $"ScreenRoot_{targetScreenId}"; // Add identifiable name
                if (_uiDocument.rootVisualElement == null)
                {
                    LogHelper.Error("UIScreenManager", "UIDocument rootVisualElement is null when trying to add screen.");
                    _isTransitioning = false;
                    return;
                }
                _uiDocument.rootVisualElement.Add(targetRootElement);

                // Instantiate controller if defined
                var controllerType = targetDefinition.ControllerType;
                if (controllerType != null && typeof(BaseScreenController).IsAssignableFrom(controllerType))
                {
                    targetController = (BaseScreenController)System.Activator.CreateInstance(controllerType);
                    targetController.Initialize(this, targetDefinition, targetRootElement);
                    LogHelper.Debug("UIScreenManager", $"Instantiated controller: {controllerType.Name} for screen {targetScreenId}");
                }
                else if (!string.IsNullOrEmpty(targetDefinition.ControllerTypeName))
                {
                    LogHelper.Warning("UIScreenManager", $"Controller type '{targetDefinition.ControllerTypeName}' not found or doesn't inherit from BaseScreenController for screen {targetScreenId}.");
                }
            }

            // --- Lifecycle Callbacks --- 
            if (targetController != null)
            {
                await targetController.OnBeforeShowAsync(data); // Pass data here
            }

            targetRootElement.style.display = DisplayStyle.Flex; // Ensure it's visible

            if (targetController != null)
            {
                targetController.OnAfterShow();
            }

            // --- Update Navigation Stack --- 
            if (!isGoingBack) // Only add to stack if moving forward
            {
                if (addToHistory)
                {
                    // Remove existing entry if we are revisiting a screen that shouldn't stack
                    if (!targetDefinition.PersistState)
                    { // Or based on Screen Group rules
                      // Find and remove the specific non-persisted screen before pushing the new one
                        var nonPersistedEntry = _navigationStack.FirstOrDefault(e => e.ScreenId == targetScreenId);
                        if (nonPersistedEntry != null)
                        {
                            if (nonPersistedEntry.RootElement != null) _uiDocument.rootVisualElement.Remove(nonPersistedEntry.RootElement);
                            if (nonPersistedEntry.Controller != null) nonPersistedEntry.Controller.OnDestroy();
                            _activeControllers.Remove(nonPersistedEntry.ScreenId);
                            // Rebuild stack without the removed entry
                            _navigationStack = new Stack<ScreenHistoryEntry>(_navigationStack.Where(e => e.ScreenId != targetScreenId).Reverse());
                            LogHelper.Debug("UIScreenManager", $"Removed non-persisted instance of {targetScreenId} from stack before adding new one.");
                        }
                    }

                    var newEntry = new ScreenHistoryEntry(targetScreenId, targetController, targetRootElement, data); // Store passed data

                    // Clear stack if new screen is FullScreen and history shouldn't be kept
                    if (targetDefinition.ScreenGroup == ScreenGroup.FullScreen)
                    {
                        // Potentially clear based on a flag in ScreenDefinition? 
                        // For now, let's always clear for FullScreen transitions that add history
                        ClearStackBelow(newEntry); // Pass the new entry to keep it on top
                    }

                    _navigationStack.Push(newEntry);
                }
            }
            else
            {
                // When going back, the target screen is already on top after the pop, just update its visibility/state
                // (Handled in the restore section above)
                // Ensure the restored element is visually on top (bring to front)
                targetRootElement?.BringToFront();
            }

            // Clean up non-persisted controllers that were hidden and are NOT the target screen
            foreach (var entry in entriesToHide)
            {
                if (entry.ScreenId != targetScreenId && entry.RootElement != null)
                {
                    _uiDocument.rootVisualElement.Remove(entry.RootElement);
                    if (entry.Controller != null && !entry.Controller.Definition.PersistState)
                    {
                        entry.Controller.OnDestroy();
                        _activeControllers.Remove(entry.ScreenId);
                        LogHelper.Debug("UIScreenManager", $"Destroyed controller and removed VE for non-persisted screen: {entry.ScreenId}");
                    }
                    else if (entry.Controller != null && entry.Controller.Definition.PersistState)
                    {
                        LogHelper.Debug("UIScreenManager", $"Hid persisted screen VE: {entry.ScreenId}");
                        // Just hiding, state was saved in HideScreenInternalAsync
                    }
                }
                else if (entry.ScreenId == targetScreenId && !wasRestored && entry.RootElement != null)
                {
                    // This case handles hiding a screen that is immediately replaced by a new instance of itself (e.g. non-persisted screen revisited)
                    _uiDocument.rootVisualElement.Remove(entry.RootElement);
                    if (entry.Controller != null) entry.Controller.OnDestroy();
                    _activeControllers.Remove(entry.ScreenId);
                    LogHelper.Debug("UIScreenManager", $"Destroyed old instance of screen {entry.ScreenId} being replaced.");
                }
            }

            // Update active controllers
            if (targetController != null && !_activeControllers.ContainsKey(targetScreenId))
            {
                _activeControllers.Add(targetScreenId, targetController);
            }
            else if (targetController != null && _activeControllers.ContainsKey(targetScreenId) && _activeControllers[targetScreenId] != targetController)
            {
                // If replacing an existing controller instance (e.g., non-persisted screen)
                _activeControllers[targetScreenId] = targetController;
            }

            _isTransitioning = false;
            LogHelper.Info("UIScreenManager", $"Transition complete. Current top screen: {(_navigationStack.Count > 0 ? _navigationStack.Peek().ScreenId.ToString() : "None")}");
        }

        /// <summary>
        /// Hides a screen instance internally, saving state if needed.
        /// </summary>
        private async Task HideScreenInternalAsync(ScreenHistoryEntry entry)
        {
            if (entry.RootElement == null)
            {
                LogHelper.Warning("UIScreenManager", $"Attempted to hide screen {entry.ScreenId} with null RootElement.");
                return;
            }

            if (entry.Controller != null)
            {
                await entry.Controller.OnBeforeHideAsync();
                if (entry.Controller.Definition.PersistState)
                {
                    var state = entry.Controller.OnSaveState();
                    if (state != null)
                    {
                        _persistedState[entry.ScreenId] = state;
                        LogHelper.Debug("UIScreenManager", $"Persisted state for screen: {entry.ScreenId}");
                    }
                    else
                    {
                        _persistedState.Remove(entry.ScreenId); // Remove old state if null is returned
                    }
                }
            }

            entry.RootElement.style.display = DisplayStyle.None; // Hide the element

            if (entry.Controller != null)
            {
                entry.Controller.OnAfterHide();
            }

            LogHelper.Debug("UIScreenManager", $"Hid screen: {entry.ScreenId}");
        }

        /// <summary>
        /// Clears the navigation stack below a certain entry (used for FullScreen transitions).
        /// </summary>
        private void ClearStackBelow(ScreenHistoryEntry entryToKeepOnTop)
        {
            LogHelper.Debug("UIScreenManager", "Clearing navigation history below new FullScreen.");
            var tempStack = new Stack<ScreenHistoryEntry>();
            tempStack.Push(entryToKeepOnTop); // Keep the new top entry

            // Process and destroy old entries that were below
            while (_navigationStack.Count > 0)
            {
                var oldEntry = _navigationStack.Pop();
                if (oldEntry.ScreenId != entryToKeepOnTop.ScreenId) // Avoid destroying the one we keep
                {
                    // Destroy non-persisted screens immediately
                    if (oldEntry.Controller != null && !oldEntry.Controller.Definition.PersistState)
                    {
                        if (oldEntry.RootElement != null) _uiDocument.rootVisualElement.Remove(oldEntry.RootElement);
                        oldEntry.Controller.OnDestroy();
                        _activeControllers.Remove(oldEntry.ScreenId);
                        LogHelper.Debug("UIScreenManager", $"Destroyed {oldEntry.ScreenId} during history clear.");
                    }
                    else if (oldEntry.Controller != null && oldEntry.Controller.Definition.PersistState)
                    {
                        // If persisted, just ensure it's hidden (state was saved earlier)
                        if (oldEntry.RootElement != null) oldEntry.RootElement.style.display = DisplayStyle.None;
                        LogHelper.Debug("UIScreenManager", $"Hid persisted {oldEntry.ScreenId} during history clear.");
                    }
                    else if (oldEntry.RootElement != null)
                    {
                        // Element without controller, just remove
                        _uiDocument.rootVisualElement.Remove(oldEntry.RootElement);
                    }
                }
            }

            _navigationStack = tempStack; // Assign the new stack with only the top entry
        }

        /// <summary>
        /// Clears the entire navigation history stack and destroys/hides screens.
        /// </summary>
        public void ClearHistory()
        {
            LogHelper.Debug("UIScreenManager", "Clearing full navigation history.");
            while (_navigationStack.Count > 0)
            {
                var oldEntry = _navigationStack.Pop();
                // Destroy non-persisted screens immediately
                if (oldEntry.Controller != null && !oldEntry.Controller.Definition.PersistState)
                {
                    if (oldEntry.RootElement != null) _uiDocument.rootVisualElement.Remove(oldEntry.RootElement);
                    oldEntry.Controller.OnDestroy();
                    _activeControllers.Remove(oldEntry.ScreenId);
                }
                else if (oldEntry.Controller != null && oldEntry.Controller.Definition.PersistState)
                {
                    // If persisted, just ensure it's hidden (state was saved earlier)
                    if (oldEntry.RootElement != null) oldEntry.RootElement.style.display = DisplayStyle.None;
                }
                else if (oldEntry.RootElement != null)
                {
                    // Element without controller, just remove
                    _uiDocument.rootVisualElement.Remove(oldEntry.RootElement);
                }
            }
            _persistedState.Clear(); // Clear persisted state as well when clearing history
        }

        public T GetController<T>(ScreenId screenId) where T : BaseScreenController
        {
            // Check active controllers first (includes persisted ones)
            if (_activeControllers.TryGetValue(screenId, out var controller) && controller is T typedController)
            {
                return typedController;
            }

            // Check navigation stack (might contain an entry for a screen about to be shown)
            var entry = _navigationStack.FirstOrDefault(e => e.ScreenId == screenId);
            if (entry?.Controller is T stackController)
            {
                return stackController;
            }

            return default(T);
        }

        public bool IsScreenActive(ScreenId screenId)
        {
            return _navigationStack.Count > 0 && _navigationStack.Peek().ScreenId == screenId;
        }
    }
}
