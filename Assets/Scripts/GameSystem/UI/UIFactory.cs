using System;
using System.Threading.Tasks;
using GameSystem.UI.Base;
using UnityEngine;
using VContainer;

namespace GameSystem.UI
{
    public interface IUIFactory
    {
        Task<IUIPanel> CreateUIElementAsync(UIPanelConfig config);
    }

    public class UIFactory : IUIFactory
    {
        private readonly IObjectResolver _container;

        public UIFactory(IObjectResolver container)
        {
            _container = container;
        }

        public async Task<IUIPanel> CreateUIElementAsync(UIPanelConfig config)
        {
            Type panelType = config.PanelType;
            IUIPanel element = (IUIPanel)_container.Resolve(panelType);
            await element.InitializeAsync(config);
            Debug.Log($"UI Element created with type: {element.GetType()}");
            return element;
        }
    }
}