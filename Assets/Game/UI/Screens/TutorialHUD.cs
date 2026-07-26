using Playcenter;
using Playcenter.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// UI Toolkit tutorial HUD: instruction panel (instruction + button hint +
    /// station color), step progress (X/9), numeric progress (chop/cook), and
    /// a positioned arrow over the current target station. Driven by TutorialController.
    /// </summary>
    [UIScreen]
    public sealed class TutorialHUD : BaseUIScreen, ITutorialHUD
    {
        private Label _instructionText;
        private Label _buttonHint;
        private VisualElement _colorDot;
        private Label _stepCount;
        private VisualElement _numericProgress;
        private VisualElement _progressFill;
        private Label _progressLabel;
        private VisualElement _arrow;
        private Label _arrowText;

        private Transform _arrowTarget;
        private Camera _camera;

        protected override void OnShow()
        {
            _instructionText = Root.Q<Label>("instruction-text");
            _buttonHint = Root.Q<Label>("button-hint");
            _colorDot = Root.Q<VisualElement>("station-color-dot");
            _stepCount = Root.Q<Label>("step-count");
            _numericProgress = Root.Q<VisualElement>("numeric-progress");
            _progressFill = Root.Q<VisualElement>("progress-fill");
            _progressLabel = Root.Q<Label>("progress-label");
            _arrow = Root.Q<VisualElement>("arrow-indicator");
            _arrowText = Root.Q<Label>(className: "tutorial-arrow-text");
            _camera = Camera.main;
        }

        public void ShowStep(int index, int total, TutorialStep step)
        {
            _instructionText.text = step.Instruction;
            _buttonHint.text = step.ButtonHint ?? string.Empty;

            if (ColorUtility.TryParseHtmlString(step.StationColorHex, out var color))
            {
                _colorDot.style.backgroundColor = new StyleColor(color);
                _arrowText.style.color = new StyleColor(color);
            }

            _stepCount.text = $"{index + 1} / {total}";

            _numericProgress.style.display = step.TrackProgress ? DisplayStyle.Flex : DisplayStyle.None;
            SetProgress(0f, step.TrackProgress ? "0" : string.Empty);

            _arrowTarget = step.HighlightTarget;
            _arrow.style.display = _arrowTarget != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetProgress(float progress01, string label)
        {
            _progressFill.style.width = new Length(Mathf.Clamp01(progress01) * 100f, LengthUnit.Percent);
            if (!string.IsNullOrEmpty(label))
            {
                _progressLabel.text = label;
            }
        }

        private void Update()
        {
            // Position arrow over the target station (world → screen)
            if (_arrowTarget == null || _camera == null)
            {
                return;
            }

            var screenPos = _camera.WorldToScreenPoint(_arrowTarget.position + Vector3.up * 1.5f);
            // screenPos is bottom-left origin; UI Toolkit is top-left origin
            var root = _arrow.parent;
            if (root != null)
            {
                var panelHeight = root.resolvedStyle.height;
                var x = screenPos.x;
                var y = panelHeight - screenPos.y; // flip Y
                _arrow.style.left = x - 32f;
                _arrow.style.top = y - 32f;
            }

            // Gentle bounce
            var bounce = Mathf.Sin(Time.time * 4f) * 6f;
            _arrow.style.translate = new StyleTranslate(new Translate(0, bounce));
        }
    }
}
