using System.Collections.Generic;
using RecipeRage.Modules.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// Base class for all Friends UI components
    /// Complexity Rating: 2
    /// </summary>
    public abstract class FriendsUIComponent : MonoBehaviour
    {
        [SerializeField] protected UIDocument _document;
        [SerializeField] protected VisualTreeAsset _templateAsset;
        protected bool _isInitialized;

        protected VisualElement _root;

        /// <summary>
        /// Called when the component is enabled
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!_isInitialized)
                Initialize();
        }

        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Initialize the component
        /// </summary>
        public virtual void Initialize()
        {
            if (_isInitialized)
                return;

            if (_document == null)
            {
                _document = GetComponent<UIDocument>();

                if (_document == null)
                {
                    LogHelper.Error("FriendsUI", $"No UIDocument found on {gameObject.name}");
                    return;
                }
            }

            _root = _document.rootVisualElement;

            if (_root == null)
            {
                LogHelper.Error("FriendsUI", $"No root visual element found on {gameObject.name}");
                return;
            }

            RegisterCallbacks();

            _isInitialized = true;
            LogHelper.Debug("FriendsUI", $"Initialized {GetType().Name}");
        }

        /// <summary>
        /// Register UI callbacks
        /// </summary>
        protected virtual void RegisterCallbacks()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Show the UI component
        /// </summary>
        public virtual void Show()
        {
            if (!_isInitialized)
                Initialize();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                OnShow();
            }
        }

        /// <summary>
        /// Hide the UI component
        /// </summary>
        public virtual void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
                OnHide();
            }
        }

        /// <summary>
        /// Called when the component is shown
        /// </summary>
        protected virtual void OnShow()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the component is hidden
        /// </summary>
        protected virtual void OnHide()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Find a child element by name
        /// </summary>
        protected T FindElement<T>(string name) where T : VisualElement
        {
            return _root?.Q<T>(name);
        }

        /// <summary>
        /// Find child elements by class
        /// </summary>
        protected List<T> FindElementsByClass<T>(string className) where T : VisualElement
        {
            return _root?.Query<T>(className: className).ToList();
        }

        /// <summary>
        /// Create a visual element from a template
        /// </summary>
        protected VisualElement CreateElementFromTemplate()
        {
            if (_templateAsset == null)
            {
                LogHelper.Error("FriendsUI", $"No template asset assigned for {GetType().Name}");
                return null;
            }

            return _templateAsset.Instantiate();
        }
    }
}