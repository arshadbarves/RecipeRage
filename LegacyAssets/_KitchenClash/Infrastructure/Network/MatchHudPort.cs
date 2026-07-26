using System;
using System.Collections.Generic;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Cooking;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Adapts <see cref="IMatchContext"/> NetworkBehaviours into the Application HUD port.
    /// </summary>
    public sealed class MatchHudPort : IMatchHudPort, IDisposable
    {
        private readonly IMatchContext _matchContext;
        private bool _subscribed;

        private NetworkScoreManager _networkScoreManager;
        private RoundTimer _roundTimer;
        private GamePhaseSync _gamePhaseSync;
        private MatchResultSync _matchResultSync;
        private OrderManager _orderManager;
        private PlayerController _localPlayer;

        public MatchHudPort(IMatchContext matchContext)
        {
            _matchContext = matchContext ?? throw new ArgumentNullException(nameof(matchContext));
        }

        public ulong? LocalClientId => _matchContext.LocalClientId;
        public int LocalTeamId => _matchContext.LocalTeamId;

        public float TimeRemaining => _roundTimer != null ? _roundTimer.TimeRemaining : 0f;
        public bool IsTimerRunning => _roundTimer != null && _roundTimer.IsRunning;

        public GamePhase CurrentPhase =>
            _gamePhaseSync != null ? _gamePhaseSync.CurrentPhase : GamePhase.Waiting;

        public bool HasMatchResult => _matchResultSync != null && _matchResultSync.HasResult;

        public MatchResultSnapshot CurrentMatchResult =>
            _matchResultSync != null
                ? ToSnapshot(_matchResultSync.CurrentResult)
                : MatchResultSnapshot.None;

        public event Action<ulong, int> PlayerScoreUpdated;
        public event Action<float> TimeUpdated;
        public event Action TimerExpired;
        public event Action<GamePhase, GamePhase> PhaseChanged;
        public event Action<MatchResultSnapshot, MatchResultSnapshot> MatchResultChanged;
        public event Action<RecipeOrderState> OrderCreated;
        public event Action<RecipeOrderState> OrderCompleted;
        public event Action<RecipeOrderState> OrderExpired;

        public void Refresh()
        {
            _matchContext.Refresh();

            bool wasSubscribed = _subscribed;
            if (wasSubscribed)
            {
                UnsubscribeInternal();
            }

            _networkScoreManager = _matchContext.NetworkScoreManager;
            _roundTimer = _matchContext.RoundTimer;
            _gamePhaseSync = _matchContext.GamePhaseSync;
            _matchResultSync = _matchContext.MatchResultSync;
            _orderManager = _matchContext.OrderManager;
            _localPlayer = _matchContext.LocalPlayer;

            if (wasSubscribed)
            {
                SubscribeInternal();
            }
        }

        public int GetLocalPlayerScore()
        {
            if (_networkScoreManager == null || !LocalClientId.HasValue)
            {
                return 0;
            }

            return _networkScoreManager.GetPlayerScore(LocalClientId.Value);
        }

        public int GetTeamScore(int teamId)
        {
            return _matchContext.ScoreManager != null
                ? _matchContext.ScoreManager.GetScore(teamId)
                : 0;
        }

        public IReadOnlyList<RecipeOrderState> GetActiveOrders()
        {
            if (_orderManager == null)
            {
                return Array.Empty<RecipeOrderState>();
            }

            return _orderManager.GetActiveOrders();
        }

        public string GetRecipeDisplayName(int recipeId)
        {
            Recipe recipe = _orderManager?.GetRecipeById(recipeId);
            return recipe != null ? recipe.DisplayName : null;
        }

        public bool TryGetInteractionPrompt(out string prompt)
        {
            prompt = string.Empty;
            if (_localPlayer == null)
            {
                return false;
            }

            Ray ray = new Ray(_localPlayer.transform.position, _localPlayer.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                return false;
            }

            IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
            if (interactable == null || !interactable.CanInteract(_localPlayer))
            {
                return false;
            }

            prompt = interactable.GetInteractionPrompt();
            return true;
        }

        public void Subscribe()
        {
            Refresh();
            if (_subscribed)
            {
                return;
            }

            SubscribeInternal();
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            UnsubscribeInternal();
            _subscribed = false;
        }

        public void Dispose() => Unsubscribe();

        private void SubscribeInternal()
        {
            if (_networkScoreManager != null)
            {
                _networkScoreManager.OnPlayerScoreUpdated += HandlePlayerScoreUpdated;
            }

            if (_roundTimer != null)
            {
                _roundTimer.OnTimeUpdated += HandleTimeUpdated;
                _roundTimer.OnTimerExpired += HandleTimerExpired;
            }

            if (_gamePhaseSync != null)
            {
                _gamePhaseSync.OnPhaseChanged += HandlePhaseChanged;
            }

            if (_matchResultSync != null)
            {
                _matchResultSync.OnResultChanged += HandleMatchResultChanged;
            }

            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated += HandleOrderCreated;
                _orderManager.OnOrderCompleted += HandleOrderCompleted;
                _orderManager.OnOrderExpired += HandleOrderExpired;
            }
        }

        private void UnsubscribeInternal()
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
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
                _orderManager.OnOrderExpired -= HandleOrderExpired;
            }
        }

        private void HandlePlayerScoreUpdated(ulong playerId, int score) =>
            PlayerScoreUpdated?.Invoke(playerId, score);

        private void HandleTimeUpdated(float timeRemaining) =>
            TimeUpdated?.Invoke(timeRemaining);

        private void HandleTimerExpired() => TimerExpired?.Invoke();

        private void HandlePhaseChanged(GamePhase previous, GamePhase next) =>
            PhaseChanged?.Invoke(previous, next);

        private void HandleMatchResultChanged(MatchResultState previous, MatchResultState next) =>
            MatchResultChanged?.Invoke(ToSnapshot(previous), ToSnapshot(next));

        private void HandleOrderCreated(RecipeOrderState order) => OrderCreated?.Invoke(order);

        private void HandleOrderCompleted(RecipeOrderState order) => OrderCompleted?.Invoke(order);

        private void HandleOrderExpired(RecipeOrderState order) => OrderExpired?.Invoke(order);

        internal static MatchResultSnapshot ToSnapshot(MatchResultState state)
        {
            if (!state.HasResult)
            {
                return MatchResultSnapshot.None;
            }

            return new MatchResultSnapshot(
                state.HasResult,
                state.WinningTeamId,
                state.WinningScore,
                state.IsDraw,
                state.EndReason);
        }
    }
}
