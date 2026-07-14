using System;
using KitchenClash.Application.Services;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using Playcenter.Shell;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// UI Toolkit screen host implementing <see cref="IUIService"/>.
    /// Responsibilities: document/layer setup, screen resolve, category navigation, toast host.
    /// Stack history lives in <see cref="IUIScreenStackManager"/>; transitions in screen controllers + <see cref="UITransitionHandler"/>.
    /// </summary>
    public partial class UIService : IUIService, IStartable, ITickable, IDisposable
    {
        private static readonly UIScreenCategory[] LayerOrder =
        {
            UIScreenCategory.Screen,
            UIScreenCategory.HUD,
            UIScreenCategory.Popup,
            UIScreenCategory.Modal,
            UIScreenCategory.Overlay,
            UIScreenCategory.System,
            UIScreenCategory.Toast
        };

        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<UIScreenCategory, VisualElement> _layerRoots = new();
        private readonly Dictionary<Type, UIScreenController> _controllers = new();
        private readonly Dictionary<Type, BaseUIScreen> _screens = new();
        private readonly IUIScreenStackManager _stackManager;
        private readonly VContainer.IObjectResolver _container;
        private VContainer.IObjectResolver _currentScope;

        public event Action<Type> OnScreenShown;
        public event Action<Type> OnScreenHidden;
        public event Action OnAllScreensHidden;

        private bool _isInitialized;

        public UIService(
            VContainer.IObjectResolver container,
            UIDocument uiDocument,
            IUIScreenStackManager stackManager)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _uiDocument = uiDocument;
            _stackManager = stackManager ?? throw new ArgumentNullException(nameof(stackManager));
        }

        public bool IsInitialized => _isInitialized && _root != null;

        public void SetCurrentScope(VContainer.IObjectResolver scope)
        {
            _currentScope = scope;
        }

        public void Start()
        {
            if (_isInitialized) return;

            if (_uiDocument == null)
            {
                GameLogger.LogError("UIDocument is null - cannot initialize UIService");
                return;
            }

            InitializeWithDocument(_uiDocument);
            InitializeScreens();
        }

        public void InitializeWithDocument(UIDocument uiDocument)
        {
            if (_isInitialized) return;

            _uiDocument = uiDocument;
            UIScreenRegistry.Initialize();
            SetupUIDocument();
        }

        public void InitializeScreens()
        {
            if (_isInitialized) return;

            if (_root == null)
            {
                GameLogger.LogError("Root element not ready.");
                return;
            }

            CreateScreenControllers();
            _isInitialized = true;
        }

        private void SetupUIDocument()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            OnUIDocumentReady();
        }

        private void OnUIDocumentReady()
        {
            _root = _uiDocument.rootVisualElement;
            _root.name = "ui-root";
            _root.style.width = Length.Percent(100);
            _root.style.height = Length.Percent(100);
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.top = 0;
            _root.style.right = 0;
            _root.style.bottom = 0;
            _root.AddToClassList("ui-root");

            CreateLayerRoots();
        }

        private void CreateLayerRoots()
        {
            _layerRoots.Clear();

            foreach (UIScreenCategory category in LayerOrder)
            {
                VisualElement layer = new VisualElement();
                layer.name = $"ui-layer-{category.ToString().ToLowerInvariant()}";
                layer.AddToClassList("ui-layer");
                layer.pickingMode = PickingMode.Ignore;
                layer.style.position = Position.Absolute;
                layer.style.left = 0;
                layer.style.top = 0;
                layer.style.right = 0;
                layer.style.bottom = 0;
                layer.style.width = Length.Percent(100);
                layer.style.height = Length.Percent(100);

                _root.Add(layer);
                _layerRoots[category] = layer;
            }
        }

        private VisualElement GetLayerRoot(UIScreenCategory category)
        {
            if (_layerRoots.TryGetValue(category, out VisualElement layer)) return layer;
            return _root;
        }

        private void CreateScreenControllers()
        {
            foreach (Type screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreenController(screenType);
            }
        }

        private void CreateScreenController(Type screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null) return;

            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);
            VisualElement layerRoot = GetLayerRoot(attribute.Category);

            var controller = new UIScreenController(screenType, attribute.Priority, attribute.Category, template, layerRoot);
            _controllers[screenType] = controller;
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath)) return null;

            string resourcePath = $"UI/Templates/{templatePath}";
            return Resources.Load<VisualTreeAsset>(resourcePath);
        }

        public void Dispose()
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                screen.Dispose();
            }

            _screens.Clear();
            _controllers.Clear();
            _layerRoots.Clear();
            _stackManager.ClearAll();
            _isInitialized = false;
        }

        public void Update(float deltaTime)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.IsVisible))
            {
                screen.Update(deltaTime);
            }
        }

        public void Tick()
        {
            Update(Time.deltaTime);
        }
    }
}
