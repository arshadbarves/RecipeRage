using System;
using RecipeRage.Core.Patterns;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Modules.Auth.UI
{
    /// <summary>
    /// Factory class for creating authentication UI components.
    /// This reduces the need for MonoBehaviour inheritance and provides a centralized way to create UI.
    /// Complexity Rating: 2
    /// </summary>
    public static class AuthUIFactory
    {
        // Cache UI Document asset references
        private static UIDocument _cachedDocument;
        private static VisualTreeAsset _cachedLoginUIAsset;

        /// <summary>
        /// Create or get a UI Document for authentication UI
        /// </summary>
        /// <param name="parent"> Parent transform to attach to </param>
        /// <param name="sortOrder"> UI document sort order </param>
        /// <returns> UI Document instance </returns>
        public static UIDocument GetOrCreateUIDocument(Transform parent = null, int sortOrder = 0)
        {
            if (_cachedDocument != null) return _cachedDocument;

            // Create a new game object for the UI Document
            var uiObject = new GameObject("AuthUI");

            // Set parent if provided
            if (parent != null) uiObject.transform.SetParent(parent, false);

            // Add UI Document component
            _cachedDocument = uiObject.AddComponent<UIDocument>();
            _cachedDocument.sortingOrder = sortOrder;

            // Load visual tree asset if needed
            if (_cachedLoginUIAsset == null)
            {
                _cachedLoginUIAsset = Resources.Load<VisualTreeAsset>("UI/Auth/AuthLoginUI");

                if (_cachedLoginUIAsset == null)
                    LogHelper.Error("AuthUIFactory",
                        "Could not load AuthLoginUI asset from Resources. Make sure it's in a Resources folder.");
            }

            // Assign visual tree asset to document
            if (_cachedLoginUIAsset != null) _cachedDocument.visualTreeAsset = _cachedLoginUIAsset;

            return _cachedDocument;
        }

        /// <summary>
        /// Create a login UI controller that doesn't inherit from MonoBehaviour
        /// </summary>
        /// <param name="parent"> Parent transform to attach to </param>
        /// <param name="onAuthComplete"> Callback for when authentication is complete </param>
        /// <returns> Login controller </returns>
        public static AuthLoginController CreateLoginUI(Transform parent = null, Action<bool> onAuthComplete = null)
        {
            // Get or create UI Document
            var document = GetOrCreateUIDocument(parent);

            // Create controller for the UI
            var controller = new AuthLoginController(document);

            // Show login UI with callback
            controller.ShowLoginUI(onAuthComplete);

            return controller;
        }

        /// <summary>
        /// Show an existing login UI instance
        /// </summary>
        /// <param name="onAuthComplete"> Callback for authentication completion </param>
        public static void ShowLoginUI(Action<bool> onAuthComplete = null)
        {
            // Create UI if it doesn't exist
            if (_cachedDocument == null)
            {
                CreateLoginUI(null, onAuthComplete);
                return;
            }

            // Get controller for document
            var controller = new AuthLoginController(_cachedDocument);
            controller.ShowLoginUI(onAuthComplete);
        }

        /// <summary>
        /// Hide the login UI if it exists
        /// </summary>
        public static void HideLoginUI()
        {
            if (_cachedDocument != null)
            {
                // Get controller and hide UI
                var controller = new AuthLoginController(_cachedDocument);
                controller.HideLoginUI();
            }
        }
    }

    /// <summary>
    /// Authentication login UI controller that doesn't inherit from MonoBehaviour.
    /// Controls a UI Document with authentication UI elements.
    /// Complexity Rating: 3
    /// </summary>
    public class AuthLoginController
    {
        // Auth service reference
        private readonly IAuthService _authService;

        // UI Document
        private readonly UIDocument _document;
        private Button _eosDeviceButton;
        private Label _errorLabel;
        private VisualElement _errorPanel;
        private Button _facebookButton;
        private Button _guestButton;
        private float _lastRotationTime;
        private Label _loadingLabel;
        private VisualElement _loadingPanel;
        private VisualElement _loginPanel;

        // Callback for when authentication is complete
        private Action<bool> _onAuthComplete;

        // UI Elements
        private VisualElement _root;
        private VisualElement _spinner;

        // Animation variables
        private int _spinnerRotation;
        private Button _tryAgainButton;

        /// <summary>
        /// Constructor for the auth login controller
        /// </summary>
        /// <param name="document"> UI Document to control </param>
        public AuthLoginController(UIDocument document)
        {
            // Get auth service from service locator
            if (!ServiceLocator.Instance.TryGet<IAuthService>(out var authService))
            {
                LogHelper.Error("AuthLoginController", "Failed to get auth service from service locator");
                return;
            }

            _authService = authService;
            _document = document;

            // Initialize UI elements
            InitializeUI();
        }

        /// <summary>
        /// Initialize the UI elements
        /// </summary>
        private void InitializeUI()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                LogHelper.Error("AuthLoginController", "UI Document or root visual element is null");
                return;
            }

            // Get UI elements
            _root = _document.rootVisualElement.Q("Root");

            if (_root == null)
            {
                LogHelper.Error("AuthLoginController", "Root element 'Root' not found in UI Document");
                return;
            }

            _loginPanel = _root.Q("LoginPanel");
            _loadingPanel = _root.Q("LoadingPanel");
            _errorPanel = _root.Q("ErrorPanel");
            _loadingLabel = _root.Q<Label>("LoadingLabel");
            _errorLabel = _root.Q<Label>("ErrorLabel");
            _guestButton = _root.Q<Button>("GuestButton");
            _facebookButton = _root.Q<Button>("FacebookButton");
            _eosDeviceButton = _root.Q<Button>("EOSDeviceButton");
            _tryAgainButton = _root.Q<Button>("TryAgainButton");
            _spinner = _root.Q("Spinner");

            // Set up button click handlers
            _guestButton?.RegisterCallback<ClickEvent>(evt => HandleProviderButtonClick("Guest"));
            _facebookButton?.RegisterCallback<ClickEvent>(evt => HandleProviderButtonClick("Facebook"));
            _eosDeviceButton?.RegisterCallback<ClickEvent>(evt => HandleProviderButtonClick("EOSDevice"));
            _tryAgainButton?.RegisterCallback<ClickEvent>(evt => ShowLoginPanel());

            // Initialize UI state
            ShowLoginPanel(false);
            ShowLoadingPanel(false);
            ShowErrorPanel(false);

            // Start spinner animation
            StartSpinnerAnimation();
        }

        /// <summary>
        /// Start the spinner animation using C# instead of experimental UIToolkit animation
        /// </summary>
        private void StartSpinnerAnimation()
        {
            if (_spinner == null) return;

            // Register for render event to update spinner rotation
            _spinner.RegisterCallback<GeometryChangedEvent>(OnSpinnerUpdate);
        }

        /// <summary>
        /// Update spinner rotation on render
        /// </summary>
        private void OnSpinnerUpdate(GeometryChangedEvent evt)
        {
            // Update rotation every 16ms (roughly 60fps)
            if (Time.time - _lastRotationTime > 0.016f)
            {
                _spinnerRotation = (_spinnerRotation + 6) % 360; // 6 degrees per frame = roughly full rotation in 1s
                _spinner.style.rotate = new Rotate(_spinnerRotation);
                _lastRotationTime = Time.time;
            }
        }

        /// <summary>
        /// Show the login UI
        /// </summary>
        /// <param name="onAuthComplete"> Callback when auth is complete </param>
        public void ShowLoginUI(Action<bool> onAuthComplete = null)
        {
            _onAuthComplete = onAuthComplete;

            // If auth service is not available, just show the panel
            if (_authService == null)
            {
                ShowLoginPanel();
                return;
            }

            // Check if user is already logged in
            if (_authService.CurrentUser != null)
            {
                LogHelper.Info("AuthLoginController", $"User {_authService.CurrentUser.DisplayName} is already logged in");
                _onAuthComplete?.Invoke(true);
                return;
            }

            // Show login panel
            ShowLoginPanel();
        }

        /// <summary>
        /// Hide the login UI
        /// </summary>
        public void HideLoginUI()
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Show the login panel
        /// </summary>
        /// <param name="show"> Whether to show or hide </param>
        private void ShowLoginPanel(bool show = true)
        {
            if (_root == null) return;

            _root.style.display = DisplayStyle.Flex;

            if (_loginPanel != null) _loginPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (_loadingPanel != null) _loadingPanel.style.display = DisplayStyle.None;

            if (_errorPanel != null) _errorPanel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Show the loading panel
        /// </summary>
        /// <param name="show"> Whether to show or hide </param>
        /// <param name="message"> Message to display </param>
        private void ShowLoadingPanel(bool show = true, string message = "Loading...")
        {
            if (_root == null) return;

            _root.style.display = DisplayStyle.Flex;

            if (_loginPanel != null) _loginPanel.style.display = DisplayStyle.None;

            if (_loadingPanel != null) _loadingPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (_errorPanel != null) _errorPanel.style.display = DisplayStyle.None;

            if (_loadingLabel != null) _loadingLabel.text = message;
        }

        /// <summary>
        /// Show the error panel
        /// </summary>
        /// <param name="show"> Whether to show or hide </param>
        /// <param name="error"> Error message to display </param>
        private void ShowErrorPanel(bool show = true, string error = "An error occurred")
        {
            if (_root == null) return;

            _root.style.display = DisplayStyle.Flex;

            if (_loginPanel != null) _loginPanel.style.display = DisplayStyle.None;

            if (_loadingPanel != null) _loadingPanel.style.display = DisplayStyle.None;

            if (_errorPanel != null) _errorPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (_errorLabel != null) _errorLabel.text = error;
        }

        /// <summary>
        /// Handle click on a provider login button
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        private void HandleProviderButtonClick(string providerName)
        {
            if (_authService == null)
            {
                LogHelper.Error("AuthLoginController", "Cannot handle button click, auth service is null");
                return;
            }

            LogHelper.Info("AuthLoginController", $"Login button clicked for provider '{providerName}'");

            // Show loading panel
            ShowLoadingPanel(true, $"Signing in with {providerName}...");

            // Authenticate with the selected provider
            _authService.SignInWithProvider(
                providerName,
                user =>
                {
                    LogHelper.Info("AuthLoginController", $"Authentication successful for user {user.DisplayName}");

                    // Hide UI
                    HideLoginUI();

                    // Save as preferred provider
                    _authService.SaveCurrentProviderAsPreferred();

                    // Call the completion callback
                    _onAuthComplete?.Invoke(true);
                },
                error =>
                {
                    LogHelper.Error("AuthLoginController", $"Authentication failed - {error}");

                    ShowErrorPanel(true, error);

                    // Call the completion callback
                    _onAuthComplete?.Invoke(false);
                }
            );
        }
    }
}