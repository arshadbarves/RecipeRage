using KitchenClash.Presentation;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.HUD, "Screens/GameplayHudViewTemplate")]
    public class MatchHUDScreen : BaseUIScreen
    {
        [Inject] private GameplayHudViewModel _viewModel;

        private Label _scoreLabel;
        private Label _timerLabel;
        private VisualElement _timerFill;
        private Label _phaseLabel;
        private VisualElement _interactionPrompt;
        private Label _interactionText;
        private ScrollView _ordersList;
        private VisualElement _mobileControls;
        private Button _jumpButton;
        private Button _attackButton;
        private Button _specialButton;
        private Button _interactButton;

        private EventCallback<ClickEvent> _jumpClick;
        private EventCallback<ClickEvent> _attackClick;
        private EventCallback<ClickEvent> _specialClick;
        private EventCallback<ClickEvent> _interactClick;
        private bool _ordersDirty;

        protected override void OnInitialize()
        {
            _scoreLabel = GetElement<Label>("gameplay-score");
            _timerLabel = GetElement<Label>("gameplay-timer");
            _timerFill = GetElement<VisualElement>("gameplay-timer-fill");
            _phaseLabel = GetElement<Label>("gameplay-phase");
            _interactionPrompt = GetElement<VisualElement>("interaction-prompt");
            _interactionText = GetElement<Label>("interaction-text");
            _ordersList = GetElement<ScrollView>("orders-list");
            _mobileControls = GetElement<VisualElement>("mobile-controls");
            _jumpButton = GetElement<Button>("mobile-jump");
            _attackButton = GetElement<Button>("mobile-attack");
            _specialButton = GetElement<Button>("mobile-special");
            _interactButton = GetElement<Button>("mobile-interact");

            BindViewModel();
            BindButtons();
        }

        protected override void OnShow()
        {
            _viewModel?.StartTracking();
            _ordersDirty = true;
            RenderOrders();
        }

        protected override void OnHide()
        {
            _viewModel?.StopTracking();
        }

        public override void Update(float deltaTime)
        {
            if (!IsVisible) return;

            _viewModel?.Update(deltaTime);
            if (_ordersDirty)
            {
                RenderOrders();
            }
        }

        protected override void OnDispose()
        {
            if (_jumpButton != null && _jumpClick != null) _jumpButton.UnregisterCallback(_jumpClick);
            if (_attackButton != null && _attackClick != null) _attackButton.UnregisterCallback(_attackClick);
            if (_specialButton != null && _specialClick != null) _specialButton.UnregisterCallback(_specialClick);
            if (_interactButton != null && _interactClick != null) _interactButton.UnregisterCallback(_interactClick);
            _viewModel?.Dispose();
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.ScoreText.Bind(value =>
            {
                if (_scoreLabel != null) _scoreLabel.text = value;
            });

            _viewModel.TimerText.Bind(value =>
            {
                if (_timerLabel != null) _timerLabel.text = value;
            });

            _viewModel.TimerFill.Bind(value =>
            {
                if (_timerFill != null)
                {
                    _timerFill.style.width = new Length(value * 100f, LengthUnit.Percent);
                }
            });

            _viewModel.PhaseText.Bind(value =>
            {
                if (_phaseLabel != null) _phaseLabel.text = value;
            });

            _viewModel.InteractionVisible.Bind(visible =>
            {
                if (_interactionPrompt != null)
                {
                    _interactionPrompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            _viewModel.InteractionText.Bind(value =>
            {
                if (_interactionText != null) _interactionText.text = value;
            });

            _viewModel.MobileControlsVisible.Bind(visible =>
            {
                if (_mobileControls != null)
                {
                    _mobileControls.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            _viewModel.OrdersVersion.Bind(_ => _ordersDirty = true);
        }

        private void BindButtons()
        {
            _jumpClick = _ => _viewModel?.TriggerJump();
            _attackClick = _ => _viewModel?.TriggerAttack();
            _specialClick = _ => _viewModel?.TriggerSpecial();
            _interactClick = _ => _viewModel?.TriggerInteract();

            _jumpButton?.RegisterCallback(_jumpClick);
            _attackButton?.RegisterCallback(_attackClick);
            _specialButton?.RegisterCallback(_specialClick);
            _interactButton?.RegisterCallback(_interactClick);
        }

        private void RenderOrders()
        {
            if (_ordersList == null || _viewModel == null) return;

            _ordersList.Clear();

            foreach (GameplayHudOrderItem order in _viewModel.GetActiveOrders())
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("gameplay-order-row");

                Label title = new Label(order.Title);
                title.AddToClassList("gameplay-order-title");
                row.Add(title);

                Label meta = new Label($"{FormatTime(order.TimeRemaining)}  |  {order.PointValue} pts");
                meta.AddToClassList("gameplay-order-meta");
                row.Add(meta);

                _ordersList.Add(row);
            }

            _ordersDirty = false;
        }

        private static string FormatTime(float timeRemaining)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
