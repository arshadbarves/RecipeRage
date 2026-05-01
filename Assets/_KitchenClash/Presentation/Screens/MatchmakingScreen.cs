using KitchenClash.Application.Models;
using System.Collections.Generic;
using KitchenClash.Domain;
using System;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;
using KitchenClash.Application.Services;
using KitchenClash.Presentation.ViewModels;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/MatchmakingViewTemplate")]
    public class MatchmakingScreen : BaseUIScreen
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
        private float _rotationSpeed = 60f;
        private float _currentRotation = 0f;

        // Pulse Animation State
        private float _pulseTimer = 0f;
        private const float PULSE_DURATION = 2f;
        private Action<string> _statusBinding;
        private Action<string> _playerCountBinding;
        private Action<string> _timerBinding;
        private Action<bool> _matchFoundBinding;
        private Action<string> _gameModeBinding;
        private Action<string> _mapNameBinding;
        private bool _isViewModelBound;

        protected override void OnInitialize()
        {
            _modeLabel = GetElement<Label>("ModeLabel");
            _titleLabel = GetElement<Label>("TitleLabel");
            _slotsRing = GetElement<VisualElement>("SlotsRing");
            _pulseWave = GetElement<VisualElement>("PulseWave");
            _currentCountLabel = GetElement<Label>("CurrentCountLabel");
            _maxCountLabel = GetElement<Label>("MaxCountLabel");
            _statusSubLabel = GetElement<Label>("StatusSubLabel");
            _timerLabel = GetElement<Label>("TimerLabel");
            _cancelButton = GetElement<Button>("CancelButton");

            UpdateStaticLocalization();

            if (_cancelButton != null) _cancelButton.clicked += OnCancelClicked;

            CreateSlots();
            BindViewModel();
        }

        protected override void OnShow()
        {
            _viewModel?.OnShow();
        }

        protected override void OnHide()
        {
            _viewModel?.OnHide();
        }

        protected override void OnDispose()
        {
            if (_cancelButton != null) _cancelButton.clicked -= OnCancelClicked;
            UnbindViewModel();
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
            Debug.Log("[MatchmakingScreen] Cancel button clicked");
            _viewModel?.CancelMatchmaking();
        }

        private void UpdateStatus(string status)
        {
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

                _rotationSpeed = 180f;
            }
            else
            {
                 if (_statusSubLabel != null && _localization != null)
                {
                    _statusSubLabel.text = _localization.GetText("matchmaking_players_found_sub");
                    _statusSubLabel.style.color = new StyleColor(new Color(0.53f, 0.53f, 0.53f));
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
                tick.style.rotate = new Rotate(new Angle(i * degreesPerSlot, AngleUnit.Degree));

                _slotsRing.Add(tick);
                _slotTicks.Add(tick);
            }

            UpdateSlotsVisual(1);
        }

        private void BindViewModel()
        {
            if (_viewModel == null || _isViewModelBound) return;

            _statusBinding = UpdateStatus;
            _playerCountBinding = UpdatePlayerCount;
            _timerBinding = OnTimerUpdate;
            _matchFoundBinding = OnMatchFoundChanged;
            _gameModeBinding = UpdateGameMode;
            _mapNameBinding = UpdateMapName;

            _viewModel.StatusText.Bind(_statusBinding);
            _viewModel.PlayerCountText.Bind(_playerCountBinding);
            _viewModel.SearchTimeText.Bind(_timerBinding);
            _viewModel.IsMatchFound.Bind(_matchFoundBinding);
            _viewModel.GameModeText.Bind(_gameModeBinding);
            _viewModel.MapNameText.Bind(_mapNameBinding);
            _isViewModelBound = true;
        }

        private void UnbindViewModel()
        {
            if (_viewModel == null || !_isViewModelBound) return;

            _viewModel.StatusText.Unbind(_statusBinding);
            _viewModel.PlayerCountText.Unbind(_playerCountBinding);
            _viewModel.SearchTimeText.Unbind(_timerBinding);
            _viewModel.IsMatchFound.Unbind(_matchFoundBinding);
            _viewModel.GameModeText.Unbind(_gameModeBinding);
            _viewModel.MapNameText.Unbind(_mapNameBinding);
            _isViewModelBound = false;
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

            if (_pulseWave != null)
            {
                _pulseTimer += deltaTime;
                if (_pulseTimer > PULSE_DURATION) _pulseTimer = 0f;

                float progress = _pulseTimer / PULSE_DURATION;
                float scale = Mathf.Lerp(1f, 2.5f, progress);
                float opacity = Mathf.Lerp(0.5f, 0f, progress);

                _pulseWave.style.scale = new Scale(Vector3.one * scale);
                _pulseWave.style.opacity = opacity;
            }
        }
    }
}
