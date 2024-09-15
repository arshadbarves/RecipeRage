using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace GameSystem.UI.Base
{
    public interface IUIPanel
    {
        public VisualElement Root { get; set; }

        Task InitializeAsync(UIPanelConfig config);

        void Show(Action onComplete = null);

        void Hide(Action onComplete = null);

        void UpdatePanel();

        Task CleanupAsync();
    }
}