using System.Collections.Generic;
using System.Linq;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using Playcenter.GameFlow;
using UnityEngine;

namespace KitchenClash.Presentation.ViewModels
{
    public class GameplayHudViewModel : BaseViewModel
    {
        private const float DefaultRoundDuration = 300f;

        private readonly IMatchHudPort _matchHud;
        private readonly IAppFlow _appFlow;
        private readonly Dictionary<int, RecipeOrderState> _orders = new();
        private bool _isTracking;
        private bool _hasTriggeredGameOver;
        private float _localTimer = DefaultRoundDuration;

        public BindableProperty<string> ScoreText { get; } = new("Score: 0");
        public BindableProperty<string> TimerText { get; } = new("05:00");
        public BindableProperty<float> TimerFill { get; } = new(1f);
        public BindableProperty<string> PhaseText { get; } = new("WAITING");
        public BindableProperty<bool> InteractionVisible { get; } = new(false);
        public BindableProperty<string> InteractionText { get; } = new(string.Empty);
        public BindableProperty<bool> MobileControlsVisible { get; } = new(false);
        public BindableProperty<int> OrdersVersion { get; } = new(0);

        public GameplayHudViewModel(IMatchHudPort matchHud, IAppFlow appFlow)
        {
            _matchHud = matchHud;
            _appFlow = appFlow;
        }

        public IReadOnlyList<GameplayHudOrderItem> GetActiveOrders()
        {
            List<GameplayHudOrderItem> items = new();

            foreach (RecipeOrderState order in _orders.Values.OrderBy(o => o.OrderId))
            {
                if (order.IsCompleted || order.IsExpired)
                {
                    continue;
                }

                string displayName = _matchHud?.GetRecipeDisplayName(order.RecipeId);
                float elapsed = Mathf.Max(0f, Time.time - order.CreationTime);
                float remaining = Mathf.Max(0f, order.TimeLimit - elapsed);

                items.Add(new GameplayHudOrderItem
                {
                    OrderId = order.OrderId,
                    Title = !string.IsNullOrEmpty(displayName)
                        ? displayName.ToUpperInvariant()
                        : $"ORDER {order.OrderId}",
                    TimeRemaining = remaining,
                    PointValue = order.PointValue
                });
            }

            return items;
        }

        public void StartTracking()
        {
            if (_isTracking)
            {
                return;
            }

            _isTracking = true;
            _hasTriggeredGameOver = false;
            _matchHud?.Subscribe();
            SeedState();
            SubscribeToEvents();
        }

        public void StopTracking()
        {
            if (!_isTracking)
            {
                return;
            }

            UnsubscribeFromEvents();
            _matchHud?.Unsubscribe();
            _isTracking = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isTracking)
            {
                return;
            }

            _matchHud?.Refresh();
            UpdateInteractionPrompt();
            UpdateTimerFallback(deltaTime);
        }

        public void TriggerJump() { }
        public void TriggerAttack() { }
        public void TriggerSpecial() { }
        public void TriggerInteract() { }

        public override void Dispose()
        {
            StopTracking();
            base.Dispose();
        }

        private void SeedState()
        {
            if (_matchHud == null)
            {
                return;
            }

            ScoreText.Value = $"Score: {_matchHud.GetLocalPlayerScore()}";

            if (_matchHud.IsTimerRunning || _matchHud.TimeRemaining > 0f)
            {
                _localTimer = Mathf.Max(0f, _matchHud.TimeRemaining);
            }

            UpdateTimerUi(_localTimer);
            HandlePhaseChanged(_matchHud.CurrentPhase, _matchHud.CurrentPhase);
            TryTransitionToGameOver();

            _orders.Clear();
            foreach (RecipeOrderState order in _matchHud.GetActiveOrders())
            {
                _orders[order.OrderId] = order;
            }

            OrdersVersion.Value++;
            MobileControlsVisible.Value = UnityEngine.Application.isMobilePlatform;
            UpdateInteractionPrompt();
        }

        private void SubscribeToEvents()
        {
            if (_matchHud == null)
            {
                return;
            }

            _matchHud.PlayerScoreUpdated -= HandlePlayerScoreUpdated;
            _matchHud.PlayerScoreUpdated += HandlePlayerScoreUpdated;
            _matchHud.TimeUpdated -= HandleTimeUpdated;
            _matchHud.TimeUpdated += HandleTimeUpdated;
            _matchHud.TimerExpired -= HandleTimerExpired;
            _matchHud.TimerExpired += HandleTimerExpired;
            _matchHud.PhaseChanged -= HandlePhaseChanged;
            _matchHud.PhaseChanged += HandlePhaseChanged;
            _matchHud.MatchResultChanged -= HandleMatchResultChanged;
            _matchHud.MatchResultChanged += HandleMatchResultChanged;
            _matchHud.OrderCreated -= HandleOrderCreated;
            _matchHud.OrderCreated += HandleOrderCreated;
            _matchHud.OrderCompleted -= HandleOrderResolved;
            _matchHud.OrderCompleted += HandleOrderResolved;
            _matchHud.OrderExpired -= HandleOrderResolved;
            _matchHud.OrderExpired += HandleOrderResolved;
        }

        private void UnsubscribeFromEvents()
        {
            if (_matchHud == null)
            {
                return;
            }

            _matchHud.PlayerScoreUpdated -= HandlePlayerScoreUpdated;
            _matchHud.TimeUpdated -= HandleTimeUpdated;
            _matchHud.TimerExpired -= HandleTimerExpired;
            _matchHud.PhaseChanged -= HandlePhaseChanged;
            _matchHud.MatchResultChanged -= HandleMatchResultChanged;
            _matchHud.OrderCreated -= HandleOrderCreated;
            _matchHud.OrderCompleted -= HandleOrderResolved;
            _matchHud.OrderExpired -= HandleOrderResolved;
        }

        private void HandlePlayerScoreUpdated(ulong playerId, int score)
        {
            if (!_matchHud.LocalClientId.HasValue || playerId != _matchHud.LocalClientId.Value)
            {
                return;
            }

            ScoreText.Value = $"Score: {score}";
        }

        private void HandleTimeUpdated(float timeRemaining)
        {
            _localTimer = Mathf.Max(0f, timeRemaining);
            UpdateTimerUi(_localTimer);
        }

        private void HandleTimerExpired()
        {
            _localTimer = 0f;
            UpdateTimerUi(0f);
            PhaseText.Value = GamePhase.GameOver.ToString().ToUpperInvariant();
        }

        private void HandlePhaseChanged(GamePhase previousPhase, GamePhase newPhase)
        {
            PhaseText.Value = newPhase.ToString().ToUpperInvariant();
            TryTransitionToGameOver();
        }

        private void HandleMatchResultChanged(MatchResultSnapshot previousResult, MatchResultSnapshot newResult)
        {
            TryTransitionToGameOver();
        }

        private void TryTransitionToGameOver()
        {
            if (_hasTriggeredGameOver || _matchHud == null)
            {
                return;
            }

            if (_matchHud.CurrentPhase != GamePhase.GameOver)
            {
                return;
            }

            if (!_matchHud.HasMatchResult)
            {
                return;
            }

            _hasTriggeredGameOver = true;

            MatchResultSnapshot currentResult = _matchHud.CurrentMatchResult;
            int localTeamId = _matchHud.LocalTeamId;
            bool won = !currentResult.IsDraw && currentResult.WinningTeamId == localTeamId;

            _appFlow?.NotifyMatchCompleted(new MatchResultInfo
            {
                IsDraw = currentResult.IsDraw,
                WinningTeamId = currentResult.WinningTeamId,
                Won = won,
                LocalTeamId = localTeamId,
                LocalTeamScore = _matchHud.GetTeamScore(localTeamId)
            });
        }

        private void HandleOrderCreated(RecipeOrderState order)
        {
            _orders[order.OrderId] = order;
            OrdersVersion.Value++;
        }

        private void HandleOrderResolved(RecipeOrderState order)
        {
            _orders.Remove(order.OrderId);
            OrdersVersion.Value++;
        }

        private void UpdateTimerFallback(float deltaTime)
        {
            if (_matchHud != null && _matchHud.IsTimerRunning)
            {
                return;
            }

            if (_localTimer <= 0f)
            {
                return;
            }

            _localTimer = Mathf.Max(0f, _localTimer - deltaTime);
            UpdateTimerUi(_localTimer);
        }

        private void UpdateTimerUi(float gameTime)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            TimerText.Value = $"{minutes:00}:{seconds:00}";
            TimerFill.Value = Mathf.Clamp01(gameTime / DefaultRoundDuration);
        }

        private void UpdateInteractionPrompt()
        {
            if (_matchHud != null && _matchHud.TryGetInteractionPrompt(out string prompt))
            {
                InteractionVisible.Value = true;
                InteractionText.Value = prompt;
                return;
            }

            InteractionVisible.Value = false;
            InteractionText.Value = string.Empty;
        }
    }

    public class GameplayHudOrderItem
    {
        public int OrderId { get; set; }
        public string Title { get; set; }
        public float TimeRemaining { get; set; }
        public int PointValue { get; set; }
    }
}
