using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSystem.UI.Base;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace GameSystem.UI
{
    public class UISystem : IGameSystem
    {
        private readonly Dictionary<Type, IUIPanel> _uiPanels = new Dictionary<Type, IUIPanel>();

        private UIPanelConfig[] _uiPanelConfigs;

        [Inject] private IUIFactory _uiFactory;
        [Inject] private UIDocument _uiDocument;
        [Inject] private Canvas _mainCanvas;

        public async Task InitializeAsync()
        {
            _uiPanelConfigs = Resources.LoadAll<UIPanelConfig>("UIPanels");
            foreach (UIPanelConfig config in _uiPanelConfigs)
            {
                await CreateUIPanel(config);
            }
        }

        private async Task CreateUIPanel(UIPanelConfig config)
        {
            if (_uiPanels.ContainsKey(config.PanelType))
            {
                Debug.LogWarning($"Panel of type {config.PanelType} already exists. Skipping creation.");
                return;
            }

            IUIPanel panel = await _uiFactory.CreateUIElementAsync(config);
            _uiPanels.TryAdd(panel.GetType(), panel);
        }

        public void ShowPanel<T>(Action onPanelShown = null) where T : IUIPanel
        {
            if (_uiPanels.TryGetValue(typeof(T), out IUIPanel panel))
            {
                if (panel.Root.parent == null)
                {
                    _uiDocument.rootVisualElement.Add(panel.Root);
                }

                panel.Show(onPanelShown);
            }
        }

        public void HidePanel<T>(Action onPanelHidden = null) where T : IUIPanel
        {
            if (_uiPanels.TryGetValue(typeof(T), out var panel))
            {
                panel.Hide(() =>
                {
                    _uiDocument.rootVisualElement.Remove(panel.Root);
                    onPanelHidden?.Invoke();
                });
            }
        }

        public void Update()
        {
            foreach (var panel in _uiPanels.Values)
            {
                panel.UpdatePanel();
            }
        }

        public async Task CleanupAsync()
        {
            foreach (var panel in _uiPanels.Values)
            {
                await panel.CleanupAsync();
            }

            _uiPanels.Clear();
        }
        
        public Canvas GetCanvas()
        {
            return _mainCanvas;
        }
    }
}