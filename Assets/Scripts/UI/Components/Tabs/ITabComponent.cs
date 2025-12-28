using UnityEngine.UIElements;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Interface for modular tab content components.
    /// Follows SOLID: Interface Segregation and Dependency Inversion.
    /// </summary>
    public interface ITabComponent
    {
        string TabId { get; }
        VisualElement Root { get; }
        void Initialize(VisualElement root);
        void OnShow();
        void OnHide();
        void Update(float deltaTime);
        void Dispose();
    }
}
