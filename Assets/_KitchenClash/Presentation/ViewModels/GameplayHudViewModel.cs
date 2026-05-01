using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.Controls;
using KitchenClash.Infrastructure.Network.Cooking;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Application.Services;
using UnityEngine;
using KitchenClash.Application.State;
using KitchenClash.Infrastructure.States;

namespace KitchenClash.Presentation.ViewModels
{
    public class GameplayHudViewModel : BaseViewModel
    {
        private const float DefaultRoundDuration = 300f;

        private readonly IMatchContext _matchContext;
        private readonly IGameStateManager _stateManager;
        private readonly Dictionary<int, RecipeOrderState> _orders = new();
        private NetworkScoreManager _networkScoreManager;
        private RoundTimer _roundTimer;
        private GamePhaseSync _gamePhaseSync;
        private MatchResultSync _matchResultSync;
        private OrderManager _orderManager;
        private PlayerController _localPlayer;
        // private MobileControlsManager _mobileControlsManager; // Removed: not in IMatchContext
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

        public GameplayHudViewModel(IMatchContext matchContext, IGameStateManager stateManager)
        {
            _matchContext = matchContext;
            _stateManager = stateManager;
        }

        public IReadOnlyList<GameplayHudOrderItem> GetActiveOrders()
        {
            List<GameplayHudOrderItem> items = new();

            foreach (RecipeOrderState order in _orders.Values.OrderBy(order => order.OrderId))
            {
                if (order.IsCompleted || order.IsExpired) continue;

                Recipe recipe = _orderManager?.GetRecipeById(order.RecipeId);
                float elapsed = Mathf.Max(0f, Time.time - order.CreationTime);
                float remaining = Mathf.Max(0f, order.TimeLimit - elapsed);

                items.Add(new GameplayHudOrderItem
                {
                    OrderId = order.OrderId,
                    Title = recipe?.DisplayName?.ToUpperInvariant() ?? $"ORDER {order.OrderId}",
                    TimeRemaining = remaining,
                    PointValue = order.PointValue
                });
            }

            return items;
        }

        public void StartTracking()
        {
            if (_isTracking) return;

            _isTracking = true;
            _hasTriggeredGameOver = false;
            TryResolveRuntimeDependencies();
            SeedState();
            SubscribeToEvents();
        }

        public void StopTracking()
        {
            if (!_isTracking) return;

            UnsubscribeFromEvents();
            _isTracking = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isTracking) return;

            TryResolveRuntimeDependencies();
            UpdateInteractionPrompt();
            UpdateTimerFallback(deltaTime);
        }

        public void TriggerJump() { /* MobileControlsManager removed */ }
        public void TriggerAttack() { /* MobileControlsManager removed */ }
        public void TriggerSpecial() { /* MobileControlsManager removed */ }
        public void TriggerInteract() { /* MobileControlsManager removed */ }

        public override void Dispose()
        {
            StopTracking();
            base.Dispose();
        }

        private void TryResolveRuntimeDependencies()
        {
            bool didResolveDependency = false;
            _matchContext.Refresh();

            if (_networkScoreManager == null)
            {
                _networkScoreManager = _matchContext.NetworkScoreManager;
                didResolveDependency |= _networkScoreManager != null;
            }

            if (_roundTimer == null)
            {
                _roundTimer = _matchContext.RoundTimer;
                didResolveDependency |= _roundTimer != null;
            }

            if (_gamePhaseSync == null)
            {
                _gamePhaseSync = _matchContext.GamePhaseSync;
                didResolveDependency |= _gamePhaseSync != null;
            }

            if (_matchResultSync == null)
            {
                _matchResultSync = _matchContext.MatchResultSync;
                didResolveDependency |= _matchResultSync != null;
            }

            if (_orderManager == null)
            {
                _orderManager = _matchContext.OrderManager;
                didResolveDependency |= _orderManager != null;
            }

            if (_localPlayer == null)
            {
                _localPlayer = _matchContext.LocalPlayer;
                didResolveDependency |= _localPlayer != null;
            }

            MobileControlsVisible.Value = UnityEngine.Application.isMobilePlatform;

            if (didResolveDependency && _isTracking)
            {
                SubscribeToEvents();
                SeedState();
            }
        }

        private void SeedState()
        {
            if (_networkScoreManager != null && _matchContext.LocalClientId.HasValue)
            {
                int score = _networkScoreManager.GetPlayerScore(_matchContext.LocalClientId.Value);
                ScoreText.Value = $"Score: {score}";
            }

            if (_roundTimer != null)
            {
                _localTimer = Mathf.Max(0f, _roundTimer.TimeRemaining);
                UpdateTimerUi(_localTimer);
            }
            else
            {
                UpdateTimerUi(_localTimer);
            }

            if (_gamePhaseSync != null)
            {
                HandlePhaseChanged(_gamePhaseSync.CurrentPhase, _gamePhaseSync.CurrentPhase);
            }

            TryTransitionToGameOver();

            _orders.Clear();
            if (_orderManager != null)
            {
                foreach (RecipeOrderState order in _orderManager.GetActiveOrders())
                {
                    _orders[order.OrderId] = order;
                }
            }

            OrdersVersion.Value++;
            UpdateInteractionPrompt();
        }

        private void SubscribeToEvents()
        {
            if (_networkScoreManager != null)
            {
                _networkScoreManager.OnPlayerScoreUpdated -= HandlePlayerScoreUpdated;
                _networkScoreManager.OnPlayerScoreUpdated += HandlePlayerScoreUpdated;
            }

            if (_roundTimer != null)
            {
                _roundTimer.OnTimeUpdated -= HandleTimeUpdated;
                _roundTimer.OnTimeUpdated += HandleTimeUpdated;
                _roundTimer.OnTimerExpired -= HandleTimerExpired;
                _roundTimer.OnTimerExpired += HandleTimerExpired;
            }

            if (_gamePhaseSync != null)
            {
                _gamePhaseSync.OnPhaseChanged -= HandlePhaseChanged;
                _gamePhaseSync.OnPhaseChanged += HandlePhaseChanged;
            }

            if (_matchResultSync != null)
            {
                _matchResultSync.OnResultChanged -= HandleMatchResultChanged;
                _matchResultSync.OnResultChanged += HandleMatchResultChanged;
            }

            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated -= HandleOrderCreated;
                _orderManager.OnOrderCreated += HandleOrderCreated;
                _orderManager.OnOrderCompleted -= HandleOrderResolved;
                _orderManager.OnOrderCompleted += HandleOrderResolved;
                _orderManager.OnOrderExpired -= HandleOrderResolved;
                _orderManager.OnOrderExpired += HandleOrderResolved;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_networkScoreManager != null)
            {
                _networkScoreManager.OnPlayerScoreUpdated -= HandlePlayerScoreUpdated;
            }

            if (_roundTimer != null)
            {
                _roundTimer.OnTimeUpdated -= HandleTimeUpdated;
                _roundTimer.OnTimerExpired -= HandleTimerExpired;
            }

            if (_gamePhaseSync != null)
            {
                _gamePhaseSync.OnPhaseChanged -= HandlePhaseChanged;
            }

            if (_matchResultSync != null)
            {
                _matchResultSync.OnResultChanged -= HandleMatchResultChanged;
            }

            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated -= HandleOrderCreated;
                _orderManager.OnOrderCompleted -= HandleOrderResolved;
                _orderManager.OnOrderExpired -= HandleOrderResolved;
            }
        }

        private void HandlePlayerScoreUpdated(ulong playerId, int score)
        {
            if (!_matchContext.LocalClientId.HasValue || playerId != _matchContext.LocalClientId.Value) return;
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

        private void HandleMatchResultChanged(MatchResultState previousResult, MatchResultState newResult)
        {
            TryTransitionToGameOver();
        }

        private void TryTransitionToGameOver()
        {
            if (_hasTriggeredGameOver) return;
            if (_gamePhaseSync?.CurrentPhase != GamePhase.GameOver) return;
            if (_matchResultSync == null || !_matchResultSync.HasResult) return;

            _hasTriggeredGameOver = true;
            _stateManager?.ChangeState<GameOverState>();
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
            if (_roundTimer != null && _roundTimer.IsRunning) return;
            if (_localTimer <= 0f) return;

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
            if (_localPlayer == null)
            {
                InteractionVisible.Value = false;
                InteractionText.Value = string.Empty;
                return;
            }

            Ray ray = new Ray(_localPlayer.transform.position, _localPlayer.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(_localPlayer))
                {
                    InteractionVisible.Value = true;
                    InteractionText.Value = interactable.GetInteractionPrompt();
                    return;
                }
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
