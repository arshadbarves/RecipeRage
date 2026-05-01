using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Controller for a single UI screen - manages the visual element lifecycle
    /// </summary>
    public class UIScreenController
    {
        public Type ScreenType { get; }
        public UIScreenPriority Priority { get; }
        public UIScreenCategory Category { get; }
        public VisualElement Container { get; private set; }
        public TemplateContainer TemplateContainer { get; private set; }

        private readonly VisualTreeAsset _template;
        private readonly VisualElement _layerRoot;

        public event Action<UIScreenController> OnShown;
        public event Action<UIScreenController> OnHidden;
        public event Action<UIScreenController> OnAnimationStarted;
        public event Action<UIScreenController> OnAnimationCompleted;

        public UIScreenController(Type screenType, UIScreenPriority priority, UIScreenCategory category, VisualTreeAsset template, VisualElement layerRoot)
        {
            ScreenType = screenType;
            Priority = priority;
            Category = category;
            _template = template;
            _layerRoot = layerRoot;

            CreateContainer();
        }

        private void CreateContainer()
        {
            Container = new VisualElement();
            Container.name = $"screen-container-{ScreenType.Name}";
            Container.style.position = Position.Absolute;
            Container.style.left = 0;
            Container.style.top = 0;
            Container.style.right = 0;
            Container.style.bottom = 0;
            Container.style.width = new StyleLength(Length.Percent(100));
            Container.style.height = new StyleLength(Length.Percent(100));
            Container.style.display = DisplayStyle.None;
            Container.pickingMode = PickingMode.Ignore;

            if (_template != null)
            {
                TemplateContainer = _template.CloneTree();
                TemplateContainer.style.flexGrow = 1;
                Container.Add(TemplateContainer);
            }

            _layerRoot?.Add(Container);
        }

        public T GetElement<T>(string elementName) where T : VisualElement
        {
            return TemplateContainer?.Q<T>(elementName) ?? Container?.Q<T>(elementName);
        }

        public T GetElement<T>(string elementName, string className) where T : VisualElement
        {
            return TemplateContainer?.Q<T>(elementName, className) ?? Container?.Q<T>(elementName, className);
        }

        public UQueryBuilder<T>? GetElements<T>() where T : VisualElement
        {
            return TemplateContainer?.Query<T>();
        }

        public void Show(Action<VisualElement, float, Action> animateAction, float duration, bool animate, Action onComplete)
        {
            Container.style.display = DisplayStyle.Flex;
            OnAnimationStarted?.Invoke(this);

            if (animate && animateAction != null)
            {
                animateAction(Container, duration, () =>
                {
                    OnAnimationCompleted?.Invoke(this);
                    OnShown?.Invoke(this);
                    onComplete?.Invoke();
                });
            }
            else
            {
                Container.style.opacity = 1;
                OnAnimationCompleted?.Invoke(this);
                OnShown?.Invoke(this);
                onComplete?.Invoke();
            }
        }

        public void Hide(Action<VisualElement, float, Action> animateAction, float duration, bool animate, Action onComplete)
        {
            OnAnimationStarted?.Invoke(this);

            if (animate && animateAction != null)
            {
                animateAction(Container, duration, () =>
                {
                    Container.style.display = DisplayStyle.None;
                    OnAnimationCompleted?.Invoke(this);
                    OnHidden?.Invoke(this);
                    onComplete?.Invoke();
                });
            }
            else
            {
                Container.style.display = DisplayStyle.None;
                OnAnimationCompleted?.Invoke(this);
                OnHidden?.Invoke(this);
                onComplete?.Invoke();
            }
        }
    }
}
