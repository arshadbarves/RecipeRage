using RecipeRage.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example demonstrating usage of the UI Toolkit animation system.
    /// Attach this to a GameObject that has a UIDocument component.
    /// </summary>
    public class UIAnimationExample : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private string _buttonName = "PlayButton";
        [SerializeField] private string _panelName = "Panel";
        [SerializeField] private string _listItemClass = "list-item";

        private Button _button;
        private VisualElement _panel;

        private void OnEnable()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
                if (_document == null)
                {
                    Debug.LogError("UIDocument component not found");
                    return;
                }
            }

            // Wait for UI document to be ready
            _document.rootVisualElement.schedule.Execute(() =>
            {
                InitializeUI();
            });
        }

        private void InitializeUI()
        {
            var root = _document.rootVisualElement;

            // Get references to UI elements
            _button = root.Q<Button>(_buttonName);
            _panel = root.Q(_panelName);

            if (_button != null)
            {
                // Register button click event
                _button.clicked += OnButtonClicked;

                // Create a simple hover animation for the button
                _button.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    _button.Animate()
                        .Scale(1.1f, 200, UIEasingFunctions.EaseOutBack)
                        .Play();
                });

                _button.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    _button.Animate()
                        .Scale(1.0f, 200, UIEasingFunctions.EaseOutBack)
                        .Play();
                });
            }
            else
            {
                Debug.LogWarning($"Button with name '{_buttonName}' not found");
            }

            if (_panel == null)
            {
                Debug.LogWarning($"Panel with name '{_panelName}' not found");
            }
        }

        private void OnButtonClicked()
        {
            if (_panel == null) return;

            // Stop any existing animations on the panel
            _panel.StopAnimations();

            // Animate the panel with a sequence of animations
            UIAnimationController.Instance.Create(_panel)
                // First, fade out
                .Fade(0, 300, UIEasingFunctions.EaseInOutQuad)
                // Then, move and scale while hidden
                .Sequence(() =>
                {
                    // Set position off-screen
                    _panel.transform.position = new Vector3(200, 0, 0);

                    // Create a sequence that:
                    // 1. Moves the panel back in
                    // 2. Fades it in
                    // 3. Adds a slight bounce effect
                    UIAnimationController.Instance.Create(_panel)
                        .Move(0, 0, 500, UIEasingFunctions.EaseOutBack)
                        .Fade(1, 400, UIEasingFunctions.EaseInOutQuad)
                        .Scale(1.05f, 200, UIEasingFunctions.EaseOutQuad)
                        .Sequence(() =>
                        {
                            _panel.Animate()
                                .Scale(1.0f, 200, UIEasingFunctions.EaseInOutQuad)
                                .Play();
                        })
                        .Play();
                })
                .Play();

            // Animate list items with staggered delay
            AnimateListItems();
        }

        private void AnimateListItems()
        {
            // Get all elements with the list-item class
            var listItems = _document.rootVisualElement.Query().Class(_listItemClass).ToList();

            // Animate each item with increasing delay for a staggered effect
            uint delay = 0;
            foreach (var item in listItems)
            {
                item.style.opacity = 0;
                item.transform.position = new Vector3(50, 0, 0);

                UIAnimationController.Instance.Create(item)
                    .Delay(delay)
                    .Fade(1, 300, UIEasingFunctions.EaseOutQuad)
                    .Move(0, 0, 400, UIEasingFunctions.EaseOutBack)
                    .Play();

                delay += 50; // Add 50ms delay for each item
            }
        }

        /// <summary>
        /// Example of how to create a reusable animation preset
        /// </summary>
        public static string CreatePulseAnimation(VisualElement element, uint duration = 400)
        {
            return UIAnimationController.Instance.Create(element)
                .Scale(1.1f, duration / 2, UIEasingFunctions.EaseOutQuad)
                .Sequence(() =>
                {
                    element.Animate()
                        .Scale(1.0f, duration / 2, UIEasingFunctions.EaseInQuad)
                        .Play();
                })
                .Play();
        }
    }
}