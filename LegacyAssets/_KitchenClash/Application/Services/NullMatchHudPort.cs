using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// No-op HUD port for root/menu scopes when no match is active.
    /// MatchLifetimeScope overrides with the real <see cref="IMatchHudPort"/> adapter.
    /// </summary>
    public sealed class NullMatchHudPort : IMatchHudPort
    {
        public static readonly NullMatchHudPort Instance = new();

        public ulong? LocalClientId => null;
        public int LocalTeamId => -1;
        public float TimeRemaining => 0f;
        public bool IsTimerRunning => false;
        public GamePhase CurrentPhase => GamePhase.Waiting;
        public bool HasMatchResult => false;
        public MatchResultSnapshot CurrentMatchResult => MatchResultSnapshot.None;

        public event Action<ulong, int> PlayerScoreUpdated
        {
            add { }
            remove { }
        }

        public event Action<float> TimeUpdated
        {
            add { }
            remove { }
        }

        public event Action TimerExpired
        {
            add { }
            remove { }
        }

        public event Action<GamePhase, GamePhase> PhaseChanged
        {
            add { }
            remove { }
        }

        public event Action<MatchResultSnapshot, MatchResultSnapshot> MatchResultChanged
        {
            add { }
            remove { }
        }

        public event Action<RecipeOrderState> OrderCreated
        {
            add { }
            remove { }
        }

        public event Action<RecipeOrderState> OrderCompleted
        {
            add { }
            remove { }
        }

        public event Action<RecipeOrderState> OrderExpired
        {
            add { }
            remove { }
        }

        public void Refresh()
        {
        }

        public int GetLocalPlayerScore() => 0;
        public int GetTeamScore(int teamId) => 0;
        public IReadOnlyList<RecipeOrderState> GetActiveOrders() => Array.Empty<RecipeOrderState>();
        public string GetRecipeDisplayName(int recipeId) => null;

        public bool TryGetInteractionPrompt(out string prompt)
        {
            prompt = string.Empty;
            return false;
        }

        public void Subscribe()
        {
        }

        public void Unsubscribe()
        {
        }
    }
}
