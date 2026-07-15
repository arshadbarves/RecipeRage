using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using Playcenter.GameFlow;
using UnityEngine.UIElements;
using VContainer;
using Playcenter.UI;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Brawl-style match-found beat: mode/map card + load bar.
    /// Flow port drives show/hide and progress; screen is presentation only.
    /// </summary>
    [UIScreen(UIScreenCategory.Overlay, "Screens/MatchIntroScreenTemplate")]
    public class MatchIntroScreen : BaseUIScreen
    {
        [Inject] private IAppFlow _appFlow;

        private Label _statusLabel;
        private Label _modeLabel;
        private Label _mapLabel;
        private VisualElement _loadFill;
        private Label _hintLabel;

        protected override void OnInitialize()
        {
            _statusLabel = GetElement<Label>("status-label");
            _modeLabel = GetElement<Label>("mode-label");
            _mapLabel = GetElement<Label>("map-label");
            _loadFill = GetElement<VisualElement>("load-fill");
            _hintLabel = GetElement<Label>("hint-label");
            TransitionType = UITransitionType.Fade;
        }

        protected override void OnShow()
        {
            base.OnShow();
            ApplyResolvedInfo(_appFlow?.Context?.LastMatchResolved);
            SetProgress(0.08f);
        }

        public void ApplyResolvedInfo(MatchResolvedInfo info)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = info != null && info.FilledWithBots
                    ? "MATCH READY"
                    : "MATCH FOUND";
            }

            if (_modeLabel != null)
            {
                string mode = !string.IsNullOrEmpty(info?.ModeId)
                    ? FormatId(info.ModeId)
                    : "RECIPE RAGE";
                _modeLabel.text = mode;
            }

            if (_mapLabel != null)
            {
                string map = !string.IsNullOrEmpty(info?.MapId)
                    ? FormatId(info.MapId)
                    : "LOADING ARENA";
                _mapLabel.text = map;
            }

            if (_hintLabel != null)
            {
                _hintLabel.text = "GET READY";
            }
        }

        public void SetProgress(float normalized01)
        {
            if (_loadFill == null)
            {
                return;
            }

            float clamped = UnityEngine.Mathf.Clamp01(normalized01);
            _loadFill.style.width = Length.Percent(clamped * 100f);
        }

        public void SetHint(string hint)
        {
            if (_hintLabel != null)
            {
                _hintLabel.text = hint ?? string.Empty;
            }
        }

        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            return id.Replace('_', ' ').Replace('-', ' ').ToUpperInvariant();
        }
    }
}
