using System;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Base class for all UI screens in the game
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        /// <summary>
        /// The UI Document component
        /// </summary>
        protected UIDocument _uiDocument;

        /// <summary>
        /// The root visual element
        /// </summary>
        protected VisualElement _root;

        /// <summary>
        /// The main container for this screen
        /// </summary>
        protected VisualElement _container;

        /// <summary>
        /// Whether the screen is currently visible
        /// </summary>
        public bool IsVisible { get; protected set; }

        /// <summary>
        /// Event triggered when the screen is shown
        /// </summary>
        public event Action OnScreenShown;

        /// <summary>
        /// Event triggered when the screen is hidden
        /// </summary>
        public event Action OnScreenHidden;

        /// <summary>
        /// Initialize the screen
        /// </summary>
        protected virtual void Awake()
        {
            // Get UI Document component
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError($"[{GetType().Name}] UIDocument component not found");
                return;
            }

            // Initialize UI when the document is ready
            _uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Called when the UI geometry is initialized
        /// </summary>
        protected virtual void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Unregister the callback to ensure it's only called once
            _uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Get the root element
            _root = _uiDocument.rootVisualElement;

            // Get the main container
            _container = _root.Q<VisualElement>("screen-container");
            if (_container == null)
            {
                Debug.LogError($"[{GetType().Name}] Container element 'screen-container' not found");
                return;
            }

            // Initialize the screen
            InitializeScreen();

            // Hide the screen by default
            Hide(false);
        }

        /// <summary>
        /// Initialize the screen elements and event handlers
        /// </summary>
        protected abstract void InitializeScreen();

        /// <summary>
        /// Show the screen with animation
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public virtual void Show(bool animate = true)
        {
            if (_container == null) return;

            if (animate)
            {
                // Reset container state
                _container.style.opacity = 0;
                _container.style.display = DisplayStyle.Flex;

                // Animate in using Unity's native system
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _container,
                    0f,
                    1f,
                    300,
                    0,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseOut,
                    () =>
                    {
                        IsVisible = true;
                        OnScreenShown?.Invoke();
                    }
                );
            }
            else
            {
                _container.style.opacity = 1;
                _container.style.display = DisplayStyle.Flex;
                IsVisible = true;
                OnScreenShown?.Invoke();
            }
        }

        /// <summary>
        /// Hide the screen with animation
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public virtual void Hide(bool animate = true)
        {
            if (_container == null) return;

            if (animate)
            {
                UnityNativeUIAnimationSystem.AnimateOpacity(
                    _container,
                    1f,
                    0f,
                    300,
                    0,
                    UnityNativeUIAnimationSystem.EasingCurve.EaseOut,
                    () =>
                    {
                        _container.style.display = DisplayStyle.None;
                        IsVisible = false;
                        OnScreenHidden?.Invoke();
                    }
                );
            }
            else
            {
                _container.style.opacity = 0;
                _container.style.display = DisplayStyle.None;
                IsVisible = false;
                OnScreenHidden?.Invoke();
            }
        }

        /// <summary>
        /// Toggle the screen visibility
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public virtual void Toggle(bool animate = true)
        {
            if (IsVisible)
                Hide(animate);
            else
                Show(animate);
        }
    }
}
