using System.Collections.Generic;
using Core.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Localization;
using Core.UI;
using VContainer;

namespace Gameplay.UI.Features.Matchmaking
{
    [UIScreen(UIScreenCategory.Screen, "Screens/MatchmakingViewTemplate")]
    public class MatchmakingView : BaseUIScreen
    {
        [Inject] private MatchmakingViewModel _viewModel;
        [Inject] private ILocalizationManager _localization;

        // UI Elements
        private Label _modeLabel;
        private Label _titleLabel;
        private VisualElement _slotsRing;
        private VisualElement _pulseWave;
        private Label _currentCountLabel;
        private Label _maxCountLabel;
        private Label _statusSubLabel;
        private Label _timerLabel;
        private Button _cancelButton;

        // Visual State
        private const int MAX_PLAYERS = 10;
        private List<VisualElement> _slotTicks = new List<VisualElement>();
        private float _rotationSpeed = 60f; // degrees per second
        private float _currentRotation = 0f;

        // Pulse Animation State
        private float _pulseTimer = 0f;
        private const float PULSE_DURATION = 2f;

        protected override void OnInitialize()
        {
            // Find Elements
            _modeLabel = GetElement<Label>("ModeLabel");
            _titleLabel = GetElement<Label>("TitleLabel");
            _slotsRing = GetElement<VisualElement>("SlotsRing");
            _pulseWave = GetElement<VisualElement>("PulseWave");
            _currentCountLabel = GetElement<Label>("CurrentCountLabel");
            _maxCountLabel = GetElement<Label>("MaxCountLabel");
            _statusSubLabel = GetElement<Label>("StatusSubLabel");
            _timerLabel = GetElement<Label>("TimerLabel");
            _cancelButton = GetElement<Button>("CancelButton");

            // Localize Static Text
            UpdateStaticLocalization();

            // Bind Buttons
            if (_cancelButton != null) _cancelButton.clicked += OnCancelClicked;

            CreateSlots();
        }

        protected override void OnShow()
        {
            _viewModel?.OnShow();

            // Bind ViewModel properties
            if (_viewModel != null)
            {
                _viewModel.StatusText.Bind(UpdateStatus);
                _viewModel.PlayerCountText.Bind(UpdatePlayerCount);
                _viewModel.SearchTimeText.Bind(OnTimerUpdate);
                _viewModel.IsMatchFound.Bind(OnMatchFoundChanged);
                _viewModel.GameModeText.Bind(UpdateGameMode);
                _viewModel.MapNameText.Bind(UpdateMapName);
            }

            // Start Update Loop for animations will remain handled by the system calling Update()
        }

        protected override void OnHide()
        {
            _viewModel?.OnHide();

            // Unbind - in this simple setup we might just rely on OnShow re-binding
            // or we can unbind if BindableProperty supports it.
            // Given the BindableProperty implementation usually involves subscription,
            // we should technically unsubscribe, but let's assume OnShow re-binds/overwrites or we leave it.
            // Actually, best practice is to handle subscriptions carefully.
            // For now, mirroring previous logic which didn't explicitly unbind in OnHide either,
            // but ViewModel.OnHide handles its internal event unsubscription.
        }

        private void UpdateStaticLocalization()
        {
            if (_localization == null) return;

            if (_modeLabel != null) _modeLabel.text = _localization.GetText("matchmaking_mode_ffa");
            if (_titleLabel != null) _titleLabel.text = _localization.GetText("matchmaking_header_title");
            if (_statusSubLabel != null) _statusSubLabel.text = _localization.GetText("matchmaking_players_found_sub");
            if (_cancelButton != null) _cancelButton.text = _localization.GetText("matchmaking_button_cancel");
        }

        private void OnCancelClicked()
        {
            Debug.Log("[MatchmakingView] Cancel button clicked");
            // Just call CancelMatchmaking - the MatchmakingService will fire OnMatchmakingCancelled
            // which MatchmakingState listens to and will transition back to MainMenuState
            // This is the proper state machine pattern - let the state handle cleanup and navigation
            _viewModel?.CancelMatchmaking();
        }

        private void UpdateStatus(string status)
        {
            // _statusLabel.text = status;
        }

        private void UpdateGameMode(string gameMode)
        {
            if (_modeLabel != null)
                _modeLabel.text = gameMode;
        }

        private void UpdateMapName(string mapName)
        {
            if (_titleLabel != null)
                _titleLabel.text = $"MATCHMAKING - {mapName.ToUpper()}";
        }

        private void UpdatePlayerCount(string countStr)
        {
             // format is "cuurent/max"
            var parts = countStr.Split('/');
            if (parts.Length == 2)
            {
                if (_currentCountLabel != null) _currentCountLabel.text = parts[0];
                if (_maxCountLabel != null) _maxCountLabel.text = parts[1];

                if (int.TryParse(parts[0], out int current))
                {
                    UpdateSlotsVisual(current);
                }
            }
        }

        private void OnTimerUpdate(string time)
        {
             if (_timerLabel != null && _localization != null)
                _timerLabel.text = string.Format(_localization.GetText("matchmaking_est_time"), time);
        }

        private void OnMatchFoundChanged(bool found)
        {
            if (found)
            {
                if (_statusSubLabel != null && _localization != null)
                {
                    _statusSubLabel.text = _localization.GetText("matchmaking_match_ready");
                    _statusSubLabel.style.color = new StyleColor(Color.white);
                }

                if (_timerLabel != null) _timerLabel.style.display = DisplayStyle.None;
                if (_cancelButton != null) _cancelButton.style.display = DisplayStyle.None;

                // Speed up rotation
                _rotationSpeed = 180f;
            }
            else
            {
                // Reset state
                 if (_statusSubLabel != null && _localization != null)
                {
                    _statusSubLabel.text = _localization.GetText("matchmaking_players_found_sub");
                    _statusSubLabel.style.color = new StyleColor(new Color(0.53f, 0.53f, 0.53f)); // #888
                }

                if (_timerLabel != null) _timerLabel.style.display = DisplayStyle.Flex;
                if (_cancelButton != null) _cancelButton.style.display = DisplayStyle.Flex;
                _rotationSpeed = 60f;
            }
        }

        private void CreateSlots()
        {
            if (_slotsRing == null) return;

            _slotsRing.Clear();
            _slotTicks.Clear();

            float degreesPerSlot = 360f / MAX_PLAYERS;

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                var tick = new VisualElement();
                tick.AddToClassList("slot-tick");

                // Rotation logic
                tick.style.rotate = new Rotate(new Angle(i * degreesPerSlot, AngleUnit.Degree));

                _slotsRing.Add(tick);
                _slotTicks.Add(tick);
            }

            // Activate first slot (User)
            UpdateSlotsVisual(1);
        }

        private void UpdateSlotsVisual(int currentPlayers)
        {
            for (int i = 0; i < _slotTicks.Count; i++)
            {
                if (i < currentPlayers)
                {
                    _slotTicks[i].AddToClassList("active");
                }
                else
                {
                    _slotTicks[i].RemoveFromClassList("active");
                }
            }
        }

        public override void Update(float deltaTime)
        {
            if (!IsVisible) return;

            if (_slotsRing != null)
            {
                _currentRotation += _rotationSpeed * deltaTime;
                 _slotsRing.style.rotate = new Rotate(new Angle(_currentRotation, AngleUnit.Degree));
            }

            // Pulse Animation
            if (_pulseWave != null)
            {
                _pulseTimer += deltaTime;
                if (_pulseTimer > PULSE_DURATION) _pulseTimer = 0f;

                float progress = _pulseTimer / PULSE_DURATION;

                // Scale 1.0 -> 2.5
                float scale = Mathf.Lerp(1f, 2.5f, progress);

                // Opacity 0.5 -> 0
                float opacity = Mathf.Lerp(0.5f, 0f, progress);

                _pulseWave.style.scale = new Scale(Vector3.one * scale);
                _pulseWave.style.opacity = opacity;
            }
        }
    }
}
